using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Yarhl.IO;

namespace LibARMP
{
    public static class ArmpFileWriter
    {

        internal static Dictionary<Type, MethodInfo> WriteTypeCache = new Dictionary<Type, MethodInfo>();

        private static void WriteARMP (ARMP armp, DataStream datastream)
        {
            var writer = new DataWriter(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
                DefaultEncoding = System.Text.Encoding.UTF8,
            };
            if (armp.IsOldEngine)
            {
                writer.Endianness = EndiannessMode.BigEndian;
                writer.DefaultEncoding = System.Text.Encoding.GetEncoding(932);
            }

            writer.Write("armp", false); //Magic
            if (armp.IsOldEngine) writer.Write(0x02010000); //Endianness identifier for OE
            else writer.WriteTimes(0x00, 0x4);
            if (armp.IsOldEngine) //Version and Revision are flipped on different endianess. Presumably both values are read together as an int32
            {
                writer.Write(armp.Version);
                writer.Write(armp.Revision);
            }
            else
            {
                writer.Write(armp.Revision);
                writer.Write(armp.Version);
            }
            writer.WriteTimes(0x00, 0x4); //File size (only used in OE, placeholder for now)

            if (armp.IsOldEngine)
            {
                //TODO
                WriteTableOE(writer, armp.MainTable);
            }
            else
            {
                writer.WriteTimes(0x00, 0x10); //Dummy main table pointer and padding
                uint mainPtr = WriteTableRecursive(writer, armp.MainTable);
                writer.Stream.Seek(0x10);
                writer.Write(mainPtr);
            }
        }



        /// <summary>
        /// Writes an ARMP to a file.
        /// </summary>
        /// <param name="armp">The ARMP to write.</param>
        /// <param name="path">The destination file path.</param>
        public static void WriteARMPToFile (ARMP armp, string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Write))
            {
                WriteARMP(armp, datastream);
            }
        }



        /// <summary>
        /// Writes an ARMP to a stream.
        /// </summary>
        /// <param name="armp">The ARMP to write.</param>
        public static Stream WriteARMPToStream (ARMP armp)
        {
            MemoryStream stream = new MemoryStream();
            DataStream tempds = DataStreamFactory.FromMemory();
            WriteARMP(armp, tempds);
            tempds.WriteTo(stream);
            return stream;
        }



        /// <summary>
        /// Writes an ARMP to a byte array.
        /// </summary>
        /// <param name="armp"></param>
        public static byte[] WriteARMPToArray (ARMP armp)
        {
            DataStream tempds = DataStreamFactory.FromMemory();
            WriteARMP(armp, tempds);
            return tempds.ToArray();
        }



        /// <summary>
        /// Writes a value of type T to the DataStream.
        /// </summary>
        /// <typeparam name="T">The type to read.</typeparam>
        /// <param name="writer">The DataWriter.</param>
        /// <param name="value">The value of type T to write.</param>
        private static void WriteType<T> (DataWriter writer, object value)
        {
            writer.WriteOfType<T>((T)value);
        }



        /// <summary>
        /// Writes an OE table to the DataStream.
        /// </summary>
        /// <param name="writer">The DataWriter.</param>
        /// <param name="table">The ArmpTable to write.</param>
        private static void WriteTableOE (DataWriter writer, ArmpTableMain table)
        {
            long baseOffset = writer.Stream.Position;
            writer.WriteTimes(0x00, 0x40); //Placeholder table

            writer.Stream.PushToPosition(baseOffset);
            writer.Write(table.Entries.Count);
            //TODO Ishin variant
            writer.Stream.PopPosition();

            int ptr = 0;
            //Entry Validity
            if (table.TableInfo.HasEntryValidity)
            {
                //TODO Ishin
                List<bool> entryValidity = new List<bool>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    entryValidity.Add(entry.IsValid);
                }
                table.EntryValidity = entryValidity;
                ptr = (int)writer.Stream.Position;
                Util.WriteBooleanBitmask(writer, table.EntryValidity);
                writer.WritePadding(0x00, 8);
                writer.Stream.PushToPosition(baseOffset + 0xC);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Entry Names
            if (table.TableInfo.HasEntryNames)
            {
                List<string> entryNames = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    entryNames.Add(entry.Name);
                }
                table.EntryNames = entryNames;
                ptr = Util.WriteText(writer, table.EntryNames);
                writer.Stream.PushToPosition(baseOffset + 0x8);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Names and Count
            if (table.TableInfo.HasColumnNames)
            {
                ptr = Util.WriteText(writer, table.GetColumnNames());
                writer.Stream.PushToPosition(baseOffset + 0x10);
                writer.Write(table.Columns.Count);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Types
            ptr = (int)writer.Stream.Position;
            foreach (ArmpTableColumn column in table.Columns)
            {
                int typeID = -1;
                if (column.ColumnType != null)
                {
                    typeID = DataTypes.TypesOEReverse[column.ColumnType];
                }
                writer.Write(typeID);
            }
            writer.Stream.PushToPosition(baseOffset + 0x18);
            writer.Write(ptr);
            writer.Stream.PopPosition();

            //Column Metadata
            if (table.TableInfo.HasColumnMetadata)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    writer.Write(column.UnknownMetadata0x40);
                }
                writer.Stream.PushToPosition(baseOffset + 0x24);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Text
            if (table.TableInfo.HasText)
            {
                //Force an update of the text table.

                //Get the columns of type string 
                List<ArmpTableColumn> stringTypeColumns = new List<ArmpTableColumn>();
                foreach (ArmpTableColumn column in table.Columns)
                {
                    if (column.ColumnType == DataTypes.Types["string"])
                    {
                        stringTypeColumns.Add(column);
                    }
                }

                List<string> textList = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    foreach (ArmpTableColumn column in stringTypeColumns)
                    {
                        string str = (string)entry.GetValueFromColumn(column.Name);
                        if (!textList.Contains(str) && str != null) textList.Add(str);
                    }
                }
                table.Text = textList;

                ptr = Util.WriteText(writer, table.Text);
                writer.Stream.PushToPosition(baseOffset + 0x28);
                writer.Write(ptr);
                writer.Stream.PopPosition();
                //Text count
                writer.Stream.PushToPosition(baseOffset + 0x2C);
                writer.Write(table.Text.Count);
                writer.Stream.PopPosition();
            }

            //Column Contents
            List<int> columnValueOffsets = new List<int>();
            foreach (ArmpTableColumn column in table.Columns)
            {
                if (column.IsValid != null && column.IsValid == false)
                {
                    columnValueOffsets.Add(0);
                }
                else
                {
                    columnValueOffsets.Add((int)writer.Stream.Position);
                    List<bool> boolList = new List<bool>(); //Init list in case it is a boolean column
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        if (column.ColumnType == DataTypes.Types["string"])
                        {
                            if (entry.GetValueFromColumn(column.Name) != null)
                            {
                                int index = table.Text.IndexOf((string)entry.GetValueFromColumn(column.Name));
                                writer.WriteOfType<Int16>((Int16)index);
                            }
                            else
                            {
                                writer.WriteOfType<Int16>(-1);
                            }
                        }

                        else if (column.ColumnType == DataTypes.Types["boolean"])
                        {
                            boolList.Add((bool)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.ColumnType == DataTypes.Types["uint8"])
                        {
                            writer.Write((byte)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.ColumnType == DataTypes.Types["int8"])
                        {
                            writer.Write((sbyte)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.ColumnType == DataTypes.Types["uint16"])
                        {
                            writer.Write((UInt16)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.ColumnType == DataTypes.Types["int16"])
                        {
                            writer.Write((Int16)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.ColumnType == DataTypes.Types["uint32"])
                        {
                            writer.Write((UInt32)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.ColumnType == DataTypes.Types["int32"])
                        {
                            writer.Write((Int32)entry.GetValueFromColumn(column.Name));
                        }

                    }

                    if (boolList.Count > 0) //Write booleans
                    {
                        Util.WriteBooleanBitmask(writer, boolList);
                    }
                }
            }

            writer.WritePadding(0x00, 4);
            int ptrColumnOffsetTable = (int)writer.Stream.Position;
            foreach (int offset in columnValueOffsets)
            {
                writer.Write(offset);
            }

            writer.Stream.PushToPosition(baseOffset + 0x1C);
            writer.Write(ptrColumnOffsetTable);
            writer.Stream.PopPosition();
        }



        /// <summary>
        /// Recursively writes tables from the lowest level upwards.
        /// </summary>
        /// <param name="writer">The DataWriter.</param>
        /// <param name="table">The ArmpTableMain to write.</param>
        /// <returns>The pointer to the table.</returns>
        private static uint WriteTableRecursive (DataWriter writer, ArmpTable table)
        {
            List<string> tableColumns = table.GetColumnNamesByType(DataTypes.Types["table"]);
            Dictionary<ArmpTable, uint> tablePointers = new Dictionary<ArmpTable, uint>();

            if (tableColumns.Count > 0)
            {
                foreach (string column in tableColumns)
                {
                    foreach (ArmpEntry entry in table.GetAllEntries())
                    {
                        try
                        {
                            ArmpTable tableValue = (ArmpTable)entry.GetValueFromColumn(column);
                            uint tableValuePtr = WriteTableRecursive(writer, tableValue);
                            tablePointers.Add(tableValue, tableValuePtr);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            uint subtablePtr = 0;
            if (table.GetType() == typeof(ArmpTableMain))
            {
                ArmpTableMain tableMain = (ArmpTableMain)table;
                if (tableMain.SubTable != null)
                {
                    subtablePtr = WriteTableRecursive(writer, tableMain.SubTable);
                    writer.WritePadding(0x00, 0x10);
                }
            }

            UInt32 pointer = (uint)writer.Stream.Position;
            WriteTable(writer, table, tablePointers);
            writer.WritePadding(0x00, 0x10);
            writer.Stream.PushToPosition(pointer + 0x3C);
            writer.Write(subtablePtr);
            writer.Stream.PopPosition();
            return pointer;
        }



        /// <summary>
        /// Writes a DE table to the DataStream.
        /// </summary>
        /// <param name="writer">The DataWriter.</param>
        /// <param name="table">The ArmpTable to write.</param>
        private static void WriteTable (DataWriter writer, ArmpTable table, Dictionary<ArmpTable, uint> tableValuePointers = null)
        {
            long baseOffset = writer.Stream.Position;
            writer.WriteTimes(0x00, 0x50); //Placeholder table

            //Entry and column counts
            writer.Stream.PushToPosition(baseOffset);
            writer.Write(table.Entries.Count);
            writer.Write(table.Columns.Count);

            //Table ID and Storage Mode
            writer.Stream.Position = baseOffset + 0x20;
            writer.Write(table.TableInfo.TableID);
            writer.Stream.Position = baseOffset + 0x23;
            writer.Write(table.TableInfo.StorageMode);
            writer.Stream.PopPosition();

            int ptr = 0;
            //Entry Validity
            if (table.TableInfo.HasEntryValidity)
            {
                List<bool> entryValidity = new List<bool>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    entryValidity.Add(entry.IsValid);
                }
                table.EntryValidity = entryValidity;
                ptr = (int)writer.Stream.Position;
                Util.WriteBooleanBitmask(writer, table.EntryValidity);
                writer.WritePadding(0x00, 0x4);
                writer.Stream.PushToPosition(baseOffset + 0x14);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }
            else
            {
                writer.Stream.PushToPosition(baseOffset + 0x14);
                writer.Write(-1);
                writer.Stream.PopPosition();
            }

            //Entry Validator
            writer.Stream.PushToPosition(baseOffset + 0xC);
            writer.Write(table.TableInfo.EntryValidator);
            writer.Stream.PopPosition();

            //Column Validity
            if (table.TableInfo.HasColumnValidity)
            {
                ptr = (int)writer.Stream.Position;
                List<bool> columnValidity = new List<bool>();
                foreach (ArmpTableColumn column in table.Columns)
                    columnValidity.Add((bool)column.IsValid);
                Util.WriteBooleanBitmask(writer, columnValidity);
                writer.WritePadding(0x00, 0x8);
                writer.Stream.PushToPosition(baseOffset + 0x38);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }
            else
            {
                writer.Stream.PushToPosition(baseOffset + 0x38);
                writer.Write(-1);
                writer.Stream.PopPosition();
            }

            //Column Validator
            writer.Stream.PushToPosition(baseOffset + 0x2C);
            writer.Write(table.TableInfo.ColumnValidator);
            writer.Stream.PopPosition();

            //Entry Names
            if (table.TableInfo.HasEntryNames)
            {
                List<string> entryNames = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    entryNames.Add(entry.Name);
                }
                table.EntryNames = entryNames;
                ptr = Util.WriteText(writer, table.EntryNames);
                writer.Stream.PushToPosition(baseOffset + 0x10);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Names
            if (table.TableInfo.HasColumnNames)
            {
                ptr = Util.WriteText(writer, table.GetColumnNames());
                writer.Stream.PushToPosition(baseOffset + 0x28);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Text
            if (table.TableInfo.HasText)
            {
                //Force an update of the table text.

                //Get the columns of type string 
                List<ArmpTableColumn> stringTypeColumns = new List<ArmpTableColumn>();
                foreach (ArmpTableColumn column in table.Columns)
                {
                    if (column.ColumnType == DataTypes.Types["string"])
                    {
                        stringTypeColumns.Add(column);
                    }
                }

                List<string> textList = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    foreach (ArmpTableColumn column in stringTypeColumns)
                    {
                        string str = (string)entry.GetValueFromColumn(column.Name);
                        if (!textList.Contains(str) && str != null) textList.Add(str);
                    }
                }
                table.Text = textList;

                ptr = Util.WriteText(writer, table.Text);
                writer.Stream.PushToPosition(baseOffset + 0x24);
                writer.Write(ptr);
                writer.Stream.PopPosition();
                //Text count
                writer.Stream.PushToPosition(baseOffset + 0x8);
                writer.Write(table.Text.Count);
                writer.Stream.PopPosition();
            }

            //Column Types
            ptr = (int)writer.Stream.Position;
            if (table.TableInfo.IsDragonEngineV2)
            {
                foreach (ArmpTableColumn column in table.Columns)
                {
                    sbyte typeID = -1;
                    if (column.ColumnType != null)
                    {
                        typeID = DataTypes.TypesV2Reverse[column.ColumnType];
                    }
                    writer.Write(typeID);
                }
            }
            else //V1. Write the aux types because they got swapped with main during read.
            {
                foreach (ArmpTableColumn column in table.Columns)
                {
                    sbyte typeID = -1;
                    if (column.ColumnTypeAux != null)
                    {
                        typeID = DataTypes.TypesV1Reverse[column.ColumnTypeAux];
                    }
                    writer.Write(typeID);
                }
            }
            writer.Stream.PushToPosition(baseOffset + 0x18);
            writer.Write(ptr);
            writer.Stream.PopPosition();
            writer.WritePadding(0x00, 0x8);

            //Set the text ptr to column types if there is no text
            if (!table.TableInfo.HasText)
            {
                writer.Stream.PushToPosition(baseOffset + 0x24);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Types Aux (V1)
            if (!table.TableInfo.IsDragonEngineV2)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    //Write the main type because it got swapped with aux during read.
                    sbyte typeID = -1;
                    if (column.ColumnType != null)
                    {
                        typeID = DataTypes.TypesV1AuxReverse[column.ColumnType];
                    }
                    writer.Write(typeID);
                }
                writer.Stream.PushToPosition(baseOffset + 0x48);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }


            //Column Contents
            if (table.TableInfo.StorageMode == 0)
            {
                List<int> columnValueOffsets = new List<int>();
                foreach (ArmpTableColumn column in table.Columns)
                {
                    if (column.IsNoData)
                    {
#if DEBUG
                        Console.WriteLine("NO DATA!!!");
#endif
                        columnValueOffsets.Add(-1);
                    }
                    else if (table.TableInfo.HasColumnValidity && column.IsValid == false)
                    {
                        columnValueOffsets.Add(0);
                    }
                    else
                    {

                        if (column.ColumnType == DataTypes.Types["table"])
                        {
                            writer.WritePadding(0x00, 0x8);
                        }
                        columnValueOffsets.Add((int)writer.Stream.Position);
                        List<bool> boolList = new List<bool>(); //Init list in case it is a boolean column
                        List<ArmpTableMain> tableList = new List<ArmpTableMain>(); //Init list in case it is a table column
                        List<int> tableOffsetList = new List<int>(); //Init list in case it is a table column

                        if (table.TableInfo.IsDragonEngineV2 && column.IsSpecial) continue;

                        foreach (ArmpEntry entry in table.Entries)
                        {
                            if (column.ColumnType == DataTypes.Types["string"])
                            {
                                if (entry.GetValueFromColumn(column.Name) != null)
                                {
                                    int index = table.Text.IndexOf((string)entry.GetValueFromColumn(column.Name));
                                    writer.Write(index);
                                }
                                else
                                {
                                    writer.Write(-1);
                                }
                            }

                            else if (column.ColumnType == DataTypes.Types["boolean"])
                            {
                                boolList.Add((bool)entry.GetValueFromColumn(column.Name));
                            }

                            else if (column.ColumnType == DataTypes.Types["table"])
                            {
                                try
                                {
                                    ulong tablePtr = tableValuePointers[(ArmpTableMain)entry.GetValueFromColumn(column.Name)];
                                    writer.Write(tablePtr);
                                }
                                catch
                                {
                                    writer.WriteTimes(0x00, 0x8);
                                }
                            }

                            else
                            {
                                Type columnType = column.ColumnType;
                                if (WriteTypeCache.ContainsKey(columnType))
                                {
                                    MethodInfo methodref = WriteTypeCache[columnType];
                                    methodref.Invoke(null, new object[] { writer, entry.GetValueFromColumn(column.Name) });
                                }
                                else
                                {
                                    MethodInfo methodinfo = typeof(ArmpFileWriter).GetMethod("WriteType", BindingFlags.NonPublic | BindingFlags.Static);
                                    MethodInfo methodref = methodinfo.MakeGenericMethod(columnType);
                                    WriteTypeCache.Add(columnType, methodref);
                                    methodref.Invoke(null, new object[] { writer, entry.GetValueFromColumn(column.Name) });
                                }
                            }
                        }

                        if (boolList.Count > 0) //Write booleans
                        {
                            Util.WriteBooleanBitmask(writer, boolList);
                        }

                    }
                }

                writer.WritePadding(0x00, 0x10);
                int ptrColumnOffsetTable = (int)writer.Stream.Position;
                foreach (int offset in columnValueOffsets)
                {
                    writer.Write(offset);
                }

                writer.Stream.PushToPosition(baseOffset + 0x1C);
                writer.Write(ptrColumnOffsetTable);
                writer.Stream.PopPosition();
            }


            else if (table.TableInfo.StorageMode == 1)
            {
                List<int> entryValueOffsets = new List<int>();
                foreach (ArmpEntry entry in table.GetAllEntries())
                {
                    entryValueOffsets.Add((int)writer.Stream.Position);

                    foreach (string columnName in table.GetColumnNames(false))
                    {
                        if (table.TableInfo.HasColumnValidity && table.IsColumnValid(columnName))
                        {
                            Type columnType = table.GetColumnDataType(columnName);

                            if (columnType == DataTypes.Types["string"])
                            {
                                if (entry.GetValueFromColumn(columnName) != null)
                                {
                                    long index = table.Text.IndexOf((string)entry.GetValueFromColumn(columnName));
                                    writer.Write(index);
                                }
                                else
                                {
                                    writer.Write((long)-1);
                                }
                            }

                            else if (columnType == DataTypes.Types["boolean"])
                            {
                                bool val = (bool)entry.GetValueFromColumn(columnName);
                                writer.Write(Convert.ToByte(val));
                            }

                            else if (columnType == DataTypes.Types["table"])
                            {
                                try
                                {
                                    ulong tablePtr = tableValuePointers[(ArmpTableMain)entry.GetValueFromColumn(columnName)];
                                    writer.Write(tablePtr);
                                }
                                catch
                                {
                                    writer.WriteTimes(0x00, 0x8);
                                }
                            }

                            else
                            {
                                if (WriteTypeCache.ContainsKey(columnType))
                                {
                                    MethodInfo methodref = WriteTypeCache[columnType];
                                    methodref.Invoke(null, new object[] { writer, entry.GetValueFromColumn(columnName) });
                                }
                                else
                                {
                                    MethodInfo methodinfo = typeof(ArmpFileWriter).GetMethod("WriteType", BindingFlags.NonPublic | BindingFlags.Static);
                                    MethodInfo methodref = methodinfo.MakeGenericMethod(columnType);
                                    WriteTypeCache.Add(columnType, methodref);
                                    methodref.Invoke(null, new object[] { writer, entry.GetValueFromColumn(columnName) });
                                }
                            }
                        }
                    }
                }
                writer.WritePadding(0x00, 0x10);
                int ptrColumnOffsetTable = (int)writer.Stream.Position;
                foreach (int offset in entryValueOffsets)
                {
                    writer.Write(offset);
                }

                writer.Stream.PushToPosition(baseOffset + 0x1C);
                writer.Write(ptrColumnOffsetTable);
                writer.Stream.PopPosition();
            }



            //Entry Indices
            if (table.TableInfo.HasEntryIndices)
            {
                ptr = (int)writer.Stream.Position;

                foreach(ArmpEntry entry in table.Entries)
                {
                    writer.Write(entry.Index);
                }
                writer.Stream.PushToPosition(baseOffset + 0x30);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Indices
            if (table.TableInfo.HasColumnIndices)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    writer.Write(column.Index);
                }
                writer.Stream.PushToPosition(baseOffset + 0x34);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Metadata
            if (table.TableInfo.HasColumnMetadata)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    writer.Write(column.UnknownMetadata0x40);
                }
                writer.Stream.PushToPosition(baseOffset + 0x40);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Entry Info Flags (v1 only)
            if (table.TableInfo.HasExtraFieldInfo && !table.TableInfo.IsDragonEngineV2)
            {
                ptr = (int)writer.Stream.Position;
                foreach(ArmpEntry entry in table.Entries)
                {
                    string bitstring = "";
                    for(int i=0; i<entry.Flags.Length; i++)
                    {
                        bitstring += Convert.ToByte(entry.Flags[i]);
                    }
                    byte value = Convert.ToByte(Util.ReverseString(bitstring), 2);
                    writer.Write(value);
                }
                writer.Stream.PushToPosition(baseOffset + 0x4C);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Empty Values
            if (table.TableInfo.HasEmptyValues)
            {
                Dictionary<int, int> offsetDictionary = new Dictionary<int, int>();
                foreach (KeyValuePair<int, List<bool>> kvp in table.EmptyValues)
                {
                    offsetDictionary.Add(kvp.Key, (int)writer.Stream.Position);
                    Util.WriteBooleanBitmask(writer, kvp.Value);
                    writer.WritePadding(0x00, 0x4);
                }

                ptr = (int)writer.Stream.Position;
                for (int i=0; i<table.TableInfo.ColumnCount; i++)
                {
                    if (table.EmptyValuesIsNegativeOffset[i])
                    {
                        writer.Write(-1);
                    }
                    else
                    {
                        if (offsetDictionary.ContainsKey(i))
                        {
                            writer.Write(offsetDictionary[i]);
                        }
                        else
                        {
                            writer.Write(0);
                        }
                    }
                }

                writer.Stream.PushToPosition(baseOffset + 0x44);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            // Column Data Types Aux (V2)
            if (table.TableInfo.IsDragonEngineV2 && table.TableInfo.HasColumnDataTypesAux)
            {
                writer.WritePadding(0x00, 0x10);
                ptr = (int)writer.Stream.Position;
                WriteColumnDataTypesAuxTable(writer, table);

                writer.Stream.PushToPosition(baseOffset + 0x48);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            // Column Unknown Metadata 0x4C
            if (table.TableInfo.IsDragonEngineV2 && table.TableInfo.HasExtraFieldInfo)
            {
                List<int> offsets = new List<int>();
                foreach (ArmpTableColumn column in table.Columns)
                {
                    if (column.IsSpecial && column.SpecialSize > 0)
                    {
                        offsets.Add((int)writer.Stream.Position);
                        foreach (ArmpTableColumn child in column.Children)
                        {
                            writer.Write(child.UnknownMetadata0x4C);
                        }
                    }
                }
                offsets.Add((int)writer.Stream.Position);
                writer.Write(0);
                writer.WritePadding(0x00, 0x8);

                ptr = (int)writer.Stream.Position;

                foreach (ArmpTableColumn column in table.Columns)
                {
                    writer.Write(column.SpecialSize);
                    writer.Write(offsets[0]);
                    writer.WriteTimes(0x00, 0x18);
                    if (column.IsSpecial && column.SpecialSize > 0)
                    {
                        offsets.RemoveAt(0);
                    }
                }

                writer.Stream.PushToPosition(baseOffset + 0x4C);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }
        }


        /// <summary>
        /// Writes the DE v2 Data Types Aux table.
        /// </summary>
        /// <param name="writer">The DataWriter.</param>
        /// <param name="table">The ArmpTable.</param>
        private static void WriteColumnDataTypesAuxTable (DataWriter writer, ArmpTable table)
        {
            int distance = 0; //Distance from start

            foreach (string column in table.GetColumnNames())
            {
                Type type = table.GetColumnDataType(column);
                sbyte typeID = -1;
                if (type != null)
                    typeID = DataTypes.TypesV2Reverse[type];

                int size = 0;
                if (table.IsColumnSpecial(column))
                    size = Util.CountStringOccurrences($"{column}[", table.GetColumnNames());

                writer.Write((int)DataTypes.TypeIDsV2Aux[typeID]); //Aux Type ID
                if (typeID == -1)
                    writer.Write(-1);
                else
                    writer.Write(distance); //Distance from start
                writer.Write(size); //Array size
                writer.WriteTimes(0x00, 4); //Padding

                if (typeID != -1)
                    distance += DataTypes.TypesV2Sizes[typeID];
            }
        }
    }
}

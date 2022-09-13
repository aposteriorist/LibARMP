using LibARMP.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using Yarhl.IO;

namespace LibARMP
{
    public static class ArmpFileWriter
    {
        private static void WriteARMP(ARMP armp, DataStream datastream)
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
                writer.WriteTimes(0x00, 0x10); //Padding
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
        public static void WriteARMPToFile(ARMP armp, string path)
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
        public static Stream WriteARMPToStream(ARMP armp)
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
        public static byte[] WriteARMPToArray(ARMP armp)
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
            //Row Validity
            if (table.TableInfo.HasRowValidity)
            {
                //TODO Ishin
                List<bool> rowValidity = new List<bool>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    rowValidity.Add(entry.IsValid);
                }
                table.RowValidity = rowValidity;
                ptr = (int)writer.Stream.Position;
                Util.WriteBooleanBitmask(writer, table.RowValidity);
                writer.WritePadding(0x00, 8);
                writer.Stream.PushToPosition(baseOffset + 0xC);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Row Names
            if (table.TableInfo.HasRowNames)
            {
                List<string> rowNames = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    rowNames.Add(entry.Name);
                }
                table.RowNames = rowNames;
                ptr = Util.WriteText(writer, table.RowNames);
                writer.Stream.PushToPosition(baseOffset + 0x8);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Names and Count
            if (table.TableInfo.HasColumnNames)
            {
                ptr = Util.WriteText(writer, table.ColumnNames);
                writer.Stream.PushToPosition(baseOffset + 0x10);
                writer.Write(table.ColumnNames.Count);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Types
            ptr = (int)writer.Stream.Position;
            foreach (Type type in table.ColumnDataTypes)
            {
                int typeID = -1;
                if (type != null)
                {
                    typeID = DataTypes.TypesOEReverse[type];
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
                foreach (int meta in table.ColumnMetadata)
                {
                    writer.Write(meta);
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
                List<string> stringTypeColumns = new List<string>();
                foreach (string column in table.ColumnNames)
                {
                    int columnIndex = table.ColumnNames.IndexOf(column);
                    if (table.ColumnDataTypes[columnIndex] == DataTypes.Types["string"])
                    {
                        stringTypeColumns.Add(column);
                    }
                }

                List<string> textList = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    foreach (string column in stringTypeColumns)
                    {
                        string str = (string)entry.GetValueFromColumn(column);
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
            foreach (string column in table.ColumnNames)
            {
                int columnIndex = table.ColumnNames.IndexOf(column);
                if (table.ColumnValidity != null && table.ColumnValidity[columnIndex] != true)
                {
                    columnValueOffsets.Add(0);
                }
                else
                {
                    columnValueOffsets.Add((int)writer.Stream.Position);
                    List<bool> boolList = new List<bool>(); //Init list in case it is a boolean column
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        if (table.ColumnDataTypes[columnIndex] == DataTypes.Types["string"])
                        {
                            if (entry.GetValueFromColumn(column) != null)
                            {
                                int index = table.Text.IndexOf((string)entry.GetValueFromColumn(column));
                                writer.WriteOfType<Int16>((Int16)index);
                            }
                            else
                            {
                                writer.WriteOfType<Int16>(-1);
                            }
                        }

                        else if (table.ColumnDataTypes[columnIndex] == DataTypes.Types["boolean"])
                        {
                            boolList.Add((bool)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypes[columnIndex] == DataTypes.Types["uint8"])
                        {
                            writer.Write((byte)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypes[columnIndex] == DataTypes.Types["int8"])
                        {
                            writer.Write((sbyte)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypes[columnIndex] == DataTypes.Types["uint16"])
                        {
                            writer.Write((UInt16)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypes[columnIndex] == DataTypes.Types["int16"])
                        {
                            writer.Write((Int16)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypes[columnIndex] == DataTypes.Types["uint32"])
                        {
                            writer.Write((UInt32)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypes[columnIndex] == DataTypes.Types["int32"])
                        {
                            writer.Write((Int32)entry.GetValueFromColumn(column));
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

            //Row and column counts
            writer.Stream.PushToPosition(baseOffset);
            writer.Write(table.Entries.Count);
            writer.Write(table.ColumnNames.Count);

            //Table ID and Storage Mode
            writer.Stream.Position = baseOffset + 0x20;
            writer.Write(table.TableInfo.TableID);
            writer.Stream.Position = baseOffset + 0x23;
            writer.Write(table.TableInfo.StorageMode);
            writer.Stream.PopPosition();

            int ptr = 0;
            //Row Validity
            if (table.TableInfo.HasRowValidity)
            {
                List<bool> rowValidity = new List<bool>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    rowValidity.Add(entry.IsValid);
                }
                table.RowValidity = rowValidity;
                ptr = (int)writer.Stream.Position;
                Util.WriteBooleanBitmask(writer, table.RowValidity);
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

            //Row Validator
            writer.Stream.PushToPosition(baseOffset + 0xC);
            writer.Write(table.TableInfo.RowValidator);
            writer.Stream.PopPosition();

            //Column Validity
            if (table.TableInfo.HasColumnValidity)
            {
                ptr = (int)writer.Stream.Position;
                Util.WriteBooleanBitmask(writer, table.ColumnValidity);
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

            //Row Names
            if (table.TableInfo.HasRowNames)
            {
                List<string> rowNames = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    rowNames.Add(entry.Name);
                }
                table.RowNames = rowNames;
                ptr = Util.WriteText(writer, table.RowNames);
                writer.Stream.PushToPosition(baseOffset + 0x10);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Names
            if (table.TableInfo.HasColumnNames)
            {
                ptr = Util.WriteText(writer, table.ColumnNames);
                writer.Stream.PushToPosition(baseOffset + 0x28);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Text
            if (table.TableInfo.HasText)
            {
                //Force an update of the table text.

                //Get the columns of type string 
                List<string> stringTypeColumns = new List<string>();
                foreach (string column in table.ColumnNames)
                {
                    int columnIndex = table.ColumnNames.IndexOf(column);
                    if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["string"])
                    {
                        stringTypeColumns.Add(column);
                    }
                }

                List<string> textList = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    foreach (string column in stringTypeColumns)
                    {
                        string str = (string)entry.GetValueFromColumn(column);
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
            foreach (Type type in table.ColumnDataTypes)
            {
                sbyte typeID = -1;
                if (type != null)
                {
                    typeID = DataTypes.TypesV1Reverse[type];
                }
                writer.Write(typeID);
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

            //Column Types Aux
            //TODO v2 table
            ptr = (int)writer.Stream.Position;
            foreach (Type type in table.ColumnDataTypesAux)
            {
                sbyte typeID = -1;
                if (type != null)
                {
                    typeID = DataTypes.TypesV1AuxReverse[type];
                }
                writer.Write(typeID);
            }
            writer.Stream.PushToPosition(baseOffset + 0x48);
            writer.Write(ptr);
            writer.Stream.PopPosition();
            //writer.WritePadding(0x00, 4);


            //Column Contents
            List<int> columnValueOffsets = new List<int>();
            foreach (string column in table.ColumnNames)
            {
                int columnIndex = table.ColumnNames.IndexOf(column);
                if (table.NoDataColumns.Contains(columnIndex))
                {
                    Console.WriteLine("NO DATA!!!");
                    columnValueOffsets.Add(-1);
                }
                else if (table.ColumnValidity != null && table.ColumnValidity[columnIndex] != true)
                {
                    columnValueOffsets.Add(0);
                }
                else
                {

                    if (table.GetColumnDataType(column) == DataTypes.Types["table"])
                    {
                        writer.WritePadding(0x00, 0x4);
                    }
                    columnValueOffsets.Add((int)writer.Stream.Position);
                    List<bool> boolList = new List<bool>(); //Init list in case it is a boolean column
                    List<ArmpTableMain> tableList = new List<ArmpTableMain>(); //Init list in case it is a table column
                    List<int> tableOffsetList = new List<int>(); //Init list in case it is a table column
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["string"])
                        {
                            if (entry.GetValueFromColumn(column) != null)
                            {
                                int index = table.Text.IndexOf((string)entry.GetValueFromColumn(column));
                                writer.Write(index);
                            }
                            else
                            {
                                writer.Write(-1);
                            }
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["boolean"])
                        {
                            boolList.Add((bool)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["table"])
                        {
                            try
                            {
                                ulong tablePtr = tableValuePointers[(ArmpTableMain)entry.GetValueFromColumn(column)];
                                writer.Write(tablePtr);
                            } catch
                            {
                                writer.WriteTimes(0x00, 0x8);
                            }
                        }

                        else
                        {
                            var methodinfo = typeof(ArmpFileWriter).GetMethod("WriteType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                            var methodref = methodinfo.MakeGenericMethod(table.ColumnDataTypesAux[columnIndex]);
                            methodref.Invoke(null, new object[] { writer, entry.GetValueFromColumn(column) });
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
            foreach(int offset in columnValueOffsets)
            {
                writer.Write(offset);
            }

            writer.Stream.PushToPosition(baseOffset + 0x1C);
            writer.Write(ptrColumnOffsetTable);
            writer.Stream.PopPosition();



            //Row Indices
            if (table.TableInfo.HasRowIndices)
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
                foreach (int index in table.ColumnIndices)
                {
                    writer.Write(index);
                }
                writer.Stream.PushToPosition(baseOffset + 0x34);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Metadata
            if (table.TableInfo.HasColumnMetadata)
            {
                ptr = (int)writer.Stream.Position;
                foreach (int metadata in table.ColumnMetadata)
                {
                    writer.Write(metadata);
                }
                writer.Stream.PushToPosition(baseOffset + 0x40);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Entry Flags (v1 only)
            if (table.TableInfo.HasExtraFieldInfo)
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
        }
    }
}

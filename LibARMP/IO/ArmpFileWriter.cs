using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Yarhl.IO;

namespace LibARMP.IO
{
    public static class ArmpFileWriter
    {

        internal static Dictionary<Type, MethodInfo> WriteTypeCache = new Dictionary<Type, MethodInfo>();

        private static void WriteARMP(ARMP armp, DataStream datastream)
        {
            bool isOldEngine = false;
            if (armp.FormatVersion == Version.OldEngine || armp.FormatVersion == Version.OldEngineIshin)
                isOldEngine = true;

            var writer = new DataWriter(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
                DefaultEncoding = System.Text.Encoding.UTF8,
            };
            if (isOldEngine)
            {
                writer.Endianness = EndiannessMode.BigEndian;
                writer.DefaultEncoding = System.Text.Encoding.GetEncoding(932);
            }

            writer.Write("armp", false); //Magic
            if (isOldEngine) writer.Write(0x02010000); //Endianness identifier for OE
            else writer.WriteTimes(0x00, 0x4);
            if (isOldEngine) //Version and Revision are flipped on different endianess. Presumably both values are read together as an int32
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

            if (isOldEngine)
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
        /// Writes an <see cref="ARMP"/> to a file.
        /// </summary>
        /// <param name="armp">The <see cref="ARMP"/> to write.</param>
        /// <param name="path">The destination file path.</param>
        public static void WriteARMPToFile(ARMP armp, string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Write))
            {
                WriteARMP(armp, datastream);
            }
        }



        /// <summary>
        /// Writes an <see cref="ARMP"/> to a stream.
        /// </summary>
        /// <param name="armp">The <see cref="ARMP"/> to write.</param>
        public static Stream WriteARMPToStream(ARMP armp)
        {
            MemoryStream stream = new MemoryStream();
            DataStream tempds = DataStreamFactory.FromMemory();
            WriteARMP(armp, tempds);
            tempds.WriteTo(stream);
            return stream;
        }



        /// <summary>
        /// Writes an <see cref="ARMP"/> to a byte array.
        /// </summary>
        /// <param name="armp">The <see cref="ARMP"/> to write.</param>
        public static byte[] WriteARMPToArray(ARMP armp)
        {
            DataStream tempds = DataStreamFactory.FromMemory();
            WriteARMP(armp, tempds);
            return tempds.ToArray();
        }



        /// <summary>
        /// Writes a value of type T to the <see cref="DataStream"/>.
        /// </summary>
        /// <typeparam name="T">The type to read.</typeparam>
        /// <param name="writer">The <see cref="DataWriter"/>.</param>
        /// <param name="value">The value of type T to write.</param>
        private static void WriteType<T>(DataWriter writer, object value)
        {
            writer.WriteOfType((T)value);
        }



        /// <summary>
        /// Writes an OE table to the <see cref="DataStream"/>.
        /// </summary>
        /// <param name="writer">The <see cref="DataWriter"/>.</param>
        /// <param name="table">The <see cref="ArmpTableMain"/> to write.</param>
        private static void WriteTableOE(DataWriter writer, ArmpTableMain table)
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
                writer.PushWritePop(ptr, baseOffset + 0xC);
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
                writer.PushWritePop(ptr, baseOffset + 0x8);
            }

            //Column Names and Count
            if (table.TableInfo.HasColumnNames)
            {
                ptr = Util.WriteText(writer, table.GetColumnNames());
                writer.PushWritePop(new int[] { table.Columns.Count, ptr }, baseOffset + 0x10);
            }

            //Column Types
            ptr = (int)writer.Stream.Position;
            foreach (ArmpTableColumn column in table.Columns)
            {
                int typeID = column.Type.GetID(table.TableInfo.FormatVersion);
                writer.Write(typeID);
            }
            writer.PushWritePop(ptr, baseOffset + 0x18);

            //Column Metadata
            if (table.TableInfo.HasColumnMetadata)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    writer.Write(column.UnknownMetadata0x40);
                }
                writer.PushWritePop(ptr, baseOffset + 0x24);
            }

            //Text
            if (table.TableInfo.HasText)
            {
                //Force an update of the text table.

                //Get the columns of type string 
                List<ArmpTableColumn> stringTypeColumns = new List<ArmpTableColumn>();
                foreach (ArmpTableColumn column in table.Columns)
                {
                    if (column.Type.CSType == typeof(string))
                    {
                        stringTypeColumns.Add(column);
                    }
                }

                List<string> textList = new List<string>();

                foreach (ArmpTableColumn column in stringTypeColumns)
                {
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        string str = entry.GetValueFromColumn<string>(column.Name);
                        if (!textList.Contains(str) && str != null) textList.Add(str);
                    }
                }
                table.Text = textList;

                Encoding encoding = Encoding.UTF8;
                if (table.TableInfo.FormatVersion == Version.OldEngineIshin)
                    encoding = Encoding.GetEncoding(932);

                ptr = Util.WriteText(writer, table.Text, encoding);
                writer.WritePadding(0x00, 0x10);
                writer.PushWritePop(ptr, baseOffset + 0x28);
                //Text count
                writer.PushWritePop(table.Text.Count, baseOffset + 0x2C);
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
                        if (column.Type.CSType == typeof(string))
                        {
                            if (entry.GetValueFromColumn(column.Name) != null)
                            {
                                int index = table.Text.IndexOf((string)entry.GetValueFromColumn(column.Name));
                                writer.WriteOfType((short)index);
                            }
                            else
                            {
                                writer.WriteOfType<short>(-1);
                            }
                        }

                        else if (column.Type.CSType == typeof(bool))
                        {
                            boolList.Add((bool)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.Type.CSType == typeof(byte))
                        {
                            writer.Write((byte)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.Type.CSType == typeof(sbyte))
                        {
                            writer.Write((sbyte)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.Type.CSType == typeof(ushort))
                        {
                            writer.Write((ushort)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.Type.CSType == typeof(short))
                        {
                            writer.Write((short)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.Type.CSType == typeof(uint))
                        {
                            writer.Write((uint)entry.GetValueFromColumn(column.Name));
                        }

                        else if (column.Type.CSType == typeof(int))
                        {
                            writer.Write((int)entry.GetValueFromColumn(column.Name));
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

            writer.PushWritePop(ptrColumnOffsetTable, baseOffset + 0x1C);
        }



        /// <summary>
        /// Recursively writes tables from the lowest level upwards.
        /// </summary>
        /// <param name="writer">The <see cref="DataWriter"/>.</param>
        /// <param name="table">The <see cref="ArmpTable"/> to write.</param>
        /// <returns>The pointer to the table.</returns>
        private static uint WriteTableRecursive(DataWriter writer, ArmpTable table)
        {
            List<string> tableColumns = table.GetColumnNamesByType<ArmpTableMain>();
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

            uint pointer = (uint)writer.Stream.Position;
            WriteTable(writer, table, tablePointers);
            writer.WritePadding(0x00, 0x10);
            writer.Stream.PushToPosition(pointer + 0x3C);
            writer.Write(subtablePtr);
            writer.Stream.PopPosition();
            return pointer;
        }



        /// <summary>
        /// Writes a DE table to the <see cref="DataStream"/>.
        /// </summary>
        /// <param name="writer">The <see cref="DataWriter"/>.</param>
        /// <param name="table">The <see cref="ArmpTable"/> to write.</param>
        private static void WriteTable(DataWriter writer, ArmpTable table, Dictionary<ArmpTable, uint> tableValuePointers = null)
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
            writer.Write((byte)table.TableInfo.StorageMode);
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
                writer.WritePadding(0x00, 0x8);
                writer.PushWritePop(ptr, baseOffset + 0x14);
            }
            else
            {
                writer.PushWritePop(-1, baseOffset + 0x14);
            }

            //Entry Validator
            writer.PushWritePop(table.TableInfo.EntryValidator, baseOffset + 0xC);

            //Column Validity
            if (table.TableInfo.HasColumnValidity)
            {
                ptr = (int)writer.Stream.Position;
                List<bool> columnValidity = new List<bool>();
                foreach (ArmpTableColumn column in table.Columns)
                    columnValidity.Add((bool)column.IsValid);
                Util.WriteBooleanBitmask(writer, columnValidity);
                writer.WritePadding(0x00, 0x4);
                writer.PushWritePop(ptr, baseOffset + 0x38);
            }
            else
            {
                writer.PushWritePop(-1, baseOffset + 0x38);
            }

            //Column Validator
            writer.PushWritePop(table.TableInfo.ColumnValidator, baseOffset + 0x2C);

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
                writer.PushWritePop(ptr, baseOffset + 0x10);
            }

            //Column Names
            if (table.TableInfo.HasColumnNames)
            {
                ptr = Util.WriteText(writer, table.GetColumnNames());
                writer.PushWritePop(ptr, baseOffset + 0x28);
            }

            //Text
            if (table.TableInfo.HasText)
            {
                //Force an update of the table text.

                //Get the columns of type string 
                List<ArmpTableColumn> stringTypeColumns = new List<ArmpTableColumn>();
                foreach (ArmpTableColumn column in table.Columns)
                {
                    if (column.Type.CSType == typeof(string))
                    {
                        stringTypeColumns.Add(column);
                    }
                }

                List<string> textList = new List<string>();

                foreach (ArmpTableColumn column in stringTypeColumns)
                {
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        string str = entry.GetValueFromColumn<string>(column.Name);
                        if (!textList.Contains(str) && str != null) textList.Add(str);
                    }
                }
                table.Text = textList;

                ptr = Util.WriteText(writer, table.Text);
                writer.PushWritePop(ptr, baseOffset + 0x24);
                //Text count
                writer.PushWritePop(table.Text.Count, baseOffset + 0x8);

                if (table.TableInfo.FormatVersion == Version.DragonEngineV2) writer.WritePadding(0x00, 0x8);
            }
            else
            {
                writer.WritePadding(0x00, 0x8);
            }

            //Column Types
            ptr = (int)writer.Stream.Position;
            foreach (ArmpTableColumn column in table.Columns)
            {
                sbyte typeID = column.Type.GetID(table.TableInfo.FormatVersion);
                writer.Write(typeID);
            }
            writer.WritePadding(0x00, 0x4);
            writer.PushWritePop(ptr, baseOffset + 0x18);

            //Set the text ptr to column types if there is no text
            if (!table.TableInfo.HasText)
            {
                writer.PushWritePop(ptr, baseOffset + 0x24);
            }

            //Column Types Aux (V1)
            if (table.TableInfo.FormatVersion == Version.DragonEngineV1)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    sbyte typeID = column.Type.GetIDAux(table.TableInfo.FormatVersion);
                    writer.Write(typeID);
                }
                writer.WritePadding(0x00, 0x4);
                writer.PushWritePop(ptr, baseOffset + 0x48);
            }


            //Column Contents
            if (table.TableInfo.StorageMode == StorageMode.Column)
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
                        writer.WritePadding(0x00, column.Type.Size);

                        columnValueOffsets.Add((int)writer.Stream.Position);

                        if (table.TableInfo.FormatVersion == Version.DragonEngineV2 && column.IsSpecial) continue;

                        List<bool> boolList = new List<bool>(); //Init list in case it is a boolean column
                        List<ArmpTableMain> tableList = new List<ArmpTableMain>(); //Init list in case it is a table column
                        List<int> tableOffsetList = new List<int>(); //Init list in case it is a table column

                        foreach (ArmpEntry entry in table.Entries)
                        {
                            if (column.Type.CSType == typeof(string))
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

                            else if (column.Type.CSType == typeof(bool))
                            {
                                boolList.Add((bool)entry.GetValueFromColumn(column.Name));
                            }

                            else if (column.Type.CSType == typeof(ArmpTableMain))
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
                                Type columnType = column.Type.CSType;
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

                int ptrColumnOffsetTable = (int)writer.Stream.Position;
                foreach (int offset in columnValueOffsets)
                {
                    writer.Write(offset);
                }

                writer.PushWritePop(ptrColumnOffsetTable, baseOffset + 0x1C);
            }


            else if (table.TableInfo.StorageMode == StorageMode.Entry)
            {
                writer.WritePadding(0x00, 0x10);

                table.UpdateColumnDistances();
                Dictionary<string, int> columnPaddingCache = new Dictionary<string, int>();

                List<int> entryValueOffsets = new List<int>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    int startOffset = (int)writer.Stream.Position;
                    entryValueOffsets.Add(startOffset);

                    foreach (ArmpTableColumn column in table.Columns)
                    {
                        if (column.IsSpecial) continue;

                        int columnPadding = 0;
                        if (columnPaddingCache.ContainsKey(column.Name)) columnPadding = columnPaddingCache[column.Name];
                        else
                        {
                            int amountWritten = (int)writer.Stream.Position - startOffset;
                            columnPadding = column.Distance - amountWritten;
                            columnPaddingCache.Add(column.Name, columnPadding);
                        }
                        writer.WriteTimes(0x00, columnPadding);


                        if (table.TableInfo.HasColumnValidity && table.IsColumnValid(column.Name))
                        {
                            if (column.Type.CSType == typeof(string))
                            {
                                if (entry.GetValueFromColumn(column.Name) != null)
                                {
                                    long index = table.Text.IndexOf((string)entry.GetValueFromColumn(column.Name));
                                    writer.Write(index);
                                }
                                else
                                {
                                    writer.Write((long)-1);
                                }
                            }

                            else if (column.Type.CSType == typeof(bool))
                            {
                                bool val = (bool)entry.GetValueFromColumn(column.Name);
                                writer.Write(Convert.ToByte(val));
                            }

                            else if (column.Type.CSType == typeof(ArmpTableMain))
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
                                if (WriteTypeCache.ContainsKey(column.Type.CSType))
                                {
                                    MethodInfo methodref = WriteTypeCache[column.Type.CSType];
                                    methodref.Invoke(null, new object[] { writer, entry.GetValueFromColumn(column.Name) });
                                }
                                else
                                {
                                    MethodInfo methodinfo = typeof(ArmpFileWriter).GetMethod("WriteType", BindingFlags.NonPublic | BindingFlags.Static);
                                    MethodInfo methodref = methodinfo.MakeGenericMethod(column.Type.CSType);
                                    WriteTypeCache.Add(column.Type.CSType, methodref);
                                    methodref.Invoke(null, new object[] { writer, entry.GetValueFromColumn(column.Name) });
                                }
                            }
                        }
                    }
                    writer.WritePadding(0x00, 0x4);
                }
                int ptrColumnOffsetTable = (int)writer.Stream.Position;
                foreach (int offset in entryValueOffsets)
                {
                    writer.Write(offset);
                }

                writer.PushWritePop(ptrColumnOffsetTable, baseOffset + 0x1C);
            }



            //Entry Indices
            if (table.TableInfo.HasEntryIndices)
            {
                ptr = (int)writer.Stream.Position;

                foreach (ArmpEntry entry in table.Entries)
                {
                    writer.Write(entry.Index);
                }
                writer.PushWritePop(ptr, baseOffset + 0x30);
            }

            //Column Indices
            if (table.TableInfo.HasColumnIndices)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    writer.Write(column.Index);
                }
                writer.PushWritePop(ptr, baseOffset + 0x34);
            }

            //Column Metadata
            if (table.TableInfo.HasColumnMetadata)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    writer.Write(column.UnknownMetadata0x40);
                }
                writer.PushWritePop(ptr, baseOffset + 0x40);
            }

            //Entry Info Flags (v1 only)
            if (table.TableInfo.HasExtraFieldInfo && table.TableInfo.FormatVersion == Version.DragonEngineV1)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpEntry entry in table.Entries)
                {
                    string bitstring = "";
                    for (int i = 0; i < entry.Flags.Length; i++)
                    {
                        bitstring += Convert.ToByte(entry.Flags[i]);
                    }
                    byte value = Convert.ToByte(Util.ReverseString(bitstring), 2);
                    writer.Write(value);
                }
                writer.PushWritePop(ptr, baseOffset + 0x4C);
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
                for (int i = 0; i < table.TableInfo.ColumnCount; i++)
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

                writer.PushWritePop(ptr, baseOffset + 0x44);
            }

            // Column Data Types Aux (V2)
            if (table.TableInfo.FormatVersion == Version.DragonEngineV2 && table.TableInfo.HasColumnDataTypesAux)
            {
                writer.WritePadding(0x00, 0x10);
                ptr = (int)writer.Stream.Position;
                WriteColumnDataTypesAuxTable(writer, table);

                writer.PushWritePop(ptr, baseOffset + 0x48);
            }

            // Column Unknown Metadata 0x4C
            if (table.TableInfo.FormatVersion == Version.DragonEngineV2 && table.TableInfo.HasExtraFieldInfo)
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

                writer.PushWritePop(ptr, baseOffset + 0x4C);
            }
        }


        /// <summary>
        /// Writes the DE v2 Data Types Aux table.
        /// </summary>
        /// <param name="writer">The <see cref="DataWriter"/>.</param>
        /// <param name="table">The <see cref="ArmpTable"/>.</param>
        private static void WriteColumnDataTypesAuxTable(DataWriter writer, ArmpTable table)
        {
            foreach (ArmpTableColumn column in table.Columns)
            {
                sbyte typeID = column.Type.GetID(table.TableInfo.FormatVersion);

                int size = 0;
                if (column.IsSpecial)
                    size = Util.CountStringOccurrences($"{column.Name}[", table.GetColumnNames());

                writer.Write((int)column.Type.GetIDAux(table.TableInfo.FormatVersion)); //Aux Type ID
                if (column.Type.CSType == null)
                {
                    writer.Write(-1);
                }
                else
                {
                    writer.Write(column.Distance); //Distance from start
                }
                writer.Write(size); //Array size
                writer.WriteTimes(0x00, 4); //Padding
            }
        }
    }
}

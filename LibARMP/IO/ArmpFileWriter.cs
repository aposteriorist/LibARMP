using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Yarhl.IO;

namespace LibARMP.IO
{
    public static class ArmpFileWriter
    {
        /// <summary>
        /// Writes an <see cref="ARMP"/> to a <see cref="DataStream"/>.
        /// </summary>
        /// <param name="armp">The <see cref="ARMP"/> to write.</param>
        /// <param name="datastream">The destination <see cref="DataStream"/>.</param>
        private static void WriteARMP(ARMP armp, DataStream datastream)
        {
            bool isOldEngine = false;
            if (armp.FormatVersion == Version.OldEngine || armp.FormatVersion == Version.OldEngineIshin)
                isOldEngine = true;

            var writer = new DataWriter(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
                DefaultEncoding = Encoding.UTF8,
            };
            if (isOldEngine)
            {
                writer.Endianness = EndiannessMode.BigEndian;
                writer.DefaultEncoding = Encoding.GetEncoding(932); // Shift JIS
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
        /// Writes an OE table to the <see cref="DataStream"/>.
        /// </summary>
        /// <param name="writer">The <see cref="DataWriter"/>.</param>
        /// <param name="table">The <see cref="ArmpTableMain"/> to write.</param>
        private static void WriteTableOE(DataWriter writer, ArmpTableMain table)
        {
            long baseOffset = writer.Stream.Position;
            int ptr = 0;

            writer.WriteTimes(0x00, 0x40); //Placeholder table

            ///// Entry Count /////
            writer.PushWritePop(table.Entries.Count, baseOffset);

            ///// Entry Validity /////
            #region EntryValidity

            if (table.TableInfo.HasEntryValidity)
            {
                List<bool> entryValidity = new List<bool>();
                ptr = (int)writer.Stream.Position;

                // Ishin
                if (table.TableInfo.FormatVersion == Version.OldEngineIshin)
                {
                    // Ishin stores these booleans as int32
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        writer.Write(entry.IsValid ? 1 : 0);
                    }
                    // Update the main table pointer at 0x4
                    writer.PushWritePop(ptr, baseOffset + 0x4);
                }
                // 0, K1, FOTNS
                else
                {
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        entryValidity.Add(entry.IsValid);
                    }
                    Util.WriteBooleanBitmask(writer, entryValidity);
                    // Update the main table pointer at 0xC
                    writer.PushWritePop(ptr, baseOffset + 0xC);
                }

                writer.WritePadding(0x00, 0x10);
            }
            #endregion


            ///// Entry Names /////
            #region EntryNames

            if (table.TableInfo.HasEntryNames)
            {
                List<string> entryNames = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    entryNames.Add(entry.Name);
                }
                ptr = Util.WriteText(writer, entryNames);
                // Update the main table pointer at 0x8
                writer.PushWritePop(ptr, baseOffset + 0x8);
                
                writer.WritePadding(0x00, 0x10);
            }
            #endregion


            ///// Column Names and Count /////
            #region ColumnNames

            if (table.TableInfo.HasColumnNames)
            {
                ptr = Util.WriteText(writer, table.GetColumnNames());
                // Update the main table column count at 0x10 and pointer to column names at 0x14
                writer.PushWritePop(new int[] { table.Columns.Count, ptr }, baseOffset + 0x10);
            }
            #endregion


            ///// Text /////
            #region Text

            if (table.TableInfo.HasText)
            {
                table.UpdateTextList();

                Encoding encoding = Encoding.UTF8;
                if (table.TableInfo.FormatVersion == Version.OldEngineIshin)
                    encoding = Encoding.GetEncoding(932);

                ptr = Util.WriteText(writer, table.Text, encoding);
                // Update the main table text offset table pointer at 0x28 and text count at 0x2C
                writer.PushWritePop(new int[] { ptr, table.Text.Count }, baseOffset + 0x28);
            }
            #endregion


            ///// Column Types /////
            #region ColumnTypes

            ptr = (int)writer.Stream.Position;
            foreach (ArmpTableColumn column in table.Columns)
            {
                int typeID = column.Type.GetID(table.TableInfo.FormatVersion);
                writer.Write(typeID);
            }
            // Update the main table pointer at 0x18
            writer.PushWritePop(ptr, baseOffset + 0x18);
            #endregion


            ///// Column Metadata /////
            #region ColumnMetadata

            if (table.TableInfo.HasColumnMetadata)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    writer.Write(column.UnknownMetadata0x40);
                }
                // Update the main table pointer at 0x24
                writer.PushWritePop(ptr, baseOffset + 0x24);
            }
            #endregion


            ///// Column Contents /////
            #region ColumnContents

            List<int> columnValueOffsets = new List<int>();
            foreach (ArmpTableColumn column in table.Columns)
            {
                if (column.Type.IDOE == -1)
                {
                    columnValueOffsets.Add(0);
                }
                else
                {
                    columnValueOffsets.Add((int)writer.Stream.Position);

                    // Write operations based on column type
                    if (column.Type.CSType == typeof(string))
                    {
                        foreach (ArmpEntry entry in table.Entries)
                        {
                            string value = entry.GetValueFromColumn<string>(column.Name);
                            if (value != null)
                            {
                                int index = table.Text.IndexOf(value);
                                writer.Write((short)index);
                            }
                            else
                            {
                                writer.Write((short)-1);
                            }
                        }
                    }

                    else if (column.Type.CSType == typeof(bool))
                    {
                        List<bool> boolList = new List<bool>();
                        foreach (ArmpEntry entry in table.Entries)
                        {
                            boolList.Add(entry.GetValueFromColumn<bool>(column.Name));
                        }

                        if (boolList.Count > 0)
                        {
                            Util.WriteBooleanBitmask(writer, boolList);
                        }
                    }

                    else if (column.Type.CSType == typeof(byte))
                    {
                        foreach (ArmpEntry entry in table.Entries)
                            writer.Write(entry.GetValueFromColumn<byte>(column.Name));
                    }

                    else if (column.Type.CSType == typeof(sbyte))
                    {
                        foreach (ArmpEntry entry in table.Entries)
                            writer.Write(entry.GetValueFromColumn<sbyte>(column.Name));
                    }

                    else if (column.Type.CSType == typeof(UInt16))
                    {
                        foreach (ArmpEntry entry in table.Entries)
                            writer.Write(entry.GetValueFromColumn<UInt16>(column.Name));
                    }

                    else if (column.Type.CSType == typeof(Int16))
                    {
                        foreach (ArmpEntry entry in table.Entries)
                            writer.Write(entry.GetValueFromColumn<Int16>(column.Name));
                    }

                    else if (column.Type.CSType == typeof(UInt32))
                    {
                        foreach (ArmpEntry entry in table.Entries)
                            writer.Write(entry.GetValueFromColumn<UInt32>(column.Name));
                    }

                    else if (column.Type.CSType == typeof(Int32))
                    {
                        foreach (ArmpEntry entry in table.Entries)
                            writer.Write(entry.GetValueFromColumn<Int32>(column.Name));
                    }
                }
            }

            writer.WritePadding(0x00, 0x4);

            // Write the column value offset table
            int ptrColumnOffsetTable = (int)writer.Stream.Position;
            foreach (int offset in columnValueOffsets)
            {
                writer.Write(offset);
            }

            writer.WritePadding(0x00, 0x10);

            // Update the main table pointer at 0x1C
            writer.PushWritePop(ptrColumnOffsetTable, baseOffset + 0x1C);
            #endregion


            ///// Header File Size /////
            writer.PushWritePop((int)writer.Stream.Length, 0xC);
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
            int ptr = 0;

            writer.WriteTimes(0x00, 0x50); //Placeholder table


            ///// Entry/Column Count /////
            writer.Stream.PushToPosition(baseOffset);
            writer.Write(table.Entries.Count);
            writer.Write(table.Columns.Count);


            ///// Table ID and Storage Mode /////
            writer.Stream.Position = baseOffset + 0x20;
            writer.Write(table.TableInfo.TableID);
            writer.Stream.Position = baseOffset + 0x23;
            writer.Write((byte)table.TableInfo.StorageMode);
            writer.Stream.PopPosition();


            ///// Entry Validator /////
            writer.PushWritePop(table.TableInfo.EntryValidator, baseOffset + 0xC);


            ///// Column Validator /////
            writer.PushWritePop(table.TableInfo.ColumnValidator, baseOffset + 0x2C);


            ///// Entry Validity //////
            #region EntryValidity

            if (table.TableInfo.HasEntryValidity)
            {
                List<bool> entryValidity = new List<bool>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    entryValidity.Add(entry.IsValid);
                }
                ptr = (int)writer.Stream.Position;
                Util.WriteBooleanBitmask(writer, entryValidity);

                writer.WritePadding(0x00, 0x8);

                // Update the main table pointer at 0x14
                writer.PushWritePop(ptr, baseOffset + 0x14);
            }
            else
            {
                // Update the main table pointer at 0x14
                writer.PushWritePop(-1, baseOffset + 0x14);
            }
            #endregion


            ///// Column Validity /////
            #region ColumnValidity

            if (table.TableInfo.HasColumnValidity)
            {
                List<bool> columnValidity = new List<bool>();
                foreach (ArmpTableColumn column in table.Columns)
                {
                    columnValidity.Add((bool)column.IsValid);
                }
                ptr = (int)writer.Stream.Position;
                Util.WriteBooleanBitmask(writer, columnValidity);

                writer.WritePadding(0x00, 0x4);

                // Update the main table pointer at 0x38
                writer.PushWritePop(ptr, baseOffset + 0x38);
            }
            else
            {
                // Update the main table pointer at 0x38
                writer.PushWritePop(-1, baseOffset + 0x38);
            }
            #endregion


            ///// Entry Names /////
            #region EntryNames

            if (table.TableInfo.HasEntryNames)
            {
                List<string> entryNames = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    entryNames.Add(entry.Name);
                }
                ptr = Util.WriteText(writer, entryNames);

                // Update the main table pointer at 0x10
                writer.PushWritePop(ptr, baseOffset + 0x10);
            }
            #endregion


            ///// Column Names /////
            #region ColumnNames

            if (table.TableInfo.HasColumnNames)
            {
                ptr = Util.WriteText(writer, table.GetColumnNames());

                // Update the main table pointer at 0x28
                writer.PushWritePop(ptr, baseOffset + 0x28);
            }
            #endregion


            ///// Text /////
            #region Text

            if (table.TableInfo.HasText)
            {
                table.UpdateTextList();

                ptr = Util.WriteText(writer, table.Text);

                // Update the main table text offset table pointer at 0x24
                writer.PushWritePop(ptr, baseOffset + 0x24);
                // Update the main table text count at 0x8
                writer.PushWritePop(table.Text.Count, baseOffset + 0x8);

                if (table.TableInfo.FormatVersion == Version.DragonEngineV2) writer.WritePadding(0x00, 0x8);
            }
            else
            {
                writer.WritePadding(0x00, 0x8);
            }
            #endregion


            ///// Column Types /////
            #region ColumnTypes

            ptr = (int)writer.Stream.Position;
            foreach (ArmpTableColumn column in table.Columns)
            {
                sbyte typeID = column.Type.GetID(table.TableInfo.FormatVersion);
                writer.Write(typeID);
            }
            writer.WritePadding(0x00, 0x4);
            // Update the main table pointer at 0x18
            writer.PushWritePop(ptr, baseOffset + 0x18);

            //Set the text ptr to column types if there is no text
            if (!table.TableInfo.HasText)
            {
                writer.PushWritePop(ptr, baseOffset + 0x24);
            }
            #endregion


            ///// Column Types Aux (V1) /////
            #region ColumnTypesAux(v1)

            if (table.TableInfo.FormatVersion == Version.DragonEngineV1)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    sbyte typeID = column.Type.GetIDAux(table.TableInfo.FormatVersion);
                    writer.Write(typeID);
                }
                writer.WritePadding(0x00, 0x4);
                // Update the main table pointer at 0x48
                writer.PushWritePop(ptr, baseOffset + 0x48);
            }
            #endregion


            ///// Column Contents /////
            #region ColumnContentsModeColumn

            if (table.TableInfo.StorageMode == StorageMode.Column)
            {
                List<int> columnValueOffsets = new List<int>();
                foreach (ArmpTableColumn column in table.Columns)
                {
                    if (column.IsNoData)
                    {
                        columnValueOffsets.Add(-1);
                    }
                    else if (table.TableInfo.HasColumnValidity && column.IsValid == false)
                    {
                        columnValueOffsets.Add(0);
                    }
                    // The column has data
                    else
                    {
                        writer.WritePadding(0x00, column.Type.Size);

                        columnValueOffsets.Add((int)writer.Stream.Position);

                        if (table.TableInfo.FormatVersion == Version.DragonEngineV2 && column.IsSpecial) continue;

                        // Write operations based on column type
                        if (column.Type.CSType == typeof(string))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                            {
                                string value = (string)entry.GetValueFromColumn(column.Name);
                                if (value != null)
                                {
                                    int index = table.Text.IndexOf(value);
                                    writer.Write(index);
                                }
                                else
                                {
                                    writer.Write(-1);
                                }
                            }
                        }

                        else if (column.Type.CSType == typeof(bool))
                        {
                            List<bool> boolList = new List<bool>();
                            foreach (ArmpEntry entry in table.Entries)
                            {
                                boolList.Add((bool)entry.GetValueFromColumn(column.Name));
                            }

                            if (boolList.Count > 0)
                            {
                                Util.WriteBooleanBitmask(writer, boolList);
                            }
                        }

                        else if (column.Type.CSType == typeof(ArmpTableMain))
                        {
                            if (tableValuePointers != null)
                            {
                                foreach (ArmpEntry entry in table.Entries)
                                {
                                    if (entry.Data.ContainsKey(column.Name))
                                    {
                                        ArmpTableMain tableValue = (ArmpTableMain)entry.GetValueFromColumn(column.Name);
                                        if (tableValue != null)
                                        {
                                            uint tablePtr;
                                            tableValuePointers.TryGetValue(tableValue, out tablePtr);
                                            writer.Write((ulong)tablePtr);
                                        }
                                        else
                                        {
                                            writer.WriteTimes(0x00, 0x8);
                                        }
                                    }
                                    else
                                    {
                                        writer.WriteTimes(0x00, 0x8);
                                    }
                                }
                            }
                        }

                        else if (column.Type.CSType == typeof(float))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                                writer.Write(entry.GetValueFromColumn<float>(column.Name));
                        }

                        else if (column.Type.CSType == typeof(double))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                                writer.Write(entry.GetValueFromColumn<double>(column.Name));
                        }

                        else if (column.Type.CSType == typeof(byte))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                                writer.Write(entry.GetValueFromColumn<byte>(column.Name));
                        }

                        else if (column.Type.CSType == typeof(sbyte))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                                writer.Write(entry.GetValueFromColumn<sbyte>(column.Name));
                        }

                        else if (column.Type.CSType == typeof(UInt16))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                                writer.Write(entry.GetValueFromColumn<UInt16>(column.Name));
                        }

                        else if (column.Type.CSType == typeof(Int16))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                                writer.Write(entry.GetValueFromColumn<Int16>(column.Name));
                        }

                        else if (column.Type.CSType == typeof(UInt32))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                                writer.Write(entry.GetValueFromColumn<UInt32>(column.Name));
                        }

                        else if (column.Type.CSType == typeof(Int32))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                                writer.Write(entry.GetValueFromColumn<Int32>(column.Name));
                        }

                        else if (column.Type.CSType == typeof(UInt64))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                                writer.Write(entry.GetValueFromColumn<UInt64>(column.Name));
                        }

                        else if (column.Type.CSType == typeof(Int64))
                        {
                            foreach (ArmpEntry entry in table.Entries)
                                writer.Write(entry.GetValueFromColumn<Int64>(column.Name));
                        }
                    }
                }

                // Write the column value offset table
                writer.WritePadding(0x00, 0x4);
                int ptrColumnOffsetTable = (int)writer.Stream.Position;
                foreach (int offset in columnValueOffsets)
                {
                    writer.Write(offset);
                }

                // Update the main table pointer at 0x1C
                writer.PushWritePop(ptrColumnOffsetTable, baseOffset + 0x1C);
            }
            #endregion


            #region ColumnContentsModeEntry

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

                        // Write operations based on column type
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

                            else if (column.Type.CSType == typeof(float))
                            {
                                    writer.Write(entry.GetValueFromColumn<float>(column.Name));
                            }

                            else if (column.Type.CSType == typeof(double))
                            {
                                    writer.Write(entry.GetValueFromColumn<double>(column.Name));
                            }

                            else if (column.Type.CSType == typeof(byte))
                            {
                                    writer.Write(entry.GetValueFromColumn<byte>(column.Name));
                            }

                            else if (column.Type.CSType == typeof(sbyte))
                            {
                                    writer.Write(entry.GetValueFromColumn<sbyte>(column.Name));
                            }

                            else if (column.Type.CSType == typeof(UInt16))
                            {
                                    writer.Write(entry.GetValueFromColumn<UInt16>(column.Name));
                            }

                            else if (column.Type.CSType == typeof(Int16))
                            {
                                    writer.Write(entry.GetValueFromColumn<Int16>(column.Name));
                            }

                            else if (column.Type.CSType == typeof(UInt32))
                            {
                                    writer.Write(entry.GetValueFromColumn<UInt32>(column.Name));
                            }

                            else if (column.Type.CSType == typeof(Int32))
                            {
                                    writer.Write(entry.GetValueFromColumn<Int32>(column.Name));
                            }

                            else if (column.Type.CSType == typeof(UInt64))
                            {
                                    writer.Write(entry.GetValueFromColumn<UInt64>(column.Name));
                            }

                            else if (column.Type.CSType == typeof(Int64))
                            {
                                    writer.Write(entry.GetValueFromColumn<Int64>(column.Name));
                            }
                        }
                    }
                    writer.WritePadding(0x00, 0x4);
                }

                // Write the column value offset table
                int ptrColumnOffsetTable = (int)writer.Stream.Position;
                foreach (int offset in entryValueOffsets)
                {
                    writer.Write(offset);
                }

                // Update the main table pointer at 0x1C
                writer.PushWritePop(ptrColumnOffsetTable, baseOffset + 0x1C);
            }
            #endregion


            ///// Entry Indices /////
            #region EntryIndices

            if (table.TableInfo.HasEntryIndices)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpEntry entry in table.Entries)
                {
                    writer.Write(entry.Index);
                }
                // Update the main table pointer at 0x30
                writer.PushWritePop(ptr, baseOffset + 0x30);
            }
            #endregion


            ///// Column Indices /////
            #region ColumnIndices

            if (table.TableInfo.HasColumnIndices)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    writer.Write(column.Index);
                }
                // Update the main table pointer at 0x34
                writer.PushWritePop(ptr, baseOffset + 0x34);
            }
            #endregion


            ///// Column Metadata /////
            #region ColumnMetadata

            if (table.TableInfo.HasColumnMetadata)
            {
                ptr = (int)writer.Stream.Position;
                foreach (ArmpTableColumn column in table.Columns)
                {
                    writer.Write(column.UnknownMetadata0x40);
                }
                // Update the main table pointer at 0x40
                writer.PushWritePop(ptr, baseOffset + 0x40);
            }
            #endregion


            ///// Entry Info Flags (v1 only) /////
            #region EntryInfoFlags(v1)

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
                // Update the main table pointer at 0x4C
                writer.PushWritePop(ptr, baseOffset + 0x4C);
            }
            #endregion


            ///// Empty Values /////
            #region EmptyValues

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
                // Update the main table pointer at 0x44
                writer.PushWritePop(ptr, baseOffset + 0x44);
            }
            #endregion


            ///// Column Data Types Aux (V2) /////
            #region ColumnDataTypesAux(v2)

            if (table.TableInfo.FormatVersion == Version.DragonEngineV2 && table.TableInfo.HasColumnDataTypesAux)
            {
                writer.WritePadding(0x00, 0x10);
                ptr = (int)writer.Stream.Position;
                WriteColumnDataTypesAuxTable(writer, table);
                // Update the main table pointer at 0x48
                writer.PushWritePop(ptr, baseOffset + 0x48);
            }
            #endregion


            ///// Column Unknown Metadata 0x4C /////
            #region ColumnUnknownMetadata0x4C(v2)

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
                // Update the main table pointer at 0x4C
                writer.PushWritePop(ptr, baseOffset + 0x4C);
            }
            #endregion
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

using BinaryExtensions;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace LibARMP.IO
{
    public static class ArmpFileReader
    {
        static ArmpFileReader()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }


        /// <summary>
        /// Reads an armp file.
        /// </summary>
        /// <param name="stream">The armp file as <see cref="Stream"/>.</param>
        /// <param name="baseARMPMemoryAddress">The memory address where the armp starts at. Only needed if reading from memory.</param>
        /// <returns>An <see cref="ARMP"/> object.</returns>
        public static ARMP ReadARMP(Stream stream, IntPtr baseARMPMemoryAddress = default(IntPtr))
        {
            BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true);
            reader.BaseStream.Seek(0, SeekOrigin.Begin); // Ensure the position is at the start of the stream

            ARMP armp = new ARMP();

            char[] magic = reader.ReadChars(4);
            int endianess = reader.ReadInt32(); // Only used in OE
            if (endianess == 258)
            {
                armp.FormatVersion = Version.OldEngine;
            }

            if (armp.FormatVersion == Version.OldEngine)
            {
                armp.Version = reader.ReadInt16(true);
                armp.Revision = reader.ReadInt16(true);
            }
            else
            {
                armp.Revision = reader.ReadInt16();
                armp.Version = reader.ReadInt16();
            }

#if DEBUG
            Console.WriteLine("Version: " + armp.Version);
            Console.WriteLine("Revision: " + armp.Revision);
#endif

            int fileSize = reader.ReadInt32(); // Only used in OE

            if (armp.FormatVersion == Version.OldEngine)
            {
                reader.BaseStream.PushToPosition(0xC, SeekOrigin.Current);
                // This value, usually corresponding to the entry validity pointer, is unused in Ishin since it is instead found at 0x4
                if (reader.ReadInt32(true) == 0)
                {
                    armp.FormatVersion = Version.OldEngineIshin;
                    reader.Dispose();
                    reader = new BinaryReader(stream, Encoding.GetEncoding(932), true);
                }
                reader.BaseStream.PopPosition();
                armp.MainTable = ReadTableOE(reader, armp.FormatVersion);
            }
            else
            {
                if (armp.Version == 1) armp.FormatVersion = Version.DragonEngineV1;
                else if (armp.Version == 2) armp.FormatVersion = Version.DragonEngineV2;
                uint ptrMainTable = reader.ReadUInt32();
                armp.MainTable = ReadTable(reader, ptrMainTable, armp.FormatVersion, baseARMPMemoryAddress.ToInt64());
            }

            stream.CopyTo(armp.File);
            reader.Dispose();
            return armp;
        }



        /// <summary>
        /// Reads an armp file.
        /// </summary>
        /// <param name="fileBytes">The armp file as byte array.</param>
        /// <param name="offset">The location in the array to start reading data from.</param>
        /// <param name="length">The number of bytes to read from the array.</param>
        /// <param name="baseARMPMemoryAddress">The memory address where the armp starts at. Only needed if reading from memory.</param>
        /// <returns>An <see cref="ARMP"/> object.</returns>
        public static ARMP ReadARMP(byte[] fileBytes, int offset = 0, int length = 0, IntPtr baseARMPMemoryAddress = default(IntPtr))
        {
            if (length == 0) length = fileBytes.Length;
            using (var stream = new MemoryStream(fileBytes, offset, length))
            {
                return ReadARMP(stream, baseARMPMemoryAddress);
            }
        }



        /// <summary>
        /// Reads an armp file.
        /// </summary>
        /// <param name="path">The path to the armp file.</param>
        /// <param name="baseARMPMemoryAddress">The memory address where the armp starts at. Only needed if reading from memory.</param>
        /// <returns>An <see cref="ARMP"/> object.</returns>
        public static ARMP ReadARMP(string path, IntPtr baseARMPMemoryAddress = default(IntPtr))
        {
            using (Stream stream = new MemoryStream(File.ReadAllBytes(path)))
            {
                return ReadARMP(stream, baseARMPMemoryAddress);
            }
        }



        /// <summary>
        /// Reads a Dragon Engine armp table (base).
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="ptrTable">The pointer to the table.</param>
        /// <param name="baseARMPMemoryAddress">The memory address where the armp starts at. Only needed if reading from memory.</param>
        /// <returns>An <see cref="ArmpTableBase"/> object.</returns>
        private static ArmpTableBase ReadTableBase(BinaryReader reader, long ptrTable, Version version, long baseARMPMemoryAddress)
        {
            ArmpTableBase table = new ArmpTableBase();
            reader.BaseStream.Seek(ptrTable);
            table.TableInfo = GetARMPTableInfo(reader, false);
            table.TableInfo.FormatVersion = version;
            table.TableInfo.BaseARMPMemoryAddress = baseARMPMemoryAddress;


            ///// Column Names /////
            #region ColumnNames

            List<string> columnNames = new List<string>();
            if (table.TableInfo.HasColumnNames)
            {
                columnNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrColumnNamesOffsetTable, table.TableInfo.ColumnCount, false));
            }
            else
            {
                // Fill with numbers
                for (int c = 0; c < table.TableInfo.ColumnCount; c++)
                {
                    columnNames.Add(c.ToString());
                }
            }
            #endregion


            ///// Column Data Types /////
            List<ArmpType> columnDataTypes = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypes, table.TableInfo.ColumnCount, version, false);


            ///// Column Data Types Aux /////
            #region ColumnDataTypesAux
            List<List<int>> columnDataTypesAuxTable = new List<List<int>>();
            List<ArmpType> columnDataTypesAux = new List<ArmpType>();
            if (table.TableInfo.HasColumnDataTypesAux)
            {
                if (version == Version.DragonEngineV2)
                {
                    columnDataTypesAuxTable = GetColumnDataTypesAuxTable(reader, table.TableInfo.ptrColumnDataTypesAux, table.TableInfo.ColumnCount);
                    columnDataTypesAux = ColumnDataTypesAuxTableToColumnDataTypesAux(columnDataTypesAuxTable, version);
                }
                else
                {
                    columnDataTypesAux = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypesAux, table.TableInfo.ColumnCount, version, true);
                }
            }
            #endregion


            ///// Column Validity /////
            List<bool> columnValidity = new List<bool>();
            if (table.TableInfo.HasColumnValidity) columnValidity = Util.IterateBooleanBitmask(reader, table.TableInfo.ptrColumnValidity, table.TableInfo.ColumnCount, false);


            ///// Column Indices /////
            List<int> columnIndices = new List<int>();
            if (table.TableInfo.HasColumnIndices) columnIndices = Util.IterateArray<int>(reader, table.TableInfo.ptrColumnIndices, table.TableInfo.ColumnCount, false);


            ///// Column Metadata /////
            List<int> columnMetadata0x40 = new List<int>();
            if (table.TableInfo.HasColumnMetadata) columnMetadata0x40 = Util.IterateArray<int>(reader, table.TableInfo.ptrColumnMetadata, table.TableInfo.ColumnCount, false);


            ///// Create Columns /////
            #region CreateColumns

            for (uint c = 0; c < table.TableInfo.ColumnCount; c++)
            {
                ArmpTableColumn column = new ArmpTableColumn(c, columnNames[(int)c], columnDataTypes[(int)c]);

                if (table.TableInfo.HasColumnDataTypesAux)
                {
                    // Use the aux type as main for v1. This is better than having to check for the correct type multiple times.
                    if (version == Version.DragonEngineV1)
                    {
                        column.Type = columnDataTypesAux[(int)c];
                    }
                    else // v2
                    {
                        column.Type = columnDataTypes[(int)c];
                        column.Distance = columnDataTypesAuxTable[(int)c][1];
                        column.SpecialSize = columnDataTypesAuxTable[(int)c][2];
                    }
                }

                if (table.TableInfo.HasColumnValidity) column.IsValid = columnValidity[(int)c];
                if (table.TableInfo.HasColumnIndices) column.Index = columnIndices[(int)c];
                if (table.TableInfo.HasColumnMetadata) column.UnknownMetadata0x40 = columnMetadata0x40[(int)c];

                if (version == Version.DragonEngineV2)
                {
                    if (DataTypes.SpecialTypes.Contains(column.Type.CSType)) column.IsSpecial = true;
                    else column.IsSpecial = false;
                }

                table.Columns.Add(column);
            }


            // Assign special columns' children
            if (version == Version.DragonEngineV2)
            {
                foreach (ArmpTableColumn column in table.Columns)
                {
                    if (column.IsSpecial)
                    {
                        string substring = $"{column.Name}[";
                        foreach (ArmpTableColumn column2 in table.Columns)
                        {
                            if (column2.IsSpecial) continue;
                            if (column2.Name.Contains(substring))
                            {
                                column.Children.Add(column2);
                                column2.Parent = column;
                            }
                        }
                    }
                }
            }
            #endregion


            ///// Entry Names /////
            #region EntryNames

            if (table.TableInfo.HasEntryNames)
            {
                table.EntryNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrEntryNamesOffsetTable, table.TableInfo.EntryCount, false));
            }
            else
            {
                // Fill with blanks
                table.EntryNames = new List<string>();
                for (int r = 0; r < table.TableInfo.EntryCount; r++)
                {
                    table.EntryNames.Add("");
                }
            }
            #endregion


            ///// Text /////
            if (table.TableInfo.HasText) table.Text = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrTextOffsetTable, table.TableInfo.TextCount, false));


            ///// Entry Indices /////
            if (table.TableInfo.HasEntryIndices) table.EntryIndices = Util.IterateArray<int>(reader, table.TableInfo.ptrEntryIndices, table.TableInfo.EntryCount, false);


            ///// Entry Data /////
            InitializeEntries(table);
            ReadColumnContents(reader, table.TableInfo.ptrColumnContentOffsetTable, table.TableInfo.StorageMode, version, table);


            ///// Entry Validity /////
            #region EntryValidity

            if (table.TableInfo.HasEntryValidity)
            {
                table.EntryValidity = Util.IterateBooleanBitmask(reader, table.TableInfo.ptrEntryValidity, table.TableInfo.EntryCount, false);
                SetEntryValidity(table.EntryValidity, table.Entries);
            }
            #endregion


            ///// Extra Field Info /////
            #region ExtraFieldInfo

            if (table.TableInfo.HasExtraFieldInfo)
            {
                if (version == Version.DragonEngineV1) // Flags for each entry
                {
                    IterateEntryInfoFlags(reader, table.TableInfo.ptrExtraFieldInfo, table.Entries);
                }
                if (version == Version.DragonEngineV2)
                {
                    ReadColumnUnknownMetadata0x4C(reader, table.TableInfo.ptrExtraFieldInfo, table);
                }
            }
            #endregion


            ///// Empty Values /////
            #region EmptyValues

            if (table.TableInfo.HasEmptyValues)
            {
                List<uint> emptyValuesOffsetList = Util.IterateOffsetList(reader, table.TableInfo.ptrEmptyValuesOffsetTable, table.TableInfo.ColumnCount, false);

                int columnIndex = -1;
                foreach (uint offset in emptyValuesOffsetList)
                {
                    columnIndex++;
                    if (offset == 0xFFFFFFFF)
                    {
                        table.EmptyValuesIsNegativeOffset.Add(true);
                        continue;
                    }
                    else
                    {
                        table.EmptyValuesIsNegativeOffset.Add(false);
                        if (offset == 0) continue;

                        table.EmptyValues.Add(columnIndex, Util.IterateBooleanBitmask(reader, offset, table.TableInfo.EntryCount, false));
                    }
                }
            }
            #endregion


            table.RefreshColumnNameCache();
            return table;
        }



        /// <summary>
        /// Reads a Dragon Engine armp table.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="ptrTable">The pointer to the table.</param>
        /// <param name="version">The format version.</param>
        /// <param name="baseARMPMemoryAddress">The memory address where the armp starts at. Only needed if reading from memory.</param>
        /// <returns>An <see cref="ArmpTableBase"/> object.</returns>
        private static ArmpTable ReadTable(BinaryReader reader, long ptrTable, Version version, long baseARMPMemoryAddress)
        {
            ArmpTable mainTable = new ArmpTable(ReadTableBase(reader, ptrTable, version, baseARMPMemoryAddress));
            if (mainTable.TableInfo.HasIndexerTable) mainTable.Indexer = new ArmpTableIndexer(mainTable, ReadTableBase(reader, mainTable.TableInfo.ptrIndexerTable, version, baseARMPMemoryAddress));
            return mainTable;
        }



        /// <summary>
        /// Reads an Old Engine armp table (base).
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="version">The format version.</param>
        /// <returns>An <see cref="ArmpTableBase"/> object.</returns>
        private static ArmpTableBase ReadTableBaseOE(BinaryReader reader, Version version)
        {
            ArmpTableBase table = new ArmpTableBase();

            table.TableInfo = GetARMPTableInfo(reader, true);
            table.TableInfo.FormatVersion = version;


            ///// Column Names /////
            #region ColumnNames

            List<string> columnNames = new List<string>();
            if (table.TableInfo.HasColumnNames)
            {
                columnNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrColumnNamesOffsetTable, table.TableInfo.ColumnCount, true));
            }
            else
            {
                // Fill with numbers
                for (int c = 0; c < table.TableInfo.ColumnCount; c++)
                {
                    columnNames.Add(c.ToString());
                }
            }
            #endregion


            ///// Column Data Types /////
            List<ArmpType> columnDataTypes = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypes, table.TableInfo.ColumnCount, version);


            ///// Column Metadata /////
            #region ColumnMetadata

            List<int> columnMetadata = new List<int>();
            if (table.TableInfo.HasColumnMetadata)
            {
                columnMetadata = Util.IterateArray<int>(reader, table.TableInfo.ptrColumnMetadata, table.TableInfo.ColumnCount, true);
            }
            #endregion


            ///// Create Columns /////
            #region CreateColumns

            for (uint c = 0; c < table.TableInfo.ColumnCount; c++)
            {
                ArmpTableColumn column = new ArmpTableColumn(c, columnNames[(int)c], columnDataTypes[(int)c]);
                if (table.TableInfo.HasColumnMetadata) column.UnknownMetadata0x40 = columnMetadata[(int)c];

                table.Columns.Add(column);
            }
            #endregion


            ///// Entry Names /////
            #region EntryNames

            if (table.TableInfo.HasEntryNames)
            {
                table.EntryNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrEntryNamesOffsetTable, table.TableInfo.EntryCount, true));
            }
            else
            {
                // Fill with blanks
                table.EntryNames = new List<string>();
                for (int r = 0; r < table.TableInfo.EntryCount; r++)
                {
                    table.EntryNames.Add("");
                }
            }
            #endregion


            ///// Text /////
            #region Text

            if (table.TableInfo.HasText)
            {
                List<uint> offsetList = Util.IterateOffsetList(reader, table.TableInfo.ptrTextOffsetTable, table.TableInfo.TextCount, true);
                table.Text = Util.IterateStringList(reader, offsetList);
            }
            #endregion


            ///// Entry Data /////
            InitializeEntries(table);
            ReadColumnContents(reader, table.TableInfo.ptrColumnContentOffsetTable, table);


            ///// Entry Validity /////
            #region EntryValidity

            if (table.TableInfo.HasEntryValidity)
            {
                if (version == Version.OldEngineIshin)
                {
                    List<uint> tempList = Util.IterateArray<uint>(reader, table.TableInfo.ptrEntryValidity, table.TableInfo.EntryCount, true);
                    List<bool> boolList = new List<bool>();
                    foreach (uint integer in tempList)
                    {
                        boolList.Add(Convert.ToBoolean(integer));
                    }
                    table.EntryValidity = boolList;
                    SetEntryValidity(table.EntryValidity, table.Entries);
                }
                else
                {
                    table.EntryValidity = Util.IterateBooleanBitmask(reader, table.TableInfo.ptrEntryValidity, table.TableInfo.EntryCount, true);
                    SetEntryValidity(table.EntryValidity, table.Entries);
                }
            }
            #endregion


            table.RefreshColumnNameCache();
            return table;
        }



        /// <summary>
        /// Reads an Old Engine armp table.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="version">The format version.</param>
        /// <returns>An <see cref="ArmpTable"/> object.</returns>
        private static ArmpTable ReadTableOE(BinaryReader reader, Version version)
        {
            ArmpTableBase table = ReadTableBaseOE(reader, version);
            return new ArmpTable(table);
        }



        /// <summary>
        /// Reads the table information.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="IsOldEngine">If the format version is from Old Engine.</param>
        /// <returns>An <see cref="ArmpTableInfo"/> object.</returns>
        private static ArmpTableInfo GetARMPTableInfo(BinaryReader reader, bool IsOldEngine)
        {
            ArmpTableInfo armpTableInfo = new ArmpTableInfo();

            // Old Engine
            if (IsOldEngine)
            {
                armpTableInfo.EntryCount = reader.ReadInt32(true);
                // Ishin check
                uint check = reader.ReadUInt32(true);
                if (check > 0)
                {
                    armpTableInfo.FormatVersion = Version.OldEngineIshin;
                    armpTableInfo.ptrEntryValidity = check;
                }
                else
                {
                    armpTableInfo.FormatVersion = Version.OldEngine;
                }
                armpTableInfo.ptrEntryNamesOffsetTable = reader.ReadUInt32(true);
                if (armpTableInfo.FormatVersion == Version.OldEngine) armpTableInfo.ptrEntryValidity = reader.ReadUInt32(true);
                else reader.ReadBytes(0x4);
                armpTableInfo.ColumnCount = reader.ReadInt32(true);
                armpTableInfo.ptrColumnNamesOffsetTable = reader.ReadUInt32(true);
                armpTableInfo.ptrColumnDataTypes = reader.ReadUInt32(true);
                armpTableInfo.ptrColumnContentOffsetTable = reader.ReadUInt32(true);
                reader.ReadBytes(0x4);
                armpTableInfo.ptrColumnMetadata = reader.ReadUInt32(true);
                armpTableInfo.ptrTextOffsetTable = reader.ReadUInt32(true);
                armpTableInfo.TextCount = reader.ReadInt32(true);

                // Set flags
                if (armpTableInfo.TextCount > 0) armpTableInfo.HasText = true;
                if (armpTableInfo.ptrEntryNamesOffsetTable > 0 && armpTableInfo.ptrEntryNamesOffsetTable < 0xFFFFFFFF) armpTableInfo.HasEntryNames = true;
                if (armpTableInfo.ptrColumnNamesOffsetTable > 0 && armpTableInfo.ptrColumnNamesOffsetTable < 0xFFFFFFFF) armpTableInfo.HasColumnNames = true;
                if (armpTableInfo.ptrEntryValidity > 0 && armpTableInfo.ptrEntryValidity < 0xFFFFFFFF) armpTableInfo.HasEntryValidity = true;
                if (armpTableInfo.ptrColumnMetadata > 0 && armpTableInfo.ptrColumnMetadata < 0xFFFFFFFF) armpTableInfo.HasColumnMetadata = true;

#if DEBUG
                Console.WriteLine("Entry Count: " + armpTableInfo.EntryCount);
                Console.WriteLine("Column Count: " + armpTableInfo.ColumnCount);
                Console.WriteLine("Text Count: " + armpTableInfo.TextCount);
                Console.WriteLine("Pointer to Entry Names Offset Table: " + armpTableInfo.ptrEntryNamesOffsetTable);
                Console.WriteLine("Pointer to Entry Validity: " + armpTableInfo.ptrEntryValidity);
                Console.WriteLine("Pointer to Column Data Types: " + armpTableInfo.ptrColumnDataTypes);
                Console.WriteLine("Pointer to Column Content Offset Table: " + armpTableInfo.ptrColumnContentOffsetTable);
                Console.WriteLine("Pointer to Text Offset Table: " + armpTableInfo.ptrTextOffsetTable);
                Console.WriteLine("Pointer to Column Names Offset Table: " + armpTableInfo.ptrColumnNamesOffsetTable);
                Console.WriteLine("Pointer to Column Metadata: " + armpTableInfo.ptrColumnMetadata);
#endif
            }

            //Dragon Engine
            else
            {
                armpTableInfo.ptrMainTable = (uint)reader.BaseStream.Position; //DEBUG
                armpTableInfo.EntryCount = reader.ReadInt32();
                armpTableInfo.ColumnCount = reader.ReadInt32();
                armpTableInfo.TextCount = reader.ReadInt32();
                armpTableInfo.EntryValidator = reader.ReadInt32();
                armpTableInfo.ptrEntryNamesOffsetTable = reader.ReadUInt32();
                armpTableInfo.ptrEntryValidity = reader.ReadUInt32();
                armpTableInfo.ptrColumnDataTypes = reader.ReadUInt32();
                armpTableInfo.ptrColumnContentOffsetTable = reader.ReadUInt32();
                armpTableInfo.TableID = reader.ReadInt24();
                byte tableFlags = reader.ReadByte();
                armpTableInfo.ptrTextOffsetTable = reader.ReadUInt32();
                armpTableInfo.ptrColumnNamesOffsetTable = reader.ReadUInt32();
                armpTableInfo.ColumnValidator = reader.ReadInt32();
                armpTableInfo.ptrEntryIndices = reader.ReadUInt32();
                armpTableInfo.ptrColumnIndices = reader.ReadUInt32();
                armpTableInfo.ptrColumnValidity = reader.ReadUInt32();
                armpTableInfo.ptrIndexerTable = reader.ReadUInt32();
                armpTableInfo.ptrColumnMetadata = reader.ReadUInt32(); //This seems to be used as a band aid fix for when a column name has or starts with special characters. (minigame_karaoke_music_data -> ?karaoke_music_kind)
                armpTableInfo.ptrEmptyValuesOffsetTable = reader.ReadUInt32();
                armpTableInfo.ptrColumnDataTypesAux = reader.ReadUInt32();
                armpTableInfo.ptrExtraFieldInfo = reader.ReadUInt32();


                //Set flags
                armpTableInfo.StorageMode = (StorageMode)(tableFlags & (1 << 0));
                armpTableInfo.UnknownFlag1 = (tableFlags & (1 << 1)) != 0;
                armpTableInfo.UnknownFlag2 = (tableFlags & (1 << 2)) != 0;
                armpTableInfo.UnknownFlag3 = (tableFlags & (1 << 3)) != 0;
                armpTableInfo.UnknownFlag4 = (tableFlags & (1 << 4)) != 0;
                armpTableInfo.UnknownFlag5 = (tableFlags & (1 << 5)) != 0;
                armpTableInfo.UnknownFlag6 = (tableFlags & (1 << 6)) != 0;
                armpTableInfo.IsProcessedForMemory = (tableFlags & (1 << 7)) != 0;

                if (armpTableInfo.TextCount > 0) armpTableInfo.HasText = true;
                if (armpTableInfo.ptrIndexerTable > 0 && armpTableInfo.ptrIndexerTable < 0xFFFFFFFF) armpTableInfo.HasIndexerTable = true;
                if (armpTableInfo.ptrEntryNamesOffsetTable > 0 && armpTableInfo.ptrEntryNamesOffsetTable < 0xFFFFFFFF) armpTableInfo.HasEntryNames = true;
                if (armpTableInfo.ptrColumnNamesOffsetTable > 0 && armpTableInfo.ptrColumnNamesOffsetTable < 0xFFFFFFFF) armpTableInfo.HasColumnNames = true;
                if (armpTableInfo.ptrColumnDataTypesAux > 0 && armpTableInfo.ptrColumnDataTypesAux < 0xFFFFFFFF) armpTableInfo.HasColumnDataTypesAux = true;
                if (armpTableInfo.ptrEntryValidity > 0 && armpTableInfo.ptrEntryValidity < 0xFFFFFFFF) armpTableInfo.HasEntryValidity = true;
                if (armpTableInfo.ptrColumnValidity > 0 && armpTableInfo.ptrColumnValidity < 0xFFFFFFFF) armpTableInfo.HasColumnValidity = true;
                if (armpTableInfo.ptrEntryIndices > 0 && armpTableInfo.ptrEntryIndices < 0xFFFFFFFF) armpTableInfo.HasEntryIndices = true;
                if (armpTableInfo.ptrColumnIndices > 0 && armpTableInfo.ptrColumnIndices < 0xFFFFFFFF) armpTableInfo.HasColumnIndices = true;
                if (armpTableInfo.ptrEmptyValuesOffsetTable > 0 && armpTableInfo.ptrEmptyValuesOffsetTable < 0xFFFFFFFF) armpTableInfo.HasEmptyValues = true;
                if (armpTableInfo.ptrColumnMetadata > 0 && armpTableInfo.ptrColumnMetadata < 0xFFFFFFFF) armpTableInfo.HasColumnMetadata = true;
                if (armpTableInfo.ptrExtraFieldInfo > 0 && armpTableInfo.ptrExtraFieldInfo < 0xFFFFFFFF) armpTableInfo.HasExtraFieldInfo = true;


#if DEBUG
                Console.WriteLine("Entry Count: " + armpTableInfo.EntryCount);
                Console.WriteLine("Column Count: " + armpTableInfo.ColumnCount);
                Console.WriteLine("Text Count: " + armpTableInfo.TextCount);
                Console.WriteLine("Entry Validator: " + armpTableInfo.EntryValidator);
                Console.WriteLine("Pointer to Entry Names Offset Table: " + armpTableInfo.ptrEntryNamesOffsetTable);
                Console.WriteLine("Pointer to Entry Validity: " + armpTableInfo.ptrEntryValidity);
                Console.WriteLine("Pointer to Column Data Types: " + armpTableInfo.ptrColumnDataTypes);
                Console.WriteLine("Pointer to Column Content Offset Table: " + armpTableInfo.ptrColumnContentOffsetTable);
                Console.WriteLine("Table ID: " + armpTableInfo.TableID);
                Console.WriteLine("Storage Mode: " + armpTableInfo.StorageMode);
                Console.WriteLine("Unknown Flag 1: " + armpTableInfo.UnknownFlag1);
                Console.WriteLine("Unknown Flag 2: " + armpTableInfo.UnknownFlag2);
                Console.WriteLine("Unknown Flag 3: " + armpTableInfo.UnknownFlag3);
                Console.WriteLine("Unknown Flag 4: " + armpTableInfo.UnknownFlag4);
                Console.WriteLine("Unknown Flag 5: " + armpTableInfo.UnknownFlag5);
                Console.WriteLine("Unknown Flag 6: " + armpTableInfo.UnknownFlag6);
                Console.WriteLine("Flag 7 - IsProcessedForMemory: " + armpTableInfo.IsProcessedForMemory);
                Console.WriteLine("Pointer to Text Offset Table: " + armpTableInfo.ptrTextOffsetTable);
                Console.WriteLine("Pointer to Column Names Offset Table: " + armpTableInfo.ptrColumnNamesOffsetTable);
                Console.WriteLine("Column Validator: " + armpTableInfo.ColumnValidator);
                Console.WriteLine("Pointer to Entry Indices: " + armpTableInfo.ptrEntryIndices);
                Console.WriteLine("Pointer to Column Indices: " + armpTableInfo.ptrColumnIndices);
                Console.WriteLine("Pointer to Column Validity: " + armpTableInfo.ptrColumnValidity);
                Console.WriteLine("Pointer to Indexer table: " + armpTableInfo.ptrIndexerTable);
                Console.WriteLine("Pointer to Column Metadata: " + armpTableInfo.ptrColumnMetadata);
                Console.WriteLine("Pointer to Empty Values Offset Table: " + armpTableInfo.ptrEmptyValuesOffsetTable);
                Console.WriteLine("Pointer to Column Data Types Aux: " + armpTableInfo.ptrColumnDataTypesAux);
                Console.WriteLine("Pointer to Field Info: " + armpTableInfo.ptrExtraFieldInfo);
                Console.WriteLine("Has Indexer: " + armpTableInfo.HasIndexerTable);
#endif
            }

            return armpTableInfo;
        }



        /// <summary>
        /// Reads the column data types.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="ptrDataTypes">The pointer to the column data type array.</param>
        /// <param name="amount">The amount of values in the array.</param>
        /// <param name="version">The format version.</param>
        /// <param name="isAuxiliary">Is it the auxiliary data types array?</param>
        /// <returns>An <see cref="ArmpType"/> list.</returns>
        private static List<ArmpType> GetColumnDataTypes(BinaryReader reader, uint ptrDataTypes, int amount, Version version, bool isAuxiliary = false)
        {
            List<ArmpType> returnList = new List<ArmpType>();

            Dictionary<sbyte, ArmpType> typeIdCache = new Dictionary<sbyte, ArmpType>();
            foreach (ArmpType type in DataTypes.Types)
            {
                typeIdCache.TryAdd(type.GetID(version, isAuxiliary), type);
            }

            reader.BaseStream.Seek(ptrDataTypes);

            // OE type IDs are stored as int32
            if (version == Version.OldEngine || version == Version.OldEngineIshin)
            {
                for (int i = 0; i < amount; i++)
                {
                    sbyte id = (sbyte)reader.ReadInt32(true);
                    if (typeIdCache.ContainsKey(id))
                        returnList.Add(typeIdCache[id]);
                }
            }
            else
            {
                for (int i = 0; i < amount; i++)
                {
                    sbyte id = reader.ReadSByte();
                    if (typeIdCache.ContainsKey(id))
                        returnList.Add(typeIdCache[id]);
                }
            }

            return returnList;
        }



        /// <summary>
        /// Initializes the entries of a table.
        /// </summary>
        /// <param name="table">The <see cref="ArmpTableBase"/>.</param>
        private static void InitializeEntries(ArmpTableBase table)
        {
            for (uint i = 0; i < table.TableInfo.EntryCount; i++)
            {
                if (!table.TableInfo.HasEntryIndices)
                {
                    ArmpEntry entry = new ArmpEntry(table, i, table.EntryNames[(int)i]);
                    entry.ParentTable = table;
                    table.Entries.Add(entry);
                }
                else
                {
                    ArmpEntry entry = new ArmpEntry(table, i, table.EntryNames[(int)i], table.EntryIndices[(int)i]);
                    entry.ParentTable = table;
                    table.Entries.Add(entry);
                }
            }
        }



        /// <summary>
        /// Reads the column values for each entry.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="ptrOffsetTable">The pointer to the offset table.</param>
        /// <param name="storageMode">Storage mode used.</param>
        /// <param name="version">The format version.</param>
        /// <param name="table">The table where the data will be added to.</param>
        /// <remarks><para><b>DRAGON ENGINE ONLY</b></para></remarks>
        private static void ReadColumnContents(BinaryReader reader, uint ptrOffsetTable, StorageMode storageMode, Version version, ArmpTableBase table)
        {
            reader.BaseStream.Seek(ptrOffsetTable);

            if (storageMode == StorageMode.Column)
            {
                foreach (ArmpTableColumn column in table.Columns)
                {
                    int ptrData = reader.ReadInt32();
                    long nextPtr = reader.BaseStream.Position;
                    if (ptrData == -1 || ptrData == 0)
                    {
                        column.ShortcutType = (ColumnShortcutType)ptrData;
#if DEBUG
                        Console.WriteLine(String.Format("No data column -- > {0}", column.Name));
#endif
                    }
                    else if (ptrData > 0)
                    {
                        column.ShortcutType = ColumnShortcutType.None;
                        reader.BaseStream.Seek(ptrData);
#if DEBUG
                        Console.WriteLine(column.Name + " ----> " + column.Type);
#endif
                        // Only StorageMode.Column
                        List<bool> booleanColumnDataTemp = new List<bool>();
                        if (column.Type.CSType == typeof(bool))
                        {
                            booleanColumnDataTemp = Util.IterateBooleanBitmask(reader, (uint)reader.BaseStream.Position, table.TableInfo.EntryCount, false);
                        }

                        foreach (ArmpEntry entry in table.Entries)
                        {
                            entry.ColumnValueOffsets.Add(column.Name, (int)reader.BaseStream.Position);
                            ReadValue(reader, table, version, entry, column, booleanColumnDataTemp);
                        }

                    }

                    reader.BaseStream.Seek(nextPtr);
                }
            }


            else if (storageMode == StorageMode.Entry)
            {
                foreach (ArmpEntry entry in table.Entries)
                {
                    int ptrData = reader.ReadInt32();
                    long nextPtr = reader.BaseStream.Position;

                    foreach (ArmpTableColumn column in table.Columns)
                    {
                        reader.BaseStream.Seek(ptrData + column.Distance);
                        entry.ColumnValueOffsets.Add(column.Name, (int)reader.BaseStream.Position);
                        ReadValue(reader, table, version, entry, column);
                    }

                    reader.BaseStream.Seek(nextPtr);
                }
            }
        }



        /// <summary>
        /// Reads a value corresponding to an entry and column.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="table"><see cref="ArmpTableBase"/> to save the data to.</param>
        /// <param name="version">The format version.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="column">The column.</param>
        /// <param name="booleanColumnDataTemp">(Optional) The boolean column data if its using storage mode 0.</param>
        private static void ReadValue(BinaryReader reader, ArmpTableBase table, Version version, ArmpEntry entry, ArmpTableColumn column, List<bool> booleanColumnDataTemp = null)
        {
            if (column.Type.CSType == null) // Invalid
            {
                entry.Data.Add(column.Name, null);
            }

            else if (column.IsSpecial) // Array
            {
#if DEBUG
                Console.WriteLine(column.Name + " IS SPECIAL");
#endif
            }

            else if (column.Type.CSType == typeof(bool))
            {
                if (booleanColumnDataTemp != null)
                {
                    bool value = booleanColumnDataTemp[(int)entry.ID];
                    entry.Data.Add(column.Name, value);
                }
                else
                {
                    byte value = reader.ReadByte();
                    bool boolValue = true;
                    if (value == 0) boolValue = false;
                    entry.Data.Add(column.Name, boolValue);
                }
            }

            else if (column.Type.CSType == typeof(string))
            {
                // This should only run if we are loading the armp from memory
                if (table.TableInfo.IsProcessedForMemory)
                {
                    long ptrText = reader.ReadInt64();
                    ptrText = ptrText - table.TableInfo.BaseARMPMemoryAddress;
                    reader.BaseStream.PushToPosition(ptrText);
                    string text = reader.ReadString();
                    reader.BaseStream.PopPosition();
                    entry.Data.Add(column.Name, text);
                }
                else
                {
                    int index = reader.ReadInt32();
                    if (index != -1 && table.TableInfo.HasText) // Some files have valid string indices despite not having any text.
                        entry.Data.Add(column.Name, table.Text[index]);
                    else
                        entry.Data.Add(column.Name, null);
                }
            }

            else if (column.Type.CSType == typeof(ArmpTable))
            {
                long ptrTable = reader.ReadInt64();
                long currentpos = reader.BaseStream.Position;
                if (ptrTable == 0 || ptrTable == -1) return;

                // This should only run if we are loading the armp from memory
                if (table.TableInfo.IsProcessedForMemory)
                {
                    ptrTable = ptrTable - table.TableInfo.BaseARMPMemoryAddress;
                }

                ArmpTable tableValue = ReadTable(reader, ptrTable, version, table.TableInfo.BaseARMPMemoryAddress);

                entry.Data.Add(column.Name, tableValue);
                reader.BaseStream.Position = currentpos; // Reset position to the offset table
            }

            else if (column.Type.CSType == typeof(float))
            {
                float value = reader.ReadSingle();
                entry.Data[column.Name] = value;
            }

            else if (column.Type.CSType == typeof(double))
            {
                double value = reader.ReadDouble();
                entry.Data[column.Name] = value;
            }

            else if (column.Type.CSType == typeof(byte))
            {
                byte value = reader.ReadByte();
                entry.Data[column.Name] = value;
            }

            else if (column.Type.CSType == typeof(sbyte))
            {
                sbyte value = reader.ReadSByte();
                entry.Data[column.Name] = value;
            }

            else if (column.Type.CSType == typeof(UInt16))
            {
                UInt16 value = reader.ReadUInt16();
                entry.Data[column.Name] = value;
            }

            else if (column.Type.CSType == typeof(Int16))
            {
                Int16 value = reader.ReadInt16();
                entry.Data[column.Name] = value;
            }

            else if (column.Type.CSType == typeof(UInt32))
            {
                UInt32 value = reader.ReadUInt32();
                entry.Data[column.Name] = value;
            }

            else if (column.Type.CSType == typeof(Int32))
            {
                Int32 value = reader.ReadInt32();
                entry.Data[column.Name] = value;
            }

            else if (column.Type.CSType == typeof(UInt64))
            {
                UInt64 value = reader.ReadUInt64();
                entry.Data[column.Name] = value;
            }

            else if (column.Type.CSType == typeof(Int64))
            {
                Int64 value = reader.ReadInt64();
                entry.Data[column.Name] = value;
            }
        }



        /// <summary>
        /// Reads the column values for each entry.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="ptrOffsetTable">The pointer to the offset table.</param>
        /// <param name="table">The table where the data will be added to.</param>
        /// <remarks><para><b>OLD ENGINE ONLY</b></para></remarks>
        private static void ReadColumnContents(BinaryReader reader, uint ptrOffsetTable, ArmpTableBase table)
        {
            reader.BaseStream.Seek(ptrOffsetTable);

            foreach (ArmpTableColumn column in table.Columns)
            {
                uint ptrData = reader.ReadUInt32(true);
                long nextPtr = reader.BaseStream.Position;
                reader.BaseStream.Seek(ptrData);


                // Read operations based on column type
                if (column.Type.CSType == null)
                {
                    foreach (ArmpEntry entry in table.Entries)
                        entry.Data.Add(column.Name, null);
                }

                else if (column.Type.CSType == typeof(bool))
                {
                    List<bool> booleanColumnDataTemp = new List<bool>();
                    booleanColumnDataTemp = Util.IterateBooleanBitmask(reader, (uint)reader.BaseStream.Position, table.TableInfo.EntryCount, true);
                    foreach (ArmpEntry entry in table.Entries)
                        entry.Data.Add(column.Name, booleanColumnDataTemp[(int)entry.ID]);
                }

                else if (column.Type.CSType == typeof(byte))
                {
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        byte value = reader.ReadByte();
                        entry.Data.Add(column.Name, value);
                    }
                }

                else if (column.Type.CSType == typeof(sbyte))
                {
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        sbyte value = reader.ReadSByte();
                        entry.Data.Add(column.Name, value);
                    }
                }

                else if (column.Type.CSType == typeof(UInt16))
                {
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        UInt16 value = reader.ReadUInt16(true);
                        entry.Data.Add(column.Name, value);
                    }
                }

                else if (column.Type.CSType == typeof(Int16))
                {
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        Int16 value = reader.ReadInt16(true);
                        entry.Data.Add(column.Name, value);
                    }
                }

                else if (column.Type.CSType == typeof(UInt32))
                {
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        UInt32 value = reader.ReadUInt32(true);
                        entry.Data.Add(column.Name, value);
                    }
                }

                else if (column.Type.CSType == typeof(Int32))
                {
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        Int32 value = reader.ReadInt32(true);
                        entry.Data.Add(column.Name, value);
                    }
                }

                reader.BaseStream.Seek(nextPtr);
            }
        }



        /// <summary>
        /// Sets the "Valid" flag for a list of entries.
        /// </summary>
        /// <param name="entryValidity">The validity list.</param>
        /// <param name="entries">The entry list to update.</param>
        private static void SetEntryValidity(List<bool> entryValidity, List<ArmpEntry> entries)
        {
            int iter = 0;
            foreach (bool validity in entryValidity)
            {
                entries[iter].IsValid = validity;
                iter++;
            }
        }



        /// <summary>
        /// Reads the Entry Info Flags. 
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="ptrArray">The pointer to the array.</param>
        /// <param name="entries">The entry list.</param>
        /// <remarks><para><b>DRAGON ENGINE V1 ONLY</b></para></remarks>
        private static void IterateEntryInfoFlags(BinaryReader reader, uint ptrArray, List<ArmpEntry> entries)
        {
            reader.BaseStream.Seek(ptrArray);

            for (int i = 0; i < entries.Count; i++)
            {
                byte b = reader.ReadByte();
                var bitstring = Convert.ToString(b, 2).PadLeft(8, '0');
                bitstring = Util.ReverseString(bitstring);

                ArmpEntry entry = entries[i];
                entry.Flags = new bool[8];
                int iter = 0;
                foreach (char c in bitstring)
                {
                    bool boolvalue = false;
                    if (c == '1') boolvalue = true;
                    entry.Flags[iter] = boolvalue;
                    iter++;
                }
            }
        }



        /// <summary>
        /// Reads the additional Column Metadata.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="ptrEntryInfo">The pointer to the Entry Info section.</param>
        /// <param name="table">The table.</param>
        /// <remarks><para><b>DRAGON ENGINE V2 ONLY</b></para></remarks>
        private static void ReadColumnUnknownMetadata0x4C(BinaryReader reader, uint ptrEntryInfo, ArmpTableBase table)
        {
            reader.BaseStream.Seek(ptrEntryInfo);

            for (int i = 0; i < table.Columns.Count; i++)
            {
                int size = reader.ReadInt32();
                int ptrMetadata = reader.ReadInt32();
                reader.ReadBytes(0x18); // Padding

                if (size > 0)
                {
                    ArmpTableColumn column = table.Columns[i];
                    reader.BaseStream.PushToPosition(ptrMetadata);

                    foreach (ArmpTableColumn child in column.Children)
                    {
                        child.UnknownMetadata0x4C = reader.ReadInt32();
                    }

                    reader.BaseStream.PopPosition();
                }
            }
        }



        /// <summary>
        /// Reads the Column Data Types auxiliary table.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/>.</param>
        /// <param name="ptrTable">The pointer to the auxiliary table.</param>
        /// <param name="columnAmount">The amount of columns in the table.</param>
        private static List<List<int>> GetColumnDataTypesAuxTable(BinaryReader reader, uint ptrTable, int columnAmount)
        {
            reader.BaseStream.Seek(ptrTable);
            List<List<int>> ColumnDataTypesAuxTable = new List<List<int>>();

            for (int i = 0; i < columnAmount; i++)
            {
                List<int> list = new List<int>();
                list.Add(reader.ReadInt32()); // Type
                list.Add(reader.ReadInt32()); // Distance
                list.Add(reader.ReadInt32()); // Array Size
                list.Add(reader.ReadInt32()); // Unknown
                ColumnDataTypesAuxTable.Add(list);
            }

            return ColumnDataTypesAuxTable;
        }



        /// <summary>
        /// Generates an auxiliary type list from the IDs in the aux table.
        /// </summary>
        /// <param name="columnDataTypesAuxTable">The Column Data Types Auxiliary MainTable.</param>
        /// <param name="version">The format version.</param>
        /// <returns>A <see cref="ArmpType"/> list.</returns>
        /// <remarks><para><b>DRAGON ENGINE V2 ONLY</b></para></remarks>
        private static List<ArmpType> ColumnDataTypesAuxTableToColumnDataTypesAux(List<List<int>> columnDataTypesAuxTable, Version version)
        {
            List<ArmpType> typesList = new List<ArmpType>();

            for (int i = 0; i < columnDataTypesAuxTable.Count; i++)
            {
                sbyte id = Convert.ToSByte(columnDataTypesAuxTable[i][0]);
                foreach (ArmpType armpType in DataTypes.Types)
                {
                    if (armpType.GetIDAux(version) == id)
                    {
                        typesList.Add(armpType);
                        break;
                    }
                }
            }

            return typesList;
        }
    }
}

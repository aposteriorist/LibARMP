using System;
using System.Text;
using System.Collections.Generic;
using Yarhl.IO;
using System.IO;
using System.Reflection;

namespace LibARMP
{
    public static class ArmpFileReader
    {

        internal static Dictionary<Type, MethodInfo> ReadTypeCache = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// Reads an armp file.
        /// </summary>
        /// <param name="datastream">The armp file as DataStream.</param>
        /// <returns>An ARMP object.</returns>
        public static ARMP ReadARMP (DataStream datastream)
        {
            var reader = new DataReader(datastream)
            {
                Endianness = EndiannessMode.LittleEndian,
                DefaultEncoding = System.Text.Encoding.UTF8,
            };

            ARMP armp = new ARMP();

            char[] magic = reader.ReadChars(4);
            int endianess = reader.ReadInt32(); //Only used in OE
            if (endianess == 258)
            {
                reader.Endianness = EndiannessMode.BigEndian;
                reader.DefaultEncoding = System.Text.Encoding.GetEncoding(932);
                armp.IsOldEngine = true;
            }

            if (armp.IsOldEngine)
            {
                armp.Version = reader.ReadInt16();
                armp.Revision = reader.ReadInt16();
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

            int fileSize = reader.ReadInt32(); //Only used in OE

            if (armp.IsOldEngine)
            {
                armp.MainTable = ReadTableMainOE(reader);
            }
            else
            {
                uint ptrMainTable = reader.ReadUInt32();
                armp.MainTable = ReadTableMain(reader, ptrMainTable, armp.Version);
            }

            datastream.WriteTo(armp.File);
            return armp;
        }



        /// <summary>
        /// Reads an armp file.
        /// </summary>
        /// <param name="fileBytes">The armp file as byte array.</param>
        /// <param name="offset">The location in the array to start reading data from.</param>
        /// <param name="length">The number of bytes to read from the array.</param>
        /// <returns>An ARMP object.</returns>
        public static ARMP ReadARMP (byte[] fileBytes, int offset=0, int length=0)
        {
            if (length == 0) length = fileBytes.Length;
            using (var datastream = DataStreamFactory.FromArray(fileBytes, offset, length))
            {
                return ReadARMP(datastream);
            }
        }



        /// <summary>
        /// Reads an armp file.
        /// </summary>
        /// <param name="stream">The armp file as stream.</param>
        /// <returns>An ARMP object.</returns>
        public static ARMP ReadARMP (Stream stream)
        {
            using (var datastream = DataStreamFactory.FromStream(stream))
            {
                return ReadARMP(datastream);
            }
        }



        /// <summary>
        /// Reads an armp file.
        /// </summary>
        /// <param name="path">The path to the armp file.</param>
        /// <returns>An ARMP object.</returns>
        public static ARMP ReadARMP (string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Read))
            {
                return ReadARMP(datastream);
            }
        }



        /// <summary>
        /// Reads a Dragon Engine armp table.
        /// </summary>
        /// <param name="reader">The path to the armp file.</param>
        /// <param name="ptrMainTable">The pointer to the main table.</param>
        /// <returns>An ArmpTable object.</returns>
        private static ArmpTable ReadTable (DataReader reader, long ptrMainTable, int version)
        {
            ArmpTable table = new ArmpTable();

            reader.Stream.Seek(ptrMainTable);
            table.TableInfo = GetARMPTableInfo(reader, false);
            if (version == 2) table.TableInfo.IsDragonEngineV2 = true;

            //Read general data

            //Column names
            List<string> columnNames = new List<string>();
            if (table.TableInfo.HasColumnNames)
            {
                columnNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrColumnNamesOffsetTable, table.TableInfo.ColumnCount));
            }
            else
            {
                //Fill with numbers
                for (int c = 0; c < table.TableInfo.ColumnCount; c++)
                {
                    columnNames.Add(c.ToString());
                }
            }

            //Column Data Types
            List<Type> columnDataTypes = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypes, table.TableInfo.ColumnCount, version, false);

            //Column Data Types Aux
            List<List<int>> columnDataTypesAuxTable = new List<List<int>>();
            List<Type> columnDataTypesAux = new List<Type>();
            if (table.TableInfo.HasColumnDataTypesAux)
            {
                if (version == 2)
                {
                    columnDataTypesAuxTable = GetColumnDataTypesAuxTable(reader, table.TableInfo.ptrColumnDataTypesAux, table.TableInfo.ColumnCount);
                    columnDataTypesAux = ColumnDataTypesAuxTableToColumnDataTypesAux(columnDataTypesAuxTable);
                }
                else
                {
                    columnDataTypesAux = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypesAux, table.TableInfo.ColumnCount, version, true);
                }
            }

            //Column Validity
            List<bool> columnValidity = new List<bool>();
            if (table.TableInfo.HasColumnValidity) columnValidity = Util.IterateBooleanBitmask(reader, table.TableInfo.ptrColumnValidity, table.TableInfo.ColumnCount);
            //Column Indices
            List<int> columnIndices = new List<int>();
            if (table.TableInfo.HasColumnIndices) columnIndices = Util.IterateArray<int>(reader, table.TableInfo.ptrColumnIndices, table.TableInfo.ColumnCount);
            //Column Metadata
            List<int> columnMetadata0x40 = new List<int>();
            if (table.TableInfo.HasColumnMetadata) columnMetadata0x40 = Util.IterateArray<int>(reader, table.TableInfo.ptrColumnMetadata, table.TableInfo.ColumnCount);


            //Create columns
            for (int c = 0; c < table.TableInfo.ColumnCount; c++)
            {
                ArmpTableColumn column = new ArmpTableColumn(c, columnNames[c], columnDataTypes[c]);
                
                if (table.TableInfo.HasColumnDataTypesAux)
                {
                    //Use the aux type as main for v1. This is better than having to check for the correct type multiple times.
                    if (version == 1)
                    {
                        column.ColumnType = columnDataTypesAux[c];
                        column.ColumnTypeAux = columnDataTypes[c];
                    }
                    else //v2
                    {
                        column.ColumnTypeAux = columnDataTypesAux[c];
                        column.Distance = columnDataTypesAuxTable[c][1];
                        column.SpecialSize = columnDataTypesAuxTable[c][2];
                    }
                }

                if (table.TableInfo.HasColumnValidity) column.IsValid = columnValidity[c];
                if (table.TableInfo.HasColumnIndices) column.Index = columnIndices[c];
                if (table.TableInfo.HasColumnMetadata) column.UnknownMetadata0x40 = columnMetadata0x40[c];

                if (version == 2)
                {
                    if (DataTypes.TypesV2Specials.Contains(column.ColumnType)) column.IsSpecial = true;
                    else column.IsSpecial = false;
                }

                table.Columns.Add(column);
            }


            //Assign special columns' children
            if (version == 2)
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



            //Row names
            if (table.TableInfo.HasRowNames)
            {
                table.RowNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrRowNamesOffsetTable, table.TableInfo.RowCount));
            }
            else
            {
                //Fill with blanks
                table.RowNames = new List<string>();
                for (int r = 0; r < table.TableInfo.RowCount; r++)
                {
                    table.RowNames.Add("");
                }
            }

            //Text
            if (table.TableInfo.HasText) table.Text = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrTextOffsetTable, table.TableInfo.TextCount));

            //Row Indices
            if (table.TableInfo.HasRowIndices) table.RowIndices = Util.IterateArray<int>(reader, table.TableInfo.ptrRowIndices, table.TableInfo.RowCount);

            InitializeEntries(table);
            ReadEntryData(reader, table.TableInfo.ptrColumnContentOffsetTable, table.TableInfo.StorageMode, version, table);

            //Row Validity
            if (table.TableInfo.HasRowValidity)
            {
                table.RowValidity = Util.IterateBooleanBitmask(reader, table.TableInfo.ptrRowValidity, table.TableInfo.RowCount);
                SetRowValidity(table.RowValidity, table.Entries);
            }

            //Extra Field Info
            if (table.TableInfo.HasExtraFieldInfo)
            {
                if (version == 1) //Flags for each entry
                {
                    IterateEntryInfoFlags(reader, table.TableInfo.ptrExtraFieldInfo, table.Entries);
                }
                if (version == 2)
                {
                    ReadColumnUnknownMetadata0x4C(reader, table.TableInfo.ptrExtraFieldInfo, table);
                }
            }

            if (table.TableInfo.HasEmptyValues)
            {
                List<uint> emptyValuesOffsetList = Util.IterateOffsetList(reader, table.TableInfo.ptrEmptyValuesOffsetTable, table.TableInfo.ColumnCount);

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

                        table.EmptyValues.Add(columnIndex, Util.IterateBooleanBitmask(reader, offset, table.TableInfo.RowCount));
                    }
                }
            }

            table.RefreshColumnNameCache();
            return table;
        }



        /// <summary>
        /// Reads a Dragon Engine armp table.
        /// </summary>
        /// <param name="reader">The path to the armp file.</param>
        /// <param name="ptrMainTable">The pointer to the main table.</param>
        /// <param name="version">The version number.</param>
        /// <returns>An ArmpTable object.</returns>
        private static ArmpTableMain ReadTableMain (DataReader reader, long ptrMainTable, int version)
        {
            ArmpTableMain mainTable = new ArmpTableMain(ReadTable(reader, ptrMainTable, version));
            if (mainTable.TableInfo.HasSubTable) mainTable.SubTable = new ArmpTableSub(mainTable, ReadTable(reader, mainTable.TableInfo.ptrSubTable, version));
            return mainTable;
        }



        /// <summary>
        /// Reads an Old Engine armp table.
        /// </summary>
        /// <param name="reader">The DataReader.</param>
        /// <returns>An ArmpTable object.</returns>
        private static ArmpTable ReadTable (DataReader reader)
        {
            //TODO column metadata, text
            ArmpTable table = new ArmpTable();

            table.TableInfo = GetARMPTableInfo(reader, true);

            //Read general data

            //Column names
            List<string> columnNames = new List<string>();
            if (table.TableInfo.HasColumnNames)
            {
                columnNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrColumnNamesOffsetTable, table.TableInfo.ColumnCount));
            }
            else
            {
                //Fill with numbers
                for (int c = 0; c < table.TableInfo.ColumnCount; c++)
                {
                    columnNames.Add(c.ToString());
                }
            }

            List<Type> columnDataTypes = new List<Type>();
            //Column Data Types
            columnDataTypes = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypes, table.TableInfo.ColumnCount);

            //Column Metadata
            List<int> columnMetadata = new List<int>();
            if (table.TableInfo.HasColumnMetadata)
            {
                columnMetadata = Util.IterateArray<int>(reader, table.TableInfo.ptrColumnMetadata, table.TableInfo.ColumnCount);
            }

            //Create columns
            for (int c = 0; c < table.TableInfo.ColumnCount; c++)
            {
                ArmpTableColumn column = new ArmpTableColumn(c, columnNames[c], columnDataTypes[c]);
                if (table.TableInfo.HasColumnMetadata) column.UnknownMetadata0x40 = columnMetadata[c];

                table.Columns.Add(column);
            }


            //Row names
            if (table.TableInfo.HasRowNames)
            {
                table.RowNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrRowNamesOffsetTable, table.TableInfo.RowCount));
            }
            else
            {
                //Fill with blanks
                table.RowNames = new List<string>();
                for (int r = 0; r < table.TableInfo.RowCount; r++)
                {
                    table.RowNames.Add("");
                }
            }

            if (table.TableInfo.HasText)
            {
                List<uint> offsetList = Util.IterateOffsetList(reader, table.TableInfo.ptrTextOffsetTable, table.TableInfo.TextCount);
                //Change encoding to utf8 to read the strings and convert to sjis later. Reading directly as sjis results in broken text (????)
                Encoding utf8 = Encoding.UTF8;
                Encoding sjis = Encoding.GetEncoding(932);
                reader.DefaultEncoding = utf8;
                table.Text = Util.IterateStringList(reader, offsetList);
                reader.DefaultEncoding = sjis;
                for (int i=0; i<table.Text.Count; i++)
                {
                    byte[] utfBytes = utf8.GetBytes(table.Text[i]);
                    byte[] sjisBytes = Encoding.Convert(utf8, sjis, utfBytes);
                    table.Text[i] = sjis.GetString(sjisBytes);
                }
            }

            InitializeEntries(table);
            ReadEntryData(reader, table.TableInfo.ptrColumnContentOffsetTable, table);

            //Row Validity
            if (table.TableInfo.HasRowValidity)
            {
                //TODO Ishin
                table.RowValidity = Util.IterateBooleanBitmask(reader, table.TableInfo.ptrRowValidity, table.TableInfo.RowCount);
                SetRowValidity(table.RowValidity, table.Entries);
            }

            table.RefreshColumnNameCache();
            return table;
        }



        /// <summary>
        /// Reads an Old Engine armp's main table.
        /// </summary>
        /// <param name="reader">The DataReader.</param>
        /// <returns>An ArmpTableMain object.</returns>
        private static ArmpTableMain ReadTableMainOE (DataReader reader)
        {
            ArmpTable table = ReadTable(reader);
            return new ArmpTableMain(table);
        }



        /// <summary>
        /// Reads the table data and returns an ArmpTableInfo object.
        /// </summary>
        /// <param name="reader">The DataStream reader</param>
        private static ArmpTableInfo GetARMPTableInfo (DataReader reader, bool IsOldEngine)
        {
            ArmpTableInfo armpTableInfo = new ArmpTableInfo();

            //Old Engine
            if (IsOldEngine)
            {
                armpTableInfo.RowCount = reader.ReadInt32();
                //Ishin check
                uint check = reader.ReadUInt32();
                if (check > 0)
                {
                    armpTableInfo.IsIshin = true;
                    armpTableInfo.ptrRowValidity = check;
                }
                else
                {
                    armpTableInfo.IsOldEngine = true;
                }
                armpTableInfo.ptrRowNamesOffsetTable = reader.ReadUInt32();
                if (armpTableInfo.IsOldEngine) armpTableInfo.ptrRowValidity = reader.ReadUInt32();
                else reader.ReadBytes(0x4);
                armpTableInfo.ColumnCount = reader.ReadInt32();
                armpTableInfo.ptrColumnNamesOffsetTable = reader.ReadUInt32();
                armpTableInfo.ptrColumnDataTypes = reader.ReadUInt32();
                armpTableInfo.ptrColumnContentOffsetTable = reader.ReadUInt32();
                reader.ReadBytes(0x4);
                armpTableInfo.ptrColumnMetadata = reader.ReadUInt32();
                armpTableInfo.ptrTextOffsetTable = reader.ReadUInt32();
                armpTableInfo.TextCount = reader.ReadInt32();

                //Set flags
                if (armpTableInfo.TextCount > 0) armpTableInfo.HasText = true;
                if (armpTableInfo.ptrRowNamesOffsetTable > 0 && armpTableInfo.ptrRowNamesOffsetTable < 0xFFFFFFFF) armpTableInfo.HasRowNames = true;
                if (armpTableInfo.ptrColumnNamesOffsetTable > 0 && armpTableInfo.ptrColumnNamesOffsetTable < 0xFFFFFFFF) armpTableInfo.HasColumnNames = true;
                if (armpTableInfo.ptrRowValidity > 0 && armpTableInfo.ptrRowValidity < 0xFFFFFFFF) armpTableInfo.HasRowValidity = true;
                if (armpTableInfo.ptrColumnMetadata > 0 && armpTableInfo.ptrColumnMetadata < 0xFFFFFFFF) armpTableInfo.HasColumnMetadata = true;

#if DEBUG
                Console.WriteLine("Row Count: " + armpTableInfo.RowCount);
                Console.WriteLine("Column Count: " + armpTableInfo.ColumnCount);
                Console.WriteLine("Text Count: " + armpTableInfo.TextCount);
                Console.WriteLine("Pointer to Row Names Offset Table: " + armpTableInfo.ptrRowNamesOffsetTable);
                Console.WriteLine("Pointer to Row Validity: " + armpTableInfo.ptrRowValidity);
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
                armpTableInfo.ptrMainTable = (uint)reader.Stream.Position; //DEBUG
                armpTableInfo.RowCount = reader.ReadInt32();
                armpTableInfo.ColumnCount = reader.ReadInt32();
                armpTableInfo.TextCount = reader.ReadInt32();
                armpTableInfo.RowValidator = reader.ReadInt32();
                armpTableInfo.ptrRowNamesOffsetTable = reader.ReadUInt32();
                armpTableInfo.ptrRowValidity = reader.ReadUInt32();
                armpTableInfo.ptrColumnDataTypes = reader.ReadUInt32();
                armpTableInfo.ptrColumnContentOffsetTable = reader.ReadUInt32();
                armpTableInfo.TableID = reader.ReadInt24();
                armpTableInfo.StorageMode = reader.ReadByte();
                armpTableInfo.ptrTextOffsetTable = reader.ReadUInt32();
                armpTableInfo.ptrColumnNamesOffsetTable = reader.ReadUInt32();
                armpTableInfo.ColumnValidator = reader.ReadInt32();
                armpTableInfo.ptrRowIndices = reader.ReadUInt32();
                armpTableInfo.ptrColumnIndices = reader.ReadUInt32();
                armpTableInfo.ptrColumnValidity = reader.ReadUInt32();
                armpTableInfo.ptrSubTable = reader.ReadUInt32();
                armpTableInfo.ptrColumnMetadata = reader.ReadUInt32(); //This seems to be used as a band aid fix for when a column name has or starts with special characters. (minigame_karaoke_music_data -> ?karaoke_music_kind)
                armpTableInfo.ptrEmptyValuesOffsetTable = reader.ReadUInt32();
                armpTableInfo.ptrColumnDataTypesAux = reader.ReadUInt32();
                armpTableInfo.ptrExtraFieldInfo = reader.ReadUInt32();


                //Set flags
                if (armpTableInfo.TextCount > 0) armpTableInfo.HasText = true;
                if (armpTableInfo.ptrSubTable > 0 && armpTableInfo.ptrSubTable < 0xFFFFFFFF) armpTableInfo.HasSubTable = true;
                if (armpTableInfo.ptrRowNamesOffsetTable > 0 && armpTableInfo.ptrRowNamesOffsetTable < 0xFFFFFFFF) armpTableInfo.HasRowNames = true;
                if (armpTableInfo.ptrColumnNamesOffsetTable > 0 && armpTableInfo.ptrColumnNamesOffsetTable < 0xFFFFFFFF) armpTableInfo.HasColumnNames = true;
                if (armpTableInfo.ptrColumnDataTypesAux > 0 && armpTableInfo.ptrColumnDataTypesAux < 0xFFFFFFFF) armpTableInfo.HasColumnDataTypesAux = true;
                if (armpTableInfo.ptrRowValidity > 0 && armpTableInfo.ptrRowValidity < 0xFFFFFFFF) armpTableInfo.HasRowValidity = true;
                if (armpTableInfo.ptrColumnValidity > 0 && armpTableInfo.ptrColumnValidity < 0xFFFFFFFF) armpTableInfo.HasColumnValidity = true;
                if (armpTableInfo.ptrRowIndices > 0 && armpTableInfo.ptrRowIndices < 0xFFFFFFFF) armpTableInfo.HasRowIndices = true;
                if (armpTableInfo.ptrColumnIndices > 0 && armpTableInfo.ptrColumnIndices < 0xFFFFFFFF) armpTableInfo.HasColumnIndices = true;
                if (armpTableInfo.ptrEmptyValuesOffsetTable > 0 && armpTableInfo.ptrEmptyValuesOffsetTable < 0xFFFFFFFF) armpTableInfo.HasEmptyValues = true;
                if (armpTableInfo.ptrColumnMetadata > 0 && armpTableInfo.ptrColumnMetadata < 0xFFFFFFFF) armpTableInfo.HasColumnMetadata = true;
                if (armpTableInfo.ptrExtraFieldInfo > 0 && armpTableInfo.ptrExtraFieldInfo < 0xFFFFFFFF) armpTableInfo.HasExtraFieldInfo = true;


#if DEBUG
                Console.WriteLine("Row Count: " + armpTableInfo.RowCount);
                Console.WriteLine("Column Count: " + armpTableInfo.ColumnCount);
                Console.WriteLine("Text Count: " + armpTableInfo.TextCount);
                Console.WriteLine("Row Validator: " + armpTableInfo.RowValidator);
                Console.WriteLine("Pointer to Row Names Offset Table: " + armpTableInfo.ptrRowNamesOffsetTable);
                Console.WriteLine("Pointer to Row Validity: " + armpTableInfo.ptrRowValidity);
                Console.WriteLine("Pointer to Column Data Types: " + armpTableInfo.ptrColumnDataTypes);
                Console.WriteLine("Pointer to Column Content Offset Table: " + armpTableInfo.ptrColumnContentOffsetTable);
                Console.WriteLine("Table ID: " + armpTableInfo.TableID);
                Console.WriteLine("Storage Mode: " + armpTableInfo.StorageMode);
                Console.WriteLine("Pointer to Text Offset Table: " + armpTableInfo.ptrTextOffsetTable);
                Console.WriteLine("Pointer to Column Names Offset Table: " + armpTableInfo.ptrColumnNamesOffsetTable);
                Console.WriteLine("Column Validator: " + armpTableInfo.ColumnValidator);
                Console.WriteLine("Pointer to Row Indices: " + armpTableInfo.ptrRowIndices);
                Console.WriteLine("Pointer to Column Indices: " + armpTableInfo.ptrColumnIndices);
                Console.WriteLine("Pointer to Column Validity: " + armpTableInfo.ptrColumnValidity);
                Console.WriteLine("Pointer to SubTable: " + armpTableInfo.ptrSubTable);
                Console.WriteLine("Pointer to Column Metadata: " + armpTableInfo.ptrColumnMetadata);
                Console.WriteLine("Pointer to Empty Values Offset Table: " + armpTableInfo.ptrEmptyValuesOffsetTable);
                Console.WriteLine("Pointer to Column Data Types Aux: " + armpTableInfo.ptrColumnDataTypesAux);
                Console.WriteLine("Pointer to Field Info: " + armpTableInfo.ptrExtraFieldInfo);
                Console.WriteLine("Has SubTable: " + armpTableInfo.HasSubTable);
#endif
            }

            return armpTableInfo;
        }



        /// <summary>
        /// Reads the column data types. (DRAGON ENGINE ONLY)
        /// </summary>
        /// <param name="reader">The DataStream Reader.</param>
        /// <param name="ptrDataTypes">The pointer to the column data type array.</param>
        /// <param name="amount">The amount of values in the array.</param>
        /// <param name="version">The armp version.</param>
        /// <param name="isAuxiliary">Is it the auxiliary data types array?</param>
        /// <returns>A types list.</returns>
        private static List<Type> GetColumnDataTypes (DataReader reader, UInt32 ptrDataTypes, int amount, int version, bool isAuxiliary)
        {
            List<Type> returnList = new List<Type>();
            IDictionary<sbyte, Type> typesDictionary = new Dictionary<sbyte, Type>();

            reader.Stream.Seek(ptrDataTypes);

            //v1
            if (version == 1 && !isAuxiliary) typesDictionary = DataTypes.TypesV1;
            else if (version == 1 && isAuxiliary) typesDictionary = DataTypes.TypesV1Aux;

            //v2
            if (version == 2) typesDictionary = DataTypes.TypesV2;

            for (int i = 0; i < amount; i++)
            {
                sbyte id = reader.ReadSByte();
                returnList.Add(typesDictionary[id]);
            }

            return returnList;
        }


        /// <summary>
        /// Reads the column data types. (OLD ENGINE ONLY)
        /// </summary>
        /// <param name="reader">The DataStream Reader.</param>
        /// <param name="ptrDataTypes">The pointer to the column data type array.</param>
        /// <param name="amount">The amount of values in the array.</param>
        /// <returns>A types list.</returns>
        private static List<Type> GetColumnDataTypes (DataReader reader, UInt32 ptrDataTypes, int amount)
        {
            List<Type> returnList = new List<Type>();
            IDictionary<int, Type> typesDictionary = new Dictionary<int, Type>();

            reader.Stream.Seek(ptrDataTypes);

            typesDictionary = DataTypes.TypesOE;

            for (int i = 0; i < amount; i++)
            {
                int id = reader.ReadInt32();
                returnList.Add(typesDictionary[id]);
            }

            return returnList;
        }


        /// <summary>
        /// Initializes the entries of a table.
        /// </summary>
        /// <param name="table">The ArmpTable.</param>
        private static void InitializeEntries (ArmpTable table)
        {
            for (int i=0; i<table.TableInfo.RowCount; i++)
            {
                if (!table.TableInfo.HasRowIndices)
                {
                    ArmpEntry entry = new ArmpEntry(i, table.RowNames[i]);
                    table.Entries.Add(entry);
                }
                else
                {
                    ArmpEntry entry = new ArmpEntry(i, table.RowNames[i], table.RowIndices[i]);
                    table.Entries.Add(entry);
                }
            }
        }



        /// <summary>
        /// Reads a value of type T and stores it in the corresponding entry.
        /// </summary>
        /// <typeparam name="T">The type to read.</typeparam>
        /// <param name="reader">The DataReader.</param>
        /// <param name="table">The ArmpTable to store the read value.</param>
        /// <param name="rowIndex">Row to insert the value into.</param>
        /// <param name="columnIndex">Column to insert the value into.</param>
        private static void ReadType<T> (DataReader reader, ArmpTable table, int rowIndex, int columnIndex)
        {
            T value = reader.Read<T>();
            table.Entries[rowIndex].Data.Add(table.GetColumnName(columnIndex), value);
        }



        /// <summary>
        /// Reads the column values for each entry. (DRAGON ENGINE ONLY)
        /// </summary>
        /// <param name="reader">The DataReader.</param>
        /// <param name="ptrOffsetTable">The pointer to the offset table.</param>
        /// <param name="storageMode">Storage mode used (0 = per column, 1 = per row).</param>
        /// <param name="version">Version of the format.</param>
        /// <param name="table">The table where the data will be added to.</param>
        private static void ReadEntryData (DataReader reader, UInt32 ptrOffsetTable, int storageMode, int version, ArmpTable table)
        {
            reader.Stream.Seek(ptrOffsetTable);

            if (storageMode == 0)
            {
                foreach (ArmpTableColumn column in table.Columns)
                {
                    bool noData = false; //Only happens with boolean column types? Unsure. TODO: verify

                    uint ptrData = reader.ReadUInt32();
                    long nextPtr = reader.Stream.Position;
                    if (ptrData == 0xFFFFFFFF)
                    {
                        column.IsNoData = true;
#if DEBUG
                        Console.WriteLine(String.Format("No data column -- > {0}", column.Name));
#endif
                    }
                    else
                    {
                        reader.Stream.Seek(ptrData);
                    }

                    if (!noData) //TODO this is a placeholder fix
                    {
#if DEBUG
                        Console.WriteLine(column.Name + " ----> " + column.ColumnType);
#endif
                        //Only storage mode 0
                        List<bool> booleanColumnDataTemp = new List<bool>();
                        if (column.ColumnType == DataTypes.Types["boolean"])
                        {
                            booleanColumnDataTemp = Util.IterateBooleanBitmask(reader, (uint)reader.Stream.Position, table.TableInfo.RowCount);
                        }

                        for (int rowIndex = 0; rowIndex < table.TableInfo.RowCount; rowIndex++)
                        {
                            table.GetEntry(rowIndex).ColumnValueOffsets.Add(column.Name, (int)reader.Stream.Position);
                            ReadValue(reader, table, version, rowIndex, column, booleanColumnDataTemp);
                        }

                    }

                    reader.Stream.Seek(nextPtr);
                }
            }


            else if (storageMode == 1)
            {
                for (int rowIndex = 0; rowIndex < table.TableInfo.RowCount; rowIndex++)
                {
                    int ptrData = reader.ReadInt32();
                    long nextPtr = reader.Stream.Position;
                                        
                    foreach (ArmpTableColumn column in table.Columns)
                    {
                        reader.Stream.Seek(ptrData + column.Distance);
                        table.GetEntry(rowIndex).ColumnValueOffsets.Add(column.Name, (int)reader.Stream.Position);
                        ReadValue(reader, table, version, rowIndex, column);
                    }

                    reader.Stream.Seek(nextPtr);
                }
            }
        }



        /// <summary>
        /// Reads a value corresponding to a row and column.
        /// </summary>
        /// <param name="reader">The DataReader.</param>
        /// <param name="table">ArmpTable to save the data to.</param>
        /// <param name="version">armp version.</param>
        /// <param name="columnType">The type of the column to read.</param>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="columnIndex">The column index.</param>
        /// <param name="booleanColumnDataTemp">(Optional) The boolean column data if its using storage mode 0.</param>
        private static void ReadValue (DataReader reader, ArmpTable table, int version, int rowIndex, ArmpTableColumn column, List<bool> booleanColumnDataTemp = null)
        {
            if (column.ColumnType == DataTypes.Types["invalid"])
            {
                table.Entries[rowIndex].Data.Add(column.Name, null);
            }

            else if (column.ColumnType == DataTypes.Types["boolean"])
            {
                if (booleanColumnDataTemp != null)
                {
                    bool value = booleanColumnDataTemp[rowIndex];
                    table.Entries[rowIndex].Data.Add(column.Name, value);
                }
                else
                {
                    byte value = reader.ReadByte();
                    bool boolValue = true;
                    if (value == 0) boolValue = false;
                    table.Entries[rowIndex].Data.Add(column.Name, boolValue);
                }
            }

            else if (column.ColumnType == DataTypes.Types["string"])
            {
                int index = reader.ReadInt32();
                if (index != -1 && table.TableInfo.HasText) //Some files have valid string ids despite not having any text.
                    table.Entries[rowIndex].Data.Add(column.Name, table.Text[index]);
                else
                    table.Entries[rowIndex].Data.Add(column.Name, null);
            }

            else if (column.ColumnType == DataTypes.Types["table"])
            {
                Int64 tablepointer = reader.ReadInt64();
                Int64 currentpos = reader.Stream.Position;
                if (tablepointer == 0 || tablepointer == -1) return;
                ArmpTableMain tableValue = ReadTableMain(reader, tablepointer, version);

                table.Entries[rowIndex].Data.Add(column.Name, tableValue);
                reader.Stream.Position = currentpos; //Reset position to the offset table
            }

            else if (column.IsSpecial)
            {
#if DEBUG
                Console.WriteLine(column.Name + " IS SPECIAL");
#endif
            }

            else
            {
                if (ReadTypeCache.ContainsKey(column.ColumnType))
                {
                    MethodInfo methodref = ReadTypeCache[column.ColumnType];
                    methodref.Invoke(null, new object[] { reader, table, rowIndex, column.ID });
                }
                else
                {
                    MethodInfo methodinfo = typeof(ArmpFileReader).GetMethod("ReadType", BindingFlags.NonPublic | BindingFlags.Static);
                    MethodInfo methodref = methodinfo.MakeGenericMethod(column.ColumnType);
                    ReadTypeCache.Add(column.ColumnType, methodref);
                    methodref.Invoke(null, new object[] { reader, table, rowIndex, column.ID });
                }
            }
        }



        /// <summary>
        /// Reads the column values for each entry. (OLD ENGINE ONLY)
        /// </summary>
        /// <param name="reader">The DataReader.</param>
        /// <param name="ptrOffsetTable">The pointer to the offset table.</param>
        /// <param name="table">The table where the data will be added to.</param>
        private static void ReadEntryData (DataReader reader, UInt32 ptrOffsetTable, ArmpTable table)
        {
            //TODO
            reader.Stream.Seek(ptrOffsetTable);

            foreach (ArmpTableColumn column in table.Columns)
            {
                uint ptrData = reader.ReadUInt32();
                long nextPtr = reader.Stream.Position;
                reader.Stream.Seek(ptrData);

                List<bool> booleanColumnDataTemp = new List<bool>();
                if (column.ColumnType == DataTypes.Types["boolean"])
                {
                    booleanColumnDataTemp = Util.IterateBooleanBitmask(reader, (uint)reader.Stream.Position, table.TableInfo.RowCount);
                }

                for (int rowIndex = 0; rowIndex < table.TableInfo.RowCount; rowIndex++)
                {
                    //TODO make this a function
                    if (column.ColumnType == DataTypes.Types["invalid"])
                    {
                        table.Entries[rowIndex].Data.Add(column.Name, null);
                    }

                    else if (column.ColumnType == DataTypes.Types["boolean"])
                    {
                        bool value = booleanColumnDataTemp[rowIndex];
                        table.Entries[rowIndex].Data.Add(column.Name, value);
                    }

                    else
                    {
                        if (ReadTypeCache.ContainsKey(column.ColumnType))
                        {
                            MethodInfo methodref = ReadTypeCache[column.ColumnType];
                            methodref.Invoke(null, new object[] { reader, table, rowIndex, column.ID });
                        }
                        else
                        {
                            MethodInfo methodinfo = typeof(ArmpFileReader).GetMethod("ReadType", BindingFlags.NonPublic | BindingFlags.Static);
                            MethodInfo methodref = methodinfo.MakeGenericMethod(column.ColumnType);
                            ReadTypeCache.Add(column.ColumnType, methodref);
                            methodref.Invoke(null, new object[] { reader, table, rowIndex, column.ID });
                        }
                    }
                }

                reader.Stream.Seek(nextPtr);
            }
        
        }



        /// <summary>
        /// Sets the "Valid" flag for a list of entries.
        /// </summary>
        /// <param name="rowValidity">The validity list.</param>
        /// <param name="entries">The entry list to update.</param>
        private static void SetRowValidity (List<bool> rowValidity, List<ArmpEntry> entries)
        {
            int iter = 0;
            foreach(bool validity in rowValidity)
            {
                entries[iter].IsValid = validity;
                iter++;
            }
        }



        /// <summary>
        /// Reads the Entry Info Flags. (v1 only)
        /// </summary>
        /// <param name="reader">The DataReader.</param>
        /// <param name="ptrArray">The pointer to the array.</param>
        /// <param name="entries">The entry list.</param>
        /// <returns>A list.</returns>
        private static void IterateEntryInfoFlags (DataReader reader, UInt32 ptrArray, List<ArmpEntry> entries)
        {
            reader.Stream.Seek(ptrArray);

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
        /// Reads the additional Column Metadata. (v2 only)
        /// </summary>
        /// <param name="reader">The DataReader.</param>
        /// <param name="ptrEntryInfo">The pointer to the Entry Info section.</param>
        /// <param name="entries">The entry list.</param>
        private static void ReadColumnUnknownMetadata0x4C (DataReader reader, UInt32 ptrEntryInfo, ArmpTable table)
        {
            reader.Stream.Seek(ptrEntryInfo);

            for (int i = 0; i<table.Columns.Count; i++)
            {
                int size = reader.ReadInt32();
                int ptrMetadata = reader.ReadInt32();
                reader.ReadBytes(0x18); //Padding

                if (size > 0)
                {
                    ArmpTableColumn column = table.Columns[i];
                    reader.Stream.PushToPosition(ptrMetadata);

                    foreach (ArmpTableColumn child in column.Children)
                    {
                        child.UnknownMetadata0x4C = reader.ReadInt32();
                    }

                    reader.Stream.PopPosition();
                }
            }
        }



        /// <summary>
        /// Reads the Column Data Types auxiliary table.
        /// </summary>
        /// <param name="reader">The DataReader.</param>
        /// <param name="ptrTable">The pointer to the auxiliary table.</param>
        /// <param name="columnAmount">The amount of columns in the table.</param>
        private static List<List<int>> GetColumnDataTypesAuxTable (DataReader reader, UInt32 ptrTable, int columnAmount)
        {
            reader.Stream.Seek(ptrTable);
            List<List<int>> ColumnDataTypesAuxTable = new List<List<int>>();

            for (int i = 0; i < columnAmount; i++)
            {
                List<int> list = new List<int>();
                list.Add(reader.ReadInt32()); //Type
                list.Add(reader.ReadInt32()); //Distance
                list.Add(reader.ReadInt32()); //Array Size
                list.Add(reader.ReadInt32()); //Unknown
                ColumnDataTypesAuxTable.Add(list);
            }

            return ColumnDataTypesAuxTable;
        }



        /// <summary>
        /// Generates an auxiliary type list from the IDs in the aux table. (v2 only)
        /// </summary>
        /// <param name="columnDataTypesAuxTable">The Column Data Types Auxiliary Table.</param>
        /// <returns>A type list.</returns>
        private static List<Type> ColumnDataTypesAuxTableToColumnDataTypesAux (List<List<int>> columnDataTypesAuxTable)
        {
            List<Type> typesList = new List<Type>();

            for (int i = 0; i < columnDataTypesAuxTable.Count; i++)
            {
                Type type = DataTypes.TypesV2Aux[Convert.ToSByte(columnDataTypesAuxTable[i][0])];
                typesList.Add(type);
            }

            return typesList;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnDataTypes"></param>
        /// <returns></returns>
        private static List<bool> GetSpecialColumnList (List<Type> columnDataTypes)
        {
            List<bool> returnList = new List<bool>();

            foreach(Type columnType in columnDataTypes)
            {
                if (DataTypes.TypesV2Specials.Contains(columnType)) returnList.Add(true);
                else returnList.Add(false);
            }

            return returnList;
        }

    }
}

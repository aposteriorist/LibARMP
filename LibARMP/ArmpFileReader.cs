using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace LibARMP
{
    public static class ArmpFileReader
    {
        /// <summary>
        /// Reads an armp file.
        /// </summary>
        /// <param name="path">The path to the armp file.</param>
        /// <returns>An ARMP object.</returns>
        public static ARMP ReadARMP (string path)
        {
            using (var stream = DataStreamFactory.FromFile(path, FileOpenMode.Read))
            {
                var reader = new DataReader(stream)
                {
                    Endianness = EndiannessMode.LittleEndian,
                };

                ARMP armp = new ARMP();
                
                ArmpTable subTable = new ArmpTable();

                char[] magic = reader.ReadChars(4);
                int endianess = reader.ReadInt32(); //Only used in OE
                if (endianess == 258)
                {
                    reader.Endianness = EndiannessMode.BigEndian;
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

                //DEBUG
                Console.WriteLine("Version: " + armp.Version);
                Console.WriteLine("Revision: " + armp.Revision);

                int fileSize = reader.ReadInt32(); //Only used in OE

                if (armp.IsOldEngine)
                {
                    armp.MainTable = ReadTable(reader);
                }
                else
                {
                    uint ptrMainTable = reader.ReadUInt32();
                    armp.MainTable = ReadTable(reader, ptrMainTable, armp.Version);
                    if (armp.MainTable.TableInfo.HasSubTable) armp.SubTable = ReadTable(reader, armp.MainTable.TableInfo.ptrSubTable, armp.Version);
                }

                return armp;
            }
        }


        /// <summary>
        /// Reads a Dragon Engine armp table.
        /// </summary>
        /// <param name="reader">The path to the armp file.</param>
        /// <param name="ptrMainTable">The path to the armp file.</param>
        /// <returns>An ArmpTable object.</returns>
        private static ArmpTable ReadTable (DataReader reader, long ptrMainTable, int version)
        {
            ArmpTable table = new ArmpTable();

            reader.Stream.Seek(ptrMainTable);
            table.TableInfo = GetARMPTableInfo(reader, false);

            //Read general data
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

            //Column names
            if (table.TableInfo.HasColumnNames)
            {
                table.ColumnNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrColumnNamesOffsetTable, table.TableInfo.ColumnCount));
            }
            else
            {
                //Fill with numbers
                table.ColumnNames = new List<string>();
                for (int c = 0; c < table.TableInfo.ColumnCount; c++)
                {
                    table.ColumnNames.Add(c.ToString());
                }
            }

            //Text
            if (table.TableInfo.HasText) table.Text = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrTextOffsetTable, table.TableInfo.TextCount));
            //Column Data Types
            table.ColumnDataTypes = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypes, table.TableInfo.ColumnCount, version, false);
            //Column Data Types Aux
            if (table.TableInfo.HasColumnDataTypesAux)
            {
                if (table.TableInfo.StorageMode == 1)
                {
                    table.ColumnDataTypesAuxTable = GetColumnDataTypesAuxTable(reader, table.TableInfo.ptrColumnDataTypesAux, table.TableInfo.ColumnCount);
                    table.ColumnDataTypesAux = ColumnDataTypesAuxTableToColumnDataTypesAux(table.ColumnDataTypesAuxTable);
                }
                else
                {
                    table.ColumnDataTypesAux = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypesAux, table.TableInfo.ColumnCount, version, true);
                }
            }

            // Special Column list (arrays/lists). Only v2
            if (version == 2)
            {
                table.SpecialColumns = GetSpecialColumnList(table.ColumnDataTypes);
            }

            //Column Validity
            if (table.TableInfo.HasColumnValidity) table.ColumnValidity = Util.IterateBooleanBitmask(reader, table.TableInfo.ptrColumnValidity, table.TableInfo.ColumnCount);
            //Row Indices
            if (table.TableInfo.HasRowIndices) table.RowIndices = Util.IterateArray<int>(reader, table.TableInfo.ptrRowIndices, table.TableInfo.RowCount);
            //Column Indices
            if (table.TableInfo.HasColumnIndices) table.ColumnIndices = Util.IterateArray<int>(reader, table.TableInfo.ptrColumnIndices, table.TableInfo.ColumnCount);

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
                    IterateEntryFlags(reader, table.TableInfo.ptrExtraFieldInfo, table.Entries);
                }
                if (version == 2)
                {
                    //TODO
                }
            }

            return table;
        }



        /// <summary>
        /// Reads an Old Engine armp table.
        /// </summary>
        /// <param name="reader">The path to the armp file.</param>
        /// <returns>An ArmpTable object.</returns>
        private static ArmpTable ReadTable(DataReader reader)
        {
            //TODO column metadata, text
            ArmpTable table = new ArmpTable();

            table.TableInfo = GetARMPTableInfo(reader, true);

            //Read general data
            //Row names
            if (table.TableInfo.HasRowNames)
            {
                table.RowNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrRowNamesOffsetTable, table.TableInfo.RowCount));
            }

            //Column names
            if (table.TableInfo.HasColumnNames)
            {
                table.ColumnNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrColumnNamesOffsetTable, table.TableInfo.ColumnCount));
            }

            if (table.TableInfo.HasText) table.Text = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrTextOffsetTable, table.TableInfo.TextCount));

            //Column Data Types
            table.ColumnDataTypes = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypes, table.TableInfo.ColumnCount);

            InitializeEntries(table);
            ReadEntryData(reader, table.TableInfo.ptrColumnContentOffsetTable, table);

            //Row Validity
            if (table.TableInfo.HasRowValidity)
            {
                table.RowValidity = Util.IterateBooleanBitmask(reader, table.TableInfo.ptrRowValidity, table.TableInfo.RowCount);
                SetRowValidity(table.RowValidity, table.Entries);
            }

            //Column Metadata
            if (table.TableInfo.HasColumnMetadata)
            {
                table.ColumnMetadata = Util.IterateArray<int>(reader, table.TableInfo.ptrColumnMetadata, table.TableInfo.ColumnCount);
            }

            return table;
        }



        /// <summary>
        /// Reads the table data and returns an ArmpTableInfo object.
        /// </summary>
        /// <param name="reader">The DataStream reader</param>
        private static ArmpTableInfo GetARMPTableInfo (DataReader reader, bool IsOldEngine)
        {
            ArmpTableInfo armpTableInfo = new ArmpTableInfo();

            if (IsOldEngine)
            {
                //TODO check for Ishin version
                armpTableInfo.RowCount = reader.ReadInt32();
                reader.ReadBytes(0x4);
                armpTableInfo.ptrRowNamesOffsetTable = reader.ReadUInt32();
                armpTableInfo.ptrRowValidity = reader.ReadUInt32();
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
                if (armpTableInfo.ptrRowNamesOffsetTable > 0) armpTableInfo.HasRowNames = true;
                if (armpTableInfo.ptrColumnNamesOffsetTable > 0) armpTableInfo.HasColumnNames = true;
                if (armpTableInfo.ptrRowValidity > 0) armpTableInfo.HasRowValidity = true;
                if (armpTableInfo.ptrColumnMetadata > 0) armpTableInfo.HasColumnMetadata = true;

                //DEBUG
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
            }
            else
            {
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
                armpTableInfo.ptrFieldID = reader.ReadUInt32(); //TODO verify (this seems to go unused)
                armpTableInfo.ptrEmptyValuesOffsetTable = reader.ReadUInt32();
                armpTableInfo.ptrColumnDataTypesAux = reader.ReadUInt32();
                armpTableInfo.ptrExtraFieldInfo = reader.ReadUInt32();


                //Set flags
                if (armpTableInfo.TextCount > 0) armpTableInfo.HasText = true;
                if (armpTableInfo.ptrSubTable > 0) armpTableInfo.HasSubTable = true;
                if (armpTableInfo.ptrRowNamesOffsetTable > 0) armpTableInfo.HasRowNames = true;
                if (armpTableInfo.ptrColumnNamesOffsetTable > 0) armpTableInfo.HasColumnNames = true;
                if (armpTableInfo.ptrColumnDataTypesAux > 0) armpTableInfo.HasColumnDataTypesAux = true;
                if (armpTableInfo.ptrRowValidity > 0) armpTableInfo.HasRowValidity = true;
                if (armpTableInfo.ptrColumnValidity > 0) armpTableInfo.HasColumnValidity = true;
                if (armpTableInfo.ptrRowIndices > 0) armpTableInfo.HasRowIndices = true;
                if (armpTableInfo.ptrColumnIndices > 0) armpTableInfo.HasColumnIndices = true;
                if (armpTableInfo.ptrExtraFieldInfo > 0) armpTableInfo.HasExtraFieldInfo = true;


                //DEBUG
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
                Console.WriteLine("Pointer to Field IDs: " + armpTableInfo.ptrFieldID);
                Console.WriteLine("Pointer to Empty Values Offset Table: " + armpTableInfo.ptrEmptyValuesOffsetTable);
                Console.WriteLine("Pointer to Column Data Types Aux: " + armpTableInfo.ptrColumnDataTypesAux);
                Console.WriteLine("Pointer to Field Info: " + armpTableInfo.ptrExtraFieldInfo);
                Console.WriteLine("Has SubTable: " + armpTableInfo.HasSubTable);
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
        private static List<Type> GetColumnDataTypes(DataReader reader, UInt32 ptrDataTypes, int amount)
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
                for (int columnIndex = 0; columnIndex < table.TableInfo.ColumnCount; columnIndex++)
                {
                    uint ptrData = reader.ReadUInt32();
                    long nextPtr = reader.Stream.Position;
                    reader.Stream.Seek(ptrData);

                    //TODO change data type table for v2 storagemode 0
                    Type columnType;
                    if (version == 1)
                    {
                        columnType = table.ColumnDataTypesAux[columnIndex];
                    }
                    else //v2
                    {
                        columnType = table.ColumnDataTypes[columnIndex];
                    }
                        

                    //Only storage mode 0
                    List<bool> booleanColumnDataTemp = new List<bool>();
                    if (columnType == DataTypes.Types["boolean"])
                    {
                        booleanColumnDataTemp = Util.IterateBooleanBitmask(reader, (uint)reader.Stream.Position, table.TableInfo.RowCount);
                    }

                    for (int rowIndex = 0; rowIndex < table.TableInfo.RowCount; rowIndex++)
                    {
                        //Can't do a switch for this because type patterns are in preview or whatever ¯\_(ツ)_/¯
                        if (columnType == DataTypes.Types["invalid"])
                        {
                            table.Entries[rowIndex].Data.Add( table.ColumnNames[columnIndex], null );
                        }

                        else if (columnType == DataTypes.Types["string"])
                        {
                            int index = reader.ReadInt32();
                            if (index != -1)
                                table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], table.Text[index]);
                            else
                                table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], null);
                        }

                        else if(columnType == DataTypes.Types["uint8"])
                        {
                            byte value = reader.ReadByte();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["uint16"])
                        {
                            UInt16 value = reader.ReadUInt16();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["uint32"])
                        {
                            UInt32 value = reader.ReadUInt32();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["uint64"])
                        {
                            UInt64 value = reader.ReadUInt64();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["int8"])
                        {
                            sbyte value = reader.ReadSByte();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["int16"])
                        {
                            Int16 value = reader.ReadInt16();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["int32"])
                        {
                            Int32 value = reader.ReadInt32();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["int64"])
                        {
                            Int64 value = reader.ReadInt64();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["float32"])
                        {
                            float value = reader.ReadSingle();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["float64"])
                        {
                            double value = reader.ReadDouble();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["boolean"])
                        {
                            bool value = booleanColumnDataTemp[rowIndex];
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["string"])
                        {
                            Int32 strpointer = reader.ReadInt32();
                            reader.Stream.Position = strpointer;
                            string value = reader.ReadString();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["table"])
                        {
                            Int64 tablepointer = reader.ReadInt64();
                            Int64 currentpos = reader.Stream.Position;
                            if (tablepointer == 0 || tablepointer == -1) continue;
                            ArmpTable tbl = ReadTable(reader, tablepointer, version);
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], tbl);
                            reader.Stream.Position = currentpos; //Reset position to the offset table
                        }

                        else
                        {
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], null);
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
                    //reader.Stream.Seek(ptrData);
                                        
                    for (int columnIndex = 0; columnIndex < table.TableInfo.ColumnCount; columnIndex++)
                    {
                        //TODO
                        Type columnType = table.ColumnDataTypes[columnIndex];
                        reader.Stream.Seek(ptrData + table.ColumnDataTypesAuxTable[columnIndex][1]);

                        if (columnType == DataTypes.Types["invalid"])
                        {
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], null);
                        }

                        else if (columnType == DataTypes.Types["uint8"])
                        {
                            byte value = reader.ReadByte();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["uint16"])
                        {
                            UInt16 value = reader.ReadUInt16();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["uint32"])
                        {
                            UInt32 value = reader.ReadUInt32();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["uint64"])
                        {
                            UInt64 value = reader.ReadUInt64();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["int8"])
                        {
                            sbyte value = reader.ReadSByte();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["int16"])
                        {
                            Int16 value = reader.ReadInt16();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["int32"])
                        {
                            Int32 value = reader.ReadInt32();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["int64"])
                        {
                            Int64 value = reader.ReadInt64();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["float32"])
                        {
                            float value = reader.ReadSingle();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["float64"])
                        {
                            float value = reader.ReadSingle();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["boolean"])
                        {
                            byte value = reader.ReadByte();
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                        }

                        else if (columnType == DataTypes.Types["string"])
                        {
                            int index = reader.ReadInt32();
                            if (index != -1)
                                table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], table.Text[index]);
                            else
                                table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], null);
                        }

                        else if (columnType == DataTypes.Types["table"])
                        {
                            Int64 tablepointer = reader.ReadInt64();
                            Int64 currentpos = reader.Stream.Position;
                            if (tablepointer == 0 || tablepointer == -1) continue;
                            ArmpTable tbl = ReadTable(reader, tablepointer, version);
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], tbl);
                            reader.Stream.Position = currentpos; //Reset position to the offset table
                        }

                        else
                        {
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], null);
                        }
                    }

                    reader.Stream.Seek(nextPtr);
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

            for (int columnIndex = 0; columnIndex < table.TableInfo.ColumnCount; columnIndex++)
            {
                uint ptrData = reader.ReadUInt32();
                long nextPtr = reader.Stream.Position;
                reader.Stream.Seek(ptrData);

                Type columnType = table.ColumnDataTypes[columnIndex];

                List<bool> booleanColumnDataTemp = new List<bool>();
                if (columnType == DataTypes.Types["boolean"])
                {
                    booleanColumnDataTemp = Util.IterateBooleanBitmask(reader, (uint)reader.Stream.Position, table.TableInfo.RowCount);
                }

                for (int rowIndex = 0; rowIndex < table.TableInfo.RowCount; rowIndex++)
                {
                    //TODO make this a function
                    if (columnType == DataTypes.Types["invalid"])
                    {
                        table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], null);
                    }

                    else if (columnType == DataTypes.Types["uint8"])
                    {
                        byte value = reader.ReadByte();
                        table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                    }

                    else if (columnType == DataTypes.Types["uint16"])
                    {
                        UInt16 value = reader.ReadUInt16();
                        table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                    }

                    else if (columnType == DataTypes.Types["uint32"])
                    {
                        UInt32 value = reader.ReadUInt32();
                        table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                    }

                    else if (columnType == DataTypes.Types["int8"])
                    {
                        sbyte value = reader.ReadSByte();
                        table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                    }

                    else if (columnType == DataTypes.Types["int16"])
                    {
                        Int16 value = reader.ReadInt16();
                        table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                    }

                    else if (columnType == DataTypes.Types["int32"])
                    {
                        Int32 value = reader.ReadInt32();
                        table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                    }

                    else if (columnType == DataTypes.Types["boolean"])
                    {
                        bool value = booleanColumnDataTemp[rowIndex];
                        table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], value);
                    }

                    else
                    {
                        table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], null);
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
        /// Reads the flags of every entry.
        /// </summary>
        /// <param name="reader">The DataReader.</param>
        /// <param name="ptrArray">The pointer to the array.</param>
        /// <param name="entries">The entry list.</param>
        /// <returns>A list.</returns>
        private static void IterateEntryFlags (DataReader reader, UInt32 ptrArray, List<ArmpEntry> entries)
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
        private static List<Type> ColumnDataTypesAuxTableToColumnDataTypesAux(List<List<int>> columnDataTypesAuxTable)
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
        private static List<bool> GetSpecialColumnList(List<Type> columnDataTypes)
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

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

                armp.Revision = reader.ReadInt16();
                armp.Version = reader.ReadInt16();

                int fileSize = reader.ReadInt32(); //Only used in OE
                int ptrMainTable = reader.ReadInt32();

                //DEBUG
                Console.WriteLine("Version: " + armp.Version);
                Console.WriteLine("Revision: " + armp.Revision);

                armp.MainTable = ReadTable(reader, ptrMainTable, armp.Version);
                //if (armp.MainTable.TableInfo.HasSubTable) armp.SubTable = ReadTable(reader, armp.MainTable.TableInfo.ptrSubTable);

                return armp;
            }
        }


        /// <summary>
        /// Reads an armp table.
        /// </summary>
        /// <param name="reader">The path to the armp file.</param>
        /// <param name="ptrMainTable">The path to the armp file.</param>
        /// <returns>An ArmpTable object.</returns>
        private static ArmpTable ReadTable (DataReader reader, int ptrMainTable, int version)
        {
            ArmpTable table = new ArmpTable();

            reader.Stream.Seek(ptrMainTable);
            table.TableInfo = GetARMPTableInfo(reader);

            //Read general data
            table.RowNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrRowNamesOffsetTable, table.TableInfo.RowCount)); //Row names
            table.ColumnNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrColumnNamesOffsetTable, table.TableInfo.ColumnCount)); //Column names
            if (table.TableInfo.TextCount > 0) table.Text = Util.IterateStringList(reader, Util.IterateOffsetList(reader, table.TableInfo.ptrTextOffsetTable, table.TableInfo.TextCount)); //Text
            table.ColumnDataTypes = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypes, table.TableInfo.ColumnCount, version, false); //Column Data Types
            table.ColumnDataTypesAux = GetColumnDataTypes(reader, table.TableInfo.ptrColumnDataTypesAux, table.TableInfo.ColumnCount, version, true); //Column Data Types Aux
            if (table.TableInfo.ptrRowValidity > 0) table.RowValidity = Util.IterateBooleanBitmask(reader, table.TableInfo.ptrRowValidity, table.TableInfo.RowCount); //Row Validity
            if (table.TableInfo.ptrColumnValidity > 0) table.ColumnValidity = Util.IterateBooleanBitmask(reader, table.TableInfo.ptrColumnValidity, table.TableInfo.ColumnCount); //Column Validity
            if (table.TableInfo.ptrRowIndices > 0) table.RowIndices = Util.IterateArray<int>(reader, table.TableInfo.ptrRowIndices, table.TableInfo.RowCount); //Row Indices
            if (table.TableInfo.ptrColumnIndices > 0) table.ColumnIndices = Util.IterateArray<int>(reader, table.TableInfo.ptrColumnIndices, table.TableInfo.ColumnCount); //Column Indices

            InitializeEntries(table);
            ReadEntryData(reader, table.TableInfo.ptrColumnContentOffsetTable, table.TableInfo.StorageMode, table);

            return table;
        }


        /// <summary>
        /// Reads the table data and returns an ArmpTableInfo object.
        /// </summary>
        /// <param name="reader">The DataStream reader</param>
        private static ArmpTableInfo GetARMPTableInfo (DataReader reader) //TODO
        {
            //TODO OE version
            ArmpTableInfo armpTableInfo = new ArmpTableInfo();

            armpTableInfo.RowCount = reader.ReadInt32();
            armpTableInfo.ColumnCount = reader.ReadInt32();
            armpTableInfo.TextCount = reader.ReadInt32();
            armpTableInfo.RowValidator = reader.ReadInt32();
            armpTableInfo.ptrRowNamesOffsetTable = reader.ReadInt32();
            armpTableInfo.ptrRowValidity = reader.ReadInt32();
            armpTableInfo.ptrColumnDataTypes = reader.ReadInt32();
            armpTableInfo.ptrColumnContentOffsetTable = reader.ReadInt32();
            armpTableInfo.TableID = reader.ReadInt24();
            armpTableInfo.StorageMode = reader.ReadByte();
            armpTableInfo.ptrTextOffsetTable = reader.ReadInt32();
            armpTableInfo.ptrColumnNamesOffsetTable = reader.ReadInt32();
            armpTableInfo.ColumnValidator = reader.ReadInt32();
            armpTableInfo.ptrRowIndices = reader.ReadInt32();
            armpTableInfo.ptrColumnIndices = reader.ReadInt32();
            armpTableInfo.ptrColumnValidity = reader.ReadInt32();
            armpTableInfo.ptrSubTable = reader.ReadInt32();
            armpTableInfo.ptrFieldID = reader.ReadInt32(); //TODO verify
            armpTableInfo.ptrEmptyValuesOffsetTable = reader.ReadInt32();
            armpTableInfo.ptrColumnDataTypesAux = reader.ReadInt32();
            armpTableInfo.ptrFieldInfo = reader.ReadInt32();

            if (armpTableInfo.ptrSubTable != 0 && armpTableInfo.ptrSubTable != -1)
            {
                armpTableInfo.HasSubTable = true;
            }

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
            Console.WriteLine("Pointer to Field Info: " + armpTableInfo.ptrFieldInfo);
            Console.WriteLine("Has SubTable: " + armpTableInfo.HasSubTable);

            return armpTableInfo;
        }


        /// <summary>
        /// Reads a byte array (Validity Bool).
        /// </summary>
        /// <param name="reader">The DataStream Reader.</param>
        /// <param name="ptrArray">The pointer to the array.</param>
        /// <param name="amount">The amount of values in the array.</param>
        /// <returns>A list.</returns>
        private static List<string> IterateValidityBool(DataReader reader, int ptrArray, int amount)
        {
            List<string> returnList = new List<string>();
            reader.Stream.Seek(ptrArray);

            for (int i = 0; i < amount; i++)
            {
                byte b = reader.ReadByte();
                string bitstring = Convert.ToString(b, 2);
                returnList.Add(bitstring);
            }

            return returnList;
        }


        /// <summary>
        /// Reads the column data types.
        /// </summary>
        /// <param name="reader">The DataStream Reader.</param>
        /// <param name="ptrDataTypes">The pointer to the column data type array.</param>
        /// <param name="amount">The amount of values in the array.</param>
        /// <param name="version">The armp version.</param>
        /// <param name="isAuxiliary">Is it the auxiliary data types array?</param>
        /// <returns>A types list.</returns>
        private static List<Type> GetColumnDataTypes(DataReader reader, int ptrDataTypes, int amount, int version, bool isAuxiliary)
        {
            List<Type> returnList = new List<Type>();
            IDictionary<sbyte, Type> typesDictionary = new Dictionary<sbyte, Type>();

            reader.Stream.Seek(ptrDataTypes);

            if (version == 1 && !isAuxiliary) typesDictionary = DataTypes.TypesV1;
            else if (version == 1 && isAuxiliary) typesDictionary = DataTypes.TypesV1Aux;
            //TODO add v2

            for (int i = 0; i < amount; i++)
            {
                sbyte id = reader.ReadSByte();
                returnList.Add(typesDictionary[id]);
            }

            return returnList;
        }


        /// <summary>
        /// Initializes the entries of a table.
        /// </summary>
        /// <param name="table">The ArmpTable.</param>
        private static void InitializeEntries(ArmpTable table)
        {
            for (int i=0; i<table.TableInfo.RowCount; i++)
            {
                ArmpEntry entry = new ArmpEntry(i, table.RowNames[i], table.RowIndices[i]);
                table.Entries.Add(entry);
            }
        }



        private static void ReadEntryData (DataReader reader, int ptrOffsetTable, int storageMode, ArmpTable table)
        {
            reader.Stream.Seek(ptrOffsetTable);

            if (storageMode == 0)
            {
                for (int columnIndex = 0; columnIndex < table.TableInfo.ColumnCount; columnIndex++)
                {
                    int ptrData = reader.ReadInt32();
                    long nextPtr = reader.Stream.Position;
                    reader.Stream.Seek(ptrData);

                    //TODO change data type table for v2 storagemode 0
                    Type columnType = table.ColumnDataTypesAux[columnIndex];
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

                        else
                        {
                            table.Entries[rowIndex].Data.Add(table.ColumnNames[columnIndex], null);
                        }

                    }

                    reader.Stream.Seek(nextPtr);
                }
            }
        }
        

    }
}

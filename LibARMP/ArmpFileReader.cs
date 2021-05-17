using System;
using Yarhl.IO;

namespace LibARMP
{
    public static class ArmpFileReader
    {
        /// <summary>
        /// Reads an armp file and returns and ARMP object
        /// </summary>
        /// <param name="path">The path to the armp file.</param>
        public static ARMP ReadARMP (string path)
        {
            using (var stream = DataStreamFactory.FromFile(path, FileOpenMode.Read))
            {
                var reader = new DataReader(stream)
                {
                    Endianness = EndiannessMode.LittleEndian,
                };

                ARMP armp = new ARMP();

                char[] magic = reader.ReadChars(4);
                int endianess = reader.ReadInt32(); //Only used in OE

                armp.revision = reader.ReadInt16();
                armp.version = reader.ReadInt16();

                int fileSize = reader.ReadInt32(); //Only used in OE
                int ptrMainTable = reader.ReadInt32();

                //DEBUG
                Console.WriteLine("Version: " + armp.version);
                Console.WriteLine("Revision: " + armp.revision);

                reader.Stream.Seek(ptrMainTable);
                ArmpTableInfo TableInfo = GetARMPTableInfo(reader);

                //Read entries
                armp.rowNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, TableInfo.ptrRowNamesOffsetTable, TableInfo.rowCount)); //Row names
                armp.columnNames = Util.IterateStringList(reader, Util.IterateOffsetList(reader, TableInfo.ptrColumnNamesOffsetTable, TableInfo.columnCount)); //Column names
                if (TableInfo.ptrRowValidity != 0) armp.rowValidity = Util.IterateBooleanBitmask(reader, TableInfo.ptrRowValidity, TableInfo.rowCount); //Row Validity
                if (TableInfo.ptrColumnValidity != 0) armp.columnValidity = Util.IterateBooleanBitmask(reader, TableInfo.ptrColumnValidity, TableInfo.columnCount); //Column Validity

                return armp;
            }
        }


        /// <summary>
        /// Reads the table data and returns an ArmpTableInfo object.
        /// </summary>
        /// <param name="reader">The DataStream reader</param>
        private static ArmpTableInfo GetARMPTableInfo (DataReader reader) //TODO
        {
            //TODO OE version
            ArmpTableInfo armpTableInfo = new ArmpTableInfo();

            armpTableInfo.rowCount = reader.ReadInt32();
            armpTableInfo.columnCount = reader.ReadInt32();
            armpTableInfo.textCount = reader.ReadInt32();
            armpTableInfo.rowValidator = reader.ReadInt32();
            armpTableInfo.ptrRowNamesOffsetTable = reader.ReadInt32();
            armpTableInfo.ptrRowValidity = reader.ReadInt32();
            armpTableInfo.ptrColumnDataTypes = reader.ReadInt32();
            armpTableInfo.ptrColumnContentOffsetTable = reader.ReadInt32();
            armpTableInfo.tableID = reader.ReadInt24();
            armpTableInfo.storageMode = reader.ReadByte();
            armpTableInfo.ptrTextOffsetTable = reader.ReadInt32();
            armpTableInfo.ptrColumnNamesOffsetTable = reader.ReadInt32();
            armpTableInfo.columnValidator = reader.ReadInt32();
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
                armpTableInfo.hasSubTable = true;
            }

            //DEBUG
            Console.WriteLine("Row Count: " + armpTableInfo.rowCount);
            Console.WriteLine("Column Count: " + armpTableInfo.columnCount);
            Console.WriteLine("Text Count: " + armpTableInfo.textCount);
            Console.WriteLine("Row Validator: " + armpTableInfo.rowValidator);
            Console.WriteLine("Pointer to Row Names Offset Table: " + armpTableInfo.ptrRowNamesOffsetTable);
            Console.WriteLine("Pointer to Row Validity: " + armpTableInfo.ptrRowValidity);
            Console.WriteLine("Pointer to Column Data Types: " + armpTableInfo.ptrColumnDataTypes);
            Console.WriteLine("Pointer to Column Content Offset Table: " + armpTableInfo.ptrColumnContentOffsetTable);
            Console.WriteLine("Table ID: " + armpTableInfo.tableID);
            Console.WriteLine("Storage Mode: " + armpTableInfo.storageMode);
            Console.WriteLine("Pointer to Text Offset Table: " + armpTableInfo.ptrTextOffsetTable);
            Console.WriteLine("Pointer to Column Names Offset Table: " + armpTableInfo.ptrColumnNamesOffsetTable);
            Console.WriteLine("Column Validator: " + armpTableInfo.columnValidator);
            Console.WriteLine("Pointer to Row Indices: " + armpTableInfo.ptrRowIndices);
            Console.WriteLine("Pointer to Column Indices: " + armpTableInfo.ptrColumnIndices);
            Console.WriteLine("Pointer to Column Validity: " + armpTableInfo.ptrColumnValidity);
            Console.WriteLine("Pointer to SubTable: " + armpTableInfo.ptrSubTable);
            Console.WriteLine("Pointer to Field IDs: " + armpTableInfo.ptrFieldID);
            Console.WriteLine("Pointer to Empty Values Offset Table: " + armpTableInfo.ptrEmptyValuesOffsetTable);
            Console.WriteLine("Pointer to Column Data Types Aux: " + armpTableInfo.ptrColumnDataTypesAux);
            Console.WriteLine("Pointer to Field Info: " + armpTableInfo.ptrFieldInfo);
            Console.WriteLine("Has SubTable: " + armpTableInfo.hasSubTable);

            return armpTableInfo;
        }




    }
}

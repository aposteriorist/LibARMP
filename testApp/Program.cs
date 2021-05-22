using System;
using LibARMP;

namespace testApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = @"Z:\\db2_test.bin";
            string path = @"Z:\\auth_costume_change.bin";


            ARMP armp = ArmpFileReader.ReadARMP(path);

            Console.WriteLine("\n--- ROW NAMES ---");
            foreach (string row in armp.MainTable.RowNames)
            {
                Console.WriteLine(row);
            }

            Console.WriteLine("\n--- COLUMN NAMES ---");
            foreach (string column in armp.MainTable.ColumnNames)
            {
                Console.WriteLine(column);
            }

            Console.WriteLine("\n--- ROW INDICES ---");
            foreach (int index in armp.MainTable.RowIndices)
            {
                Console.WriteLine(index);
            }

            Console.WriteLine("\n--- COLUMN INDICES ---");
            foreach (int index in armp.MainTable.ColumnIndices)
            {
                Console.WriteLine(index);
            }

            Console.WriteLine("\n--- COLUMN DATA TYPES ---");
            foreach (Type datatype in armp.MainTable.ColumnDataTypes)
            {
                Console.WriteLine(datatype);
            }

            Console.WriteLine("\n--- COLUMN DATA TYPES AUX ---");
            foreach (Type datatype in armp.MainTable.ColumnDataTypesAux)
            {
                Console.WriteLine(datatype);
            }
        }
    }
}

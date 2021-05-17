using System;
using LibARMP;

namespace testApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"Z:\\db2_test.bin";


            ARMP armp = ArmpFileReader.ReadARMP(path);

            Console.WriteLine("\n--- ROW NAMES ---");
            foreach (string row in armp.rowNames)
            {
                Console.WriteLine(row);
            }

            Console.WriteLine("\n--- COLUMN NAMES ---");
            foreach (string column in armp.columnNames)
            {
                Console.WriteLine(column);
            }
        }
    }
}

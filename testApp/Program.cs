using System;
using System.Collections.Generic;
using LibARMP;

namespace testApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = @"Z:\\db2_test.bin";
            string path = @"Z:\\character_character_data.bin";
            //string path = @"Z:\\auth_costume_change.bin";

            var watch = System.Diagnostics.Stopwatch.StartNew();
            ARMP armp = ArmpFileReader.ReadARMP(path);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("\nARMP LOAD TIME: "+ elapsedMs+"ms");


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

            if (armp.MainTable.Text != null)
            {
                Console.WriteLine("\n--- TEXT ---");
                foreach (string text in armp.MainTable.Text)
                {
                    Console.WriteLine(text);
                }
            }

            if (armp.MainTable.RowIndices != null)
            {
                Console.WriteLine("\n--- ROW INDICES ---");
                foreach (int index in armp.MainTable.RowIndices)
                {
                    Console.WriteLine(index);
                }
            }

            if (armp.MainTable.ColumnIndices != null)
            {
                Console.WriteLine("\n--- COLUMN INDICES ---");
                foreach (int index in armp.MainTable.ColumnIndices)
                {
                    Console.WriteLine(index);
                }
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

            Console.WriteLine("\n--- ENTRY 1 INFO ---");
            Console.WriteLine("ID: " + armp.MainTable.Entries[1].ID);
            Console.WriteLine("Index: " + armp.MainTable.Entries[1].Index);
            Console.WriteLine("Name: " + armp.MainTable.Entries[1].Name);
            List<ArmpEntry> test = armp.MainTable.SearchByValue("face_target", "kiryu");
            Console.WriteLine("TEST ENTRIES");
            foreach (ArmpEntry entry in test)
            {
                Console.WriteLine(entry.ID);
            }

            Console.WriteLine("BOOLEAN TEST ENTRY 1: " + armp.MainTable.Entries[2179].Data["main_chara"]);
            ArmpEntry temp = armp.MainTable.GenerateTemplateArmpEntry();
            Console.WriteLine(temp.Data["main_chara"]);
            armp.SubTable.GetEntry(2).SetValueFromColumn("2", (uint)12345);
            Console.WriteLine(armp.SubTable.GetEntry(2).GetValueFromColumn("2"));


            //Console.WriteLine("Data: " + armp.MainTable.Entries[1].Data["table"]);
            //Console.WriteLine("Column [name]: " + armp.MainTable.Entries[1].Data["name"]);
            //Console.WriteLine("Column [character_id]: " + armp.MainTable.Entries[1].Data["character_id"]);
            //Console.WriteLine("Column [list_index]: " + armp.MainTable.Entries[1].Data["list_index"]);
        }
    }
}

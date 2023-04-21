using System.IO;
using System.Reflection;
using Yarhl.IO;

namespace LibARMP
{
    public static class ArmpFilePatcher
    {
        private static void PatchARMP (ARMP armp, DataStream outputDataStream)
        {
            var writer = new DataWriter(outputDataStream)
            {
                Endianness = EndiannessMode.LittleEndian,
                DefaultEncoding = System.Text.Encoding.UTF8,
            };
            if (armp.FormatVersion == Version.OldEngine)
            {
                writer.Endianness = EndiannessMode.BigEndian;
                writer.DefaultEncoding = System.Text.Encoding.GetEncoding(932);
            }

            //Write the original file to the new DataStream before patching values
            using (var originalFileDS = DataStreamFactory.FromStream(armp.File))
            {
                originalFileDS.WriteTo(writer.Stream);
            }

            PatchTableRecursive(writer, armp.MainTable);
        }


        /// <summary>
        /// Writes a patched ARMP to a file.
        /// </summary>
        /// <param name="armp">The ARMP to write.</param>
        /// <param name="path">The destination file path.</param>
        public static void PatchARMPToFile (ARMP armp, string path)
        {
            using (var datastream = DataStreamFactory.FromFile(path, FileOpenMode.Write))
            {
                PatchARMP(armp, datastream);
            }
        }



        /// <summary>
        /// Writes a patched ARMP to a stream.
        /// </summary>
        /// <param name="armp">The ARMP to write.</param>
        public static Stream PatchARMPToStream (ARMP armp)
        {
            MemoryStream stream = new MemoryStream();
            DataStream tempds = DataStreamFactory.FromMemory();
            PatchARMP(armp, tempds);
            tempds.WriteTo(stream);
            return stream;
        }



        /// <summary>
        /// Writes a patched ARMP to a byte array.
        /// </summary>
        /// <param name="armp"></param>
        public static byte[] PatchARMPToArray (ARMP armp)
        {
            DataStream tempds = DataStreamFactory.FromMemory();
            PatchARMP(armp, tempds);
            return tempds.ToArray();
        }


        //PLACEHOLDER
        private static void PatchTableRecursive(DataWriter writer, ArmpTable table)
        {
            foreach(ArmpTableColumn column in table.Columns)
            {
                if (table.EditedValues.ContainsKey(column.Name))
                {
                    foreach (int entryId in table.EditedValues[column.Name])
                    {
                        object entryvalue = table.GetEntry(entryId).GetValueFromColumn(column.Name);

                        writer.Stream.Seek(table.GetEntry(entryId).ColumnValueOffsets[column.Name]);

                        MethodInfo methodinfo = typeof(ArmpFileWriter).GetMethod("WriteType", BindingFlags.NonPublic | BindingFlags.Static);
                        MethodInfo methodref = methodinfo.MakeGenericMethod(column.Type.CSType);
                        methodref.Invoke(null, new object[] { writer, entryvalue });
                        
                    }
                }

                if (column.Type.CSType == typeof(ArmpTableMain))
                {
                    foreach (ArmpEntry entry in table.GetAllEntries())
                    {
                        try
                        {
                            ArmpTableMain tablevalue = (ArmpTableMain)entry.GetValueFromColumn(column.Name);
                            PatchTableRecursive(writer, tablevalue);
                        }
                        catch { }
                    }
                }
            }

            if (table.TableInfo.HasSubTable)
            {
                ArmpTableMain main = new ArmpTableMain(table);
                PatchTableRecursive(writer, main.SubTable);
            }
        }



        /// <summary>
        /// Writes a value of type T to the DataStream.
        /// </summary>
        /// <typeparam name="T">The type to read.</typeparam>
        /// <param name="writer">The DataWriter.</param>
        /// <param name="value">The value of type T to write.</param>
        private static void WriteType<T>(DataWriter writer, object value)
        {
            writer.WriteOfType<T>((T)value);
        }
    }
}

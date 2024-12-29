using BinaryExtensions;
using System.IO;
using System.Reflection;
using System.Text;

namespace LibARMP.IO
{
    public static class ArmpFilePatcher
    {
        static ArmpFilePatcher()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }


        private static void PatchARMP(ARMP armp, Stream outputStream)
        {
            var writer = new BinaryWriter(outputStream, Encoding.UTF8, true);

            //Write the original file to the new stream before patching values
            armp.File.WriteTo(outputStream);

            PatchTableRecursive(writer, armp.MainTable);
        }


        /// <summary>
        /// Writes a patched ARMP to a file.
        /// </summary>
        /// <param name="armp">The ARMP to write.</param>
        /// <param name="path">The destination file path.</param>
        public static void PatchARMPToFile(ARMP armp, string path)
        {
            using (Stream stream = new MemoryStream())
            {
                PatchARMP(armp, stream);
                File.WriteAllBytes(path, stream.ToArray());
            }
        }



        /// <summary>
        /// Writes a patched ARMP to a stream.
        /// </summary>
        /// <param name="armp">The ARMP to write.</param>
        public static Stream PatchARMPToStream(ARMP armp)
        {
            Stream stream = new MemoryStream();
            PatchARMP(armp, stream);
            return stream;
        }



        /// <summary>
        /// Writes a patched ARMP to a byte array.
        /// </summary>
        /// <param name="armp"></param>
        public static byte[] PatchARMPToArray(ARMP armp)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                PatchARMP(armp, stream);
                return stream.ToArray();
            }
        }


        //PLACEHOLDER
        private static void PatchTableRecursive(BinaryWriter writer, ArmpTable table)
        {
            foreach (ArmpTableColumn column in table.Columns)
            {
                if (table.EditedValues.ContainsKey(column.Name))
                {
                    foreach (uint entryId in table.EditedValues[column.Name])
                    {
                        object entryvalue = table.GetEntry(entryId).GetValueFromColumn(column.Name);

                        writer.BaseStream.Seek(table.GetEntry(entryId).ColumnValueOffsets[column.Name]);

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
        private static void WriteType<T>(BinaryWriter writer, object value)
        {
            writer.WriteByType((T)value);
        }
    }
}

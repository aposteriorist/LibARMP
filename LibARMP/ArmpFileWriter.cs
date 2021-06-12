using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace LibARMP
{
    public static class ArmpFileWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="armp"></param>
        /// <param name="path"></param>
        public static void SaveARMP (ARMP armp, string path)
        {
            using (var stream = DataStreamFactory.FromFile(path, FileOpenMode.Write))
            {
                var writer = new DataWriter(stream)
                {
                    Endianness = EndiannessMode.LittleEndian,
                };
                if (armp.isOldEngine) writer.Endianness = EndiannessMode.BigEndian;

                writer.Write("armp", false); //Magic
                if (armp.isOldEngine) writer.Write(0x02010000); //Endianness identifier for OE
                else writer.WriteTimes(0x00, 0x4);
                if (armp.isOldEngine) //Version and Revision are flipped on different endianess. Presumably both values are read together as an int32
                {
                    writer.Write(armp.Version);
                    writer.Write(armp.Revision);
                }
                else
                {
                    writer.Write(armp.Revision);
                    writer.Write(armp.Version);
                }
                writer.WriteTimes(0x00, 0x4); //File size (only used in OE, placeholder for now)

                if (armp.isOldEngine)
                {
                    //TODO
                }
                else
                {
                    writer.Write(32); //Pointer to main table. Always 0x20 with our way of writing the file.
                    writer.WriteTimes(0x00, 0xC); //Padding
                    WriteTable(writer, armp.MainTable);
                }

            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static void WriteTable (DataWriter writer, ArmpTable table)
        {
            long baseOffset = writer.Stream.Position;
            writer.WriteTimes(0x00, 0x50); //Placeholder table

            //Row and column counts
            writer.Stream.PushToPosition(baseOffset);
            writer.Write(table.Entries.Count);
            writer.Write(table.ColumnNames.Count);
            writer.Stream.PopPosition();

            int ptr = 0;
            //Row Validity
            if (table.RowValidity != null)
            {
                ptr = (int)writer.Stream.Position;
                Util.WriteBooleanBitmask(writer, table.RowValidity);
                writer.WritePadding(0x00, 8);
                writer.Stream.PushToPosition(baseOffset + 0x14);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Validity
            if (table.ColumnValidity != null)
            {
                ptr = (int)writer.Stream.Position;
                Util.WriteBooleanBitmask(writer, table.ColumnValidity);
                writer.WritePadding(0x00, 8);
                writer.Stream.PushToPosition(baseOffset + 0x38);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Row Names
            if (table.TableInfo.HasRowNames)
            {
                ptr = Util.WriteText(writer, table.RowNames);
                writer.Stream.PushToPosition(baseOffset + 0x10);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Names
            if (table.TableInfo.HasColumnNames)
            {
                ptr = Util.WriteText(writer, table.ColumnNames);
                writer.Stream.PushToPosition(baseOffset + 0x28);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Text
            if (table.Text.Count > 0)
            {
                //Force an update of the table text.

                //Get the columns of type string 
                List<string> stringTypeColumns = new List<string>();
                foreach (string column in table.ColumnNames)
                {
                    int columnIndex = table.ColumnNames.IndexOf(column);
                    if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["string"])
                    {
                        stringTypeColumns.Add(column);
                    }
                }

                List<string> textList = new List<string>();
                foreach (ArmpEntry entry in table.Entries)
                {
                    foreach (string column in stringTypeColumns)
                    {
                        string str = (string)entry.GetValueFromColumn(column);
                        if (!textList.Contains(str) && str != null) textList.Add(str);
                    }
                }
                table.Text = textList;

                ptr = Util.WriteText(writer, table.Text);
                writer.Stream.PushToPosition(baseOffset + 0x24);
                writer.Write(ptr);
                writer.Stream.PopPosition();
                //Text count
                writer.Stream.PushToPosition(baseOffset + 0x8);
                writer.Write(table.Text.Count);
                writer.Stream.PopPosition();
            }

            //Column Types
            ptr = (int)writer.Stream.Position;
            foreach (Type type in table.ColumnDataTypes)
            {
                sbyte typeID = -1;
                if (type != null)
                {
                    typeID = DataTypes.TypesV1Reverse[type];
                }
                writer.Write(typeID);
            }
            writer.Stream.PushToPosition(baseOffset + 0x18);
            writer.Write(ptr);
            writer.Stream.PopPosition();
            writer.WritePadding(0x00, 4);

            //Column Types Aux
            //TODO v2 table
            ptr = (int)writer.Stream.Position;
            foreach (Type type in table.ColumnDataTypesAux)
            {
                sbyte typeID = -1;
                if (type != null)
                {
                    typeID = DataTypes.TypesV1AuxReverse[type];
                }
                writer.Write(typeID);
            }
            writer.Stream.PushToPosition(baseOffset + 0x48);
            writer.Write(ptr);
            writer.Stream.PopPosition();
            writer.WritePadding(0x00, 4);

        }

    }
}

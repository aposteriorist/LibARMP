using LibARMP.Exceptions;
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
                    int mainTableBaseOffset = (int)writer.Stream.Position;
                    WriteTable(writer, armp.MainTable);
                    if (armp.SubTable != null)
                    {
                        int ptr = (int)writer.Stream.Position;
                        WriteTable(writer, armp.SubTable);
                        writer.Stream.PushToPosition(mainTableBaseOffset + 0x3C);
                        writer.Write(ptr);
                    }
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
            if (table.Text != null && table.Text.Count > 0)
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


            //Column Contents
            List<int> columnValueOffsets = new List<int>();
            foreach (string column in table.ColumnNames)
            {
                int columnIndex = table.ColumnNames.IndexOf(column);
                if (table.ColumnValidity != null && table.ColumnValidity[columnIndex] != true)
                {
                    columnValueOffsets.Add(0);
                }
                else
                {
                    columnValueOffsets.Add((int)writer.Stream.Position);
                    List<bool> boolList = new List<bool>(); //Init list in case it is a boolean column
                    List<ArmpTable> tableList = new List<ArmpTable>(); //Init list in case it is a table column
                    List<int> tableOffsetList = new List<int>(); //Init list in case it is a table column
                    foreach (ArmpEntry entry in table.Entries)
                    {
                        if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["string"])
                        {
                            if (entry.GetValueFromColumn(column) != null)
                            {
                                int index = table.Text.IndexOf((string)entry.GetValueFromColumn(column));
                                writer.Write(index);
                            }
                            else
                            {
                                writer.Write(-1);
                            }
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["boolean"])
                        {
                            boolList.Add((bool)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["table"])
                        {
                            try
                            {
                                tableList.Add((ArmpTable)entry.GetValueFromColumn(column));
                                tableOffsetList.Add((int)writer.Stream.Position);
                                writer.WriteTimes(0x00, 0x8);
                            } catch(ColumnNotFoundException e)
                            {
                                writer.WriteTimes(0x00, 0x8);
                            }
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["uint8"])
                        {
                            writer.Write((byte)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["int8"])
                        {
                            writer.Write((sbyte)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["uint16"])
                        {
                            writer.Write((UInt16)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["int16"])
                        {
                            writer.Write((Int16)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["uint32"])
                        {
                            writer.Write((UInt32)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["int32"])
                        {
                            writer.Write((Int32)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["uint64"])
                        {
                            writer.Write((UInt64)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["int64"])
                        {
                            writer.Write((Int64)entry.GetValueFromColumn(column));
                        }

                        else if (table.ColumnDataTypesAux[columnIndex] == DataTypes.Types["float32"])
                        {
                            writer.Write((Single)entry.GetValueFromColumn(column));
                        }
                    }

                    if (boolList.Count > 0) //Write booleans
                    {
                        Util.WriteBooleanBitmask(writer, boolList);
                    }
                    else if (tableList.Count > 0) //Write tables
                    {
                        int i = 0;
                        foreach (ArmpTable tableValue in tableList)
                        {
                            long pointer = writer.Stream.Position;
                            WriteTable(writer, tableValue);
                            writer.Stream.PushToPosition(tableOffsetList[i]);
                            writer.Write(pointer);
                            writer.Stream.PopPosition();
                            i++;
                        }
                    }
                }
            }

            writer.WritePadding(0x00, 4);
            int ptrColumnOffsetTable = (int)writer.Stream.Position;
            foreach(int offset in columnValueOffsets)
            {
                writer.Write(offset);
            }

            writer.Stream.PushToPosition(baseOffset + 0x1C);
            writer.Write(ptrColumnOffsetTable);
            writer.Stream.PopPosition();


            //TODO write tables


            //Row Indices
            if (table.RowIndices != null)
            {
                ptr = (int)writer.Stream.Position;

                foreach(ArmpEntry entry in table.Entries)
                {
                    writer.Write(entry.Index);
                }
                writer.Stream.PushToPosition(baseOffset + 0x30);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }

            //Column Indices
            if (table.ColumnIndices != null)
            {
                ptr = (int)writer.Stream.Position;
                foreach (int index in table.ColumnIndices)
                {
                    writer.Write(index);
                }
                writer.Stream.PushToPosition(baseOffset + 0x34);
                writer.Write(ptr);
                writer.Stream.PopPosition();
            }


            //TODO validitybool
        }

    }
}

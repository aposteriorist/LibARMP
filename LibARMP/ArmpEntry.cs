using LibARMP.Exceptions;
using System;
using System.Collections.Generic;
using Yarhl.FileFormat;
using Yarhl.IO;

namespace LibARMP
{
    public class ArmpEntry
    {

        public ArmpEntry()
        {

        }

        public ArmpEntry(int id, string name)
        {
            ID = id;
            Name = name;
            Data = new Dictionary<string, object>();
        }

        public ArmpEntry(int id, string name, int index)
        {
            ID = id;
            Name = name;
            Index = index;
            Data = new Dictionary<string, object>();
        }


        /// <summary>
        /// Gets or sets the entry ID.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the entry name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Data per column. (column, value)
        /// </summary>
        public IDictionary<string, object> Data { get; set; }

        /// <summary>
        /// Gets or sets the entry index. Can be NULL
        /// </summary>
        public int Index { get; set; }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        public object GetValueFromColumn(string column)
        {
            if (Data.ContainsKey(column))
            {
                return Data[column];
            }
            else
            {
                throw new ColumnNotFoundException("The column '"+column+"' does not exist.");
            }
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="columnIndex">The column index.</param>
        public object GetValueFromColumn(int columnIndex)
        {
            List<string> keys = new List<string>(Data.Keys);

            if (keys.Count > columnIndex)
            {
                return Data[keys[columnIndex]];
            }
            else
            {
                throw new ColumnNotFoundException("A column with index '" + columnIndex + "' does not exist.");
            }
        }



        /// <summary>
        /// Sets the value for the specified column. NO TYPE CHECKS ARE PERFORMED
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <param name="value">The value to write.</param>
        public void SetValueFromColumn (string column, object value)
        {
            if (Data.ContainsKey(column))
            {
                Data[column] = value;
            }
            else
            {
                throw new ColumnNotFoundException("The column '" + column + "' does not exist.");
            }
        }

    }
}

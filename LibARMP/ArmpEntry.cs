using LibARMP.Exceptions;
using System;
using System.Collections.Generic;

namespace LibARMP
{   
    [Serializable]
    public class ArmpEntry
    {

        public ArmpEntry()
        {
            Data = new Dictionary<string, object>();
        }

        public ArmpEntry(int id, string name) : this()
        {
            ID = id;
            Name = name;
        }

        public ArmpEntry(int id, string name, int index) : this(id, name)
        {
            Index = index;
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
        /// Gets or sets if the entry is valid. Can be NULL
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the entry flags. Can be NULL
        /// </summary>
        public bool[] Flags { get; set; }

        /// <summary>
        /// PLACEHOLDER: Offsets for column values used by the patcher.
        /// </summary>
        internal Dictionary<string, int> ColumnValueOffsets = new Dictionary<string, int>();



        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        public object GetValueFromColumn (string columnName)
        {
            if (Data.ContainsKey(columnName))
            {
                return Data[columnName];
            }
            else
            {
                throw new ColumnNotFoundException($"The column '{columnName}' does not exist.");
            }
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="columnIndex">The column index.</param>
        public object GetValueFromColumn (int columnIndex)
        {
            List<string> keys = new List<string>(Data.Keys);

            if (keys.Count > columnIndex)
            {
                return Data[keys[columnIndex]];
            }
            else
            {
                throw new ColumnNotFoundException($"A column with index '{columnIndex}' does not exist.");
            }
        }



        /// <summary>
        /// Sets the value for the specified column. NO TYPE CHECKS ARE PERFORMED
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="value">The value to write.</param>
        public void SetValueFromColumn (string columnName, object value)
        {
            if (Data.ContainsKey(columnName))
            {
                Data[columnName] = value;
            }
            else
            {
                throw new ColumnNotFoundException($"The column '{columnName}' does not exist.");
            }
        }

    }
}

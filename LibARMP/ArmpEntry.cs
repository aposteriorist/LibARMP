using LibARMP.Exceptions;
using System;
using System.Collections.Generic;

namespace LibARMP
{   
    [Serializable]
    public class ArmpEntry
    {

        internal ArmpEntry(ArmpTable parentTable)
        {
            Data = new Dictionary<string, object>();
            ParentTable = parentTable;

            if (ParentTable.TableInfo.HasEntryIndices)
                Index = ID;

            if (ParentTable.TableInfo.HasEntryValidity)
                IsValid = true;

            if (ParentTable.TableInfo.HasExtraFieldInfo && ParentTable.TableInfo.FormatVersion == Version.DragonEngineV1)
                Flags = new bool[8] { false, false, false, false, false, false, false, false };
        }

        internal ArmpEntry(ArmpTable parentTable, int id, string name) : this(parentTable)
        {
            ID = id;
            Name = name;
        }

        internal ArmpEntry(ArmpTable parentTable, int id, string name, int index) : this(parentTable, id, name)
        {
            Index = index;
        }


        /// <summary>
        /// Gets the entry ID.
        /// </summary>
        public int ID { get; internal set; }

        /// <summary>
        /// Gets or sets the entry name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Data per column. (column, value)
        /// </summary>
        internal IDictionary<string, object> Data { get; set; }

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
        /// The ArmpTable this entry belongs to.
        /// </summary>
        internal ArmpTable ParentTable { get; set; }

        /// <summary>
        /// PLACEHOLDER: Offsets for column values used by the patcher.
        /// </summary>
        internal Dictionary<string, int> ColumnValueOffsets = new Dictionary<string, int>();



        /// <summary>
        /// Sets the value for the column to default.
        /// </summary>
        /// <param name="column">The ArmpTableColumn.</param>
        internal void SetDefaultColumnContent (ArmpTableColumn column)
        {
            if (!column.IsSpecial)
            {
                SetValueFromColumn(column.Name, column.Type.DefaultValue);
            }
        }


        /// <summary>
        /// Sets the value for the column to default.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        internal void SetDefaultColumnContent (string columnName)
        {
            try
            {
                ArmpTableColumn column = ParentTable.ColumnNameCache[columnName];
                SetDefaultColumnContent(column);
            }
            catch
            {
                throw new Exception($"The column '{columnName}' does not exist in this table.");
            }
        }


        /// <summary>
        /// Sets the values for all columns to their default.
        /// </summary>
        internal void SetDefaultColumnContent()
        {
            foreach (ArmpTableColumn column in ParentTable.Columns)
            {
                SetDefaultColumnContent(column);
            }
        }


        /// <summary>
        /// Removes the contents of the specified column.
        /// </summary>
        /// <returns>A boolean indicating if the operation completed successfully.</returns>
        internal bool RemoveColumnContent (string columnName)
        {
            try
            {
                Data.Remove(columnName);
                return true;
            }
            catch 
            {
                return false;
            }
        }


        /// <summary>
        /// Removes the contents of the specified column.
        /// </summary>
        /// <returns>A boolean indicating if the operation completed successfully.</returns>
        internal bool RemoveColumnContent (ArmpTableColumn column)
        {
            return RemoveColumnContent(column.Name);
        }


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
                throw new ColumnNotFoundException($"The column '{columnName}' does not exist or has no data.");
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
                throw new ColumnNotFoundException($"A column with index '{columnIndex}' does not exist or has no data.");
            }
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="column">The ArmpTableColumn.</param>
        public object GetValueFromColumn (ArmpTableColumn column)
        {
            return GetValueFromColumn(column.Name);
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        public T GetValueFromColumn<T> (string columnName)
        {
            object result = GetValueFromColumn(columnName);
            try
            {
                return (T)result;
            }
            catch
            {
                Type paramType = typeof(T);
                throw new Exception($"Cannot convert from {result.GetType()} to {paramType}.");
            }
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="columnIndex">The column index.</param>
        public T GetValueFromColumn<T> (int columnIndex)
        {
            object result = GetValueFromColumn(columnIndex);
            try
            {
                return (T)result;
            }
            catch
            {
                Type paramType = typeof(T);
                throw new Exception($"Cannot convert from {result.GetType()} to {paramType}");
            }
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="column">The ArmpTableColumn.</param>
        public T GetValueFromColumn<T> (ArmpTableColumn column)
        {
            object result = GetValueFromColumn(column.Name);
            try
            {
                return (T)result;
            }
            catch
            {
                Type paramType = typeof(T);
                throw new Exception($"Cannot convert from {result.GetType()} to {paramType}.");
            }
        }


        /// <summary>
        /// Sets the value for the specified column.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="value">The value to write.</param>
        public void SetValueFromColumn (string columnName, object value)
        {
            Type targetType = ParentTable.GetColumn(columnName).Type.CSType;

            if (value == null)
                return;

            if (targetType != value.GetType())
                throw new Exception($"Type mismatch. Expected {targetType} and got {value.GetType()}.");

            Data[columnName] = value;
        }
    }
}

using LibARMP.Exceptions;
using System;
using System.Collections.Generic;

namespace LibARMP
{   
    [Serializable]
    public class ArmpEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArmpEntry"/> class.
        /// </summary>
        /// <param name="parentTable">The table that contains this entry.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ArmpEntry"/> class.
        /// </summary>
        /// <param name="parentTable">The table that contains this entry.</param>
        /// <param name="id">The entry ID.</param>
        /// <param name="name">The entry name.</param>
        internal ArmpEntry(ArmpTable parentTable, int id, string name) : this(parentTable)
        {
            ID = id;
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArmpEntry"/> class.
        /// </summary>
        /// <param name="parentTable">The table that contains this entry.</param>
        /// <param name="id">The entry ID.</param>
        /// <param name="name">The entry name.</param>
        /// <param name="index">The entry index.</param>
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
        /// Data per column.
        /// </summary>
        /// <remarks><para>column name : value</para></remarks>
        internal IDictionary<string, object> Data { get; set; }

        /// <summary>
        /// Gets or sets the entry index.
        /// </summary>
        /// <remarks><para>Can be null if unused.</para></remarks>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets if the entry is valid.
        /// </summary>
        /// <remarks><para>Can be null if unused.</para></remarks>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the entry flags.
        /// </summary>
        /// <remarks><para>Can be null if unused.</para></remarks>
        public bool[] Flags { get; set; }

        /// <summary>
        /// The <see cref="ArmpTable"/> this entry belongs to.
        /// </summary>
        internal ArmpTable ParentTable { get; set; }

        /// <summary>
        /// PLACEHOLDER: Offsets for column values used by the patcher.
        /// </summary>
        internal Dictionary<string, int> ColumnValueOffsets = new Dictionary<string, int>();



        /// <summary>
        /// Sets the value for the column to default.
        /// </summary>
        /// <param name="column">The <see cref="ArmpTableColumn"/>.</param>
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
        /// <exception cref="ColumnNotFoundException">The column name doesn't match any columns in the table.</exception>
        internal void SetDefaultColumnContent (string columnName)
        {
            try
            {
                ArmpTableColumn column = ParentTable.ColumnNameCache[columnName];
                SetDefaultColumnContent(column);
            }
            catch
            {
                throw new ColumnNotFoundException(columnName);
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
        /// <returns>A <see cref="Boolean"/> indicating if the operation completed successfully.</returns>
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
        /// <returns>A <see cref="Boolean"/> indicating if the operation completed successfully.</returns>
        internal bool RemoveColumnContent (ArmpTableColumn column)
        {
            return RemoveColumnContent(column.Name);
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <exception cref="ColumnNotFoundException">The column name doesn't match any columns in the table.</exception>
        /// <exception cref="ColumnNoDataException">The column has no data.</exception>
        public object GetValueFromColumn (string columnName)
        {
            if (Data.ContainsKey(columnName))
            {
                return Data[columnName];
            }
            else //Check if the column doesn't exist or has no data
            {
                try
                {
                    ParentTable.GetColumn(columnName);
                } catch (ColumnNotFoundException ex)
                {
                    throw ex;
                }

                throw new ColumnNoDataException(columnName);
            }
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="columnIndex">The column index.</param>
        /// <exception cref="ColumnNotFoundException">The column name doesn't match any columns in the table.</exception>
        /// <exception cref="ColumnNoDataException">The column has no data.</exception>
        public object GetValueFromColumn (int columnIndex)
        {
            List<string> keys = new List<string>(Data.Keys);

            if (keys.Count > columnIndex)
            {
                try
                {
                    return Data[keys[columnIndex]];
                } catch (Exception)
                {
                    throw new ColumnNoDataException(columnIndex);
                }
            }
            else
            {
                throw new ColumnNotFoundException(columnIndex);
            }
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="column">The <see cref="ArmpTableColumn"/>.</param>
        public object GetValueFromColumn (ArmpTableColumn column)
        {
            return GetValueFromColumn(column.Name);
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <exception cref="InvalidTypeConversionException">The column type cannot be converted to the requested type.</exception>
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
                throw new InvalidTypeConversionException(result.GetType(), paramType);
            }
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="columnIndex">The column index.</param>
        /// <exception cref="InvalidTypeConversionException">The column type cannot be converted to the requested type.</exception>
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
                throw new InvalidTypeConversionException(result.GetType(), paramType);
            }
        }


        /// <summary>
        /// Gets the value for the specified column.
        /// </summary>
        /// <param name="column">The <see cref="ArmpTableColumn"/>.</param>
        /// <exception cref="InvalidTypeConversionException">The column type cannot be converted to the requested type.</exception>
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
                throw new InvalidTypeConversionException(result.GetType(), paramType);
            }
        }


        /// <summary>
        /// Sets the value for the specified column.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="TypeMismatchException">The column type does not match the type of the provided object.</exception>
        public void SetValueFromColumn (string columnName, object value)
        {
            Type targetType = ParentTable.GetColumn(columnName).Type.CSType;

            if (value == null)
                return;

            if (targetType != value.GetType())
                throw new TypeMismatchException(targetType, value.GetType());

            Data[columnName] = value;
        }
    }
}

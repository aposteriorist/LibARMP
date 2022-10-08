using LibARMP.Exceptions;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace LibARMP
{
    [Serializable]
    public class ArmpTable
    {
        public ArmpTable()
        {
            Entries = new List<ArmpEntry>();
            ColumnDataTypesAuxTable = new List<List<int>>(); //v2 only
            NoDataColumns = new List<int>();
            EmptyValues = new Dictionary<int, List<bool>>();
            EmptyValuesIsNegativeOffset = new List<bool>();
        }

        /// <summary>
        /// Pointers, flags and general information.
        /// </summary>
        public ArmpTableInfo TableInfo { get; set; }

        /// <summary>
        /// Entry names.
        /// </summary>
        internal List<string> RowNames { get; set; }

        /// <summary>
        /// Column names.
        /// </summary>
        internal List<string> ColumnNames { get; set; }

        /// <summary>
        /// Text.
        /// </summary>
        internal List<string> Text { get; set; }

        /// <summary>
        /// Row validity.
        /// </summary>
        internal List<bool> RowValidity { get; set; }

        /// <summary>
        /// Column validity.
        /// </summary>
        internal List<bool> ColumnValidity { get; set; }

        /// <summary>
        /// Row indices.
        /// </summary>
        internal List<int> RowIndices { get; set; }

        /// <summary>
        /// Column indices. These override the regular order.
        /// </summary>
        internal List<int> ColumnIndices { get; set; }

        /// <summary>
        /// Column data types.
        /// </summary>
        internal List<Type> ColumnDataTypes { get; set; }

        /// <summary>
        /// Column data types (auxiliary). Only used in storage mode 0.
        /// </summary>
        internal List<Type> ColumnDataTypesAux { get; set; }

        /// <summary>
        /// Column data types (auxiliary). Only used in storage mode 1. [Type ID, Distance, Array Size, Unknown]
        /// </summary>
        internal List<List<int>> ColumnDataTypesAuxTable { get; set; }

        /// <summary>
        /// List of booleans indicating if the column with the same index is special (arrays/lists).
        /// </summary>
        internal List<bool> SpecialColumns { get; set; }

        /// <summary>
        /// List of ints used as column metadata.
        /// </summary>
        internal List<int> ColumnMetadata { get; set; }

        /// <summary>
        /// List of entries.
        /// </summary>
        internal List<ArmpEntry> Entries { get; set; }

        /// <summary>
        /// Index list of columns that for some reason have no data (pointer = -1) despite being marked as valid.
        /// </summary>
        internal List<int> NoDataColumns { get; set; }

        /// <summary>
        /// Values marked as empty for specific columns (despite actually having a value) [column index, list<bool> (length = row count)]
        /// </summary>
        internal Dictionary<int, List<bool>> EmptyValues { get; set; }

        /// <summary>
        /// DEBUG: Boolean list (length = column count) to indicate if the offset in the empty values offset list was -1. The difference between 0 and -1 is unknown.
        /// </summary>
        internal List<bool> EmptyValuesIsNegativeOffset { get; set; }




        /// <summary>
        /// Returns all entries in the table.
        /// </summary>
        /// <returns>An ArmpEntry list.</returns>
        public List<ArmpEntry> GetAllEntries ()
        {
            try
            {
                return Entries;
            }
            catch
            {
                throw new EntryNotFoundException("No entries found for this table.");
            }
        }


        /// <summary>
        /// Returns a specific entry in the table.
        /// </summary>
        /// <returns>An ArmpEntry list.</returns>
        public ArmpEntry GetEntry (int id)
        {
            try
            {
                return Entries[id];
            }
            catch
            {
                throw new EntryNotFoundException(String.Format("No entry with ID {0}", id));
            }
        }


        /// <summary>
        /// Returns a specific entry in the table.
        /// </summary>
        /// <returns>An ArmpEntry list.</returns>
        public ArmpEntry GetEntry(string name)
        {
            try
            {
                foreach (ArmpEntry entry in GetAllEntries())
                {
                    if (entry.Name == name) return entry;
                }
                throw new EntryNotFoundException(String.Format("No entry with name '{0}'", name));
            }
            catch
            {
                throw new EntryNotFoundException(String.Format("No entry with name '{0}'", name));
            }
        }


        /// <summary>
        /// Gets the row names.
        /// </summary>
        /// <returns>A string list.</returns>
        public List<string> GetRowNames()
        {
            if (RowNames == null)
                throw new Exception("There are no row names in this table.");

            return RowNames;
        }


        /// <summary>
        /// Gets the name of a specific row.
        /// </summary>
        /// <param name="id">The ID of the row.</param>
        /// <returns>A string.</returns>
        public string GetRowName(int id)
        {
            if (RowNames == null)
                throw new Exception("There are no row names in this table.");

            try
            {
                return RowNames[id];
            }
            catch
            {
                throw new Exception(String.Format("No row name with ID {0}", id));
            }
        }


        /// <summary>
        /// Gets the column names.
        /// </summary>
        /// <param name="includeSpecials">Include special columns? (Array data types)</param>
        /// <returns>A string list.</returns>
        public List<string> GetColumnNames(bool includeSpecials = true)
        {
            List<string> returnList = new List<string>();
            for (int i=0; i<ColumnNames.Count; i++)
            {
                if(SpecialColumns != null && SpecialColumns[i] == true)
                {
                    if (includeSpecials) returnList.Add(ColumnNames[i]);
                }
                else
                {
                    returnList.Add(ColumnNames[i]);
                }
            }
            return returnList;
        }


        /// <summary>
        /// Gets the column's data type.
        /// </summary>
        /// <param name="column"></param>
        /// <returns>The column Type.</returns>
        public Type GetColumnDataType(string column)
        {
            List<Type> dataTypes = ColumnDataTypesAux; //Default for DE v1
            if (TableInfo.IsOldEngine || TableInfo.IsIshin || TableInfo.IsDragonEngineV2) dataTypes = ColumnDataTypes;
            if (!ColumnNames.Contains(column)) throw new ColumnNotFoundException(String.Format("The column '{0}' does not exist in this table.", column));
            int columnIndex = ColumnNames.IndexOf(column);
            return dataTypes[columnIndex];
        }


        /// <summary>
        /// Gets a list of column names matching the type.
        /// </summary>
        /// <param name="type">The Type to look for.</param>
        /// <returns>A string list.</returns>
        public List<string> GetColumnNamesByType (Type type)
        {
            List<string> returnList = new List<string>();
            foreach (string column in GetColumnNames(true))
            {
                if (GetColumnDataType(column) == type) 
                    returnList.Add(column);
            }

            return returnList;
        }


        /// <summary>
        /// Gets a list of column names matching the type.
        /// </summary>
        /// <typeparam name="T">The Type to look for.</typeparam>
        /// <returns>A string list.</returns>
        public List<string> GetColumnNamesByType<T>()
        {
            Type type = typeof(T);
            return GetColumnNamesByType(type);
        }


        /// <summary>
        /// Gets a list of column indices matching the type.
        /// </summary>
        /// <param name="type">The Type to look for.</param>
        /// <returns>An int list.</returns>
        public List<int> GetColumnIndicesByType (Type type)
        {
            List<int> returnList = new List<int>();
            foreach (string column in GetColumnNames(true))
            {
                if (GetColumnDataType(column) == type) returnList.Add(ColumnNames.IndexOf(column));
            }

            return returnList;
        }


        /// <summary>
        /// Gets a list of column indices matching the type.
        /// </summary>
        /// <typeparam name="T">The Type to look for.</typeparam>
        /// <returns>An int list.</returns>
        public List<int> GetColumnIndicesByType<T>()
        {
            Type type = typeof(T);
            return GetColumnIndicesByType(type);
        }


        /// <summary>
        /// Gets the column index by name.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>The column index.</returns>
        public int GetColumnIndex (string column)
        {
            try
            {
                return ColumnNames.IndexOf(column);
            }
            catch
            {
                throw new Exception(String.Format("No column with name '{0}'.", column));
            }
        }


        /// <summary>
        /// Gets a column's override index.
        /// </summary>
        /// <param name="index">The base column index.</param>
        /// <returns>The override index.</returns>
        public int GetColumnOverrideIndex(int index)
        {
            if (ColumnIndices == null) throw new Exception("This table has no column index overrides.");

            try
            {
                return ColumnIndices[index];
            }
            catch
            {
                throw new Exception(String.Format("There is no column with index {0}.", index));
            }
        }


        /// <summary>
        /// Gets a column's override index.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>The override index.</returns>
        public int GetColumnOverrideIndex(string column)
        {
            if (ColumnIndices == null) throw new Exception("This table has no column index overrides.");

            try
            {
                int index = ColumnNames.IndexOf(column);
                return ColumnIndices[index];
            }
            catch
            {
                throw new Exception(String.Format("There is no column with name '{0}'.", column));
            }
        }


        /// <summary>
        /// Sets a column's override index.
        /// </summary>
        /// <param name="index">The base column index.</param>
        /// <param name="newOverrideIndex">The new override index.</param>
        public void SetColumnOverrideIndex(int index, int newOverrideIndex)
        {
            if (ColumnIndices == null) throw new Exception("This table has no column index overrides.");

            try
            {
                ColumnIndices[index] = newOverrideIndex;
            }
            catch
            {
                throw new Exception(String.Format("There is no column with index {0}.", index));
            }
        }


        /// <summary>
        /// Sets a column's override index.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <param name="newOverrideIndex">The new override index.</param>
        public void SetColumnOverrideIndex(string column, int newOverrideIndex)
        {
            if (ColumnIndices == null) throw new Exception("This table has no column index overrides.");

            try
            {
                int index = GetColumnIndex(column);
                ColumnIndices[index] = newOverrideIndex;
            }
            catch
            {
                throw new Exception(String.Format("There is no column with name '{0}'.", column));
            }
        }


        /// <summary>
        /// Gets a boolean indicating if the column is valid.
        /// </summary>
        /// <param name="columnIndex">The column index.</param>
        /// <returns>A boolean.</returns>
        public bool IsColumnValid (int columnIndex)
        {
            if (ColumnValidity == null) throw new Exception("This table has no column validity.");

            try
            {
                return ColumnValidity[columnIndex];
            }
            catch
            {
                throw new Exception(String.Format("No column with index {0}", columnIndex));
            }
        }


        /// <summary>
        /// Gets a boolean indicating if the column is valid.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>A boolean.</returns>
        public bool IsColumnValid (string column)
        {
            if (ColumnValidity == null) throw new Exception("This table has no column validity.");

            try
            {
                int columnIndex = GetColumnIndex(column);
                return ColumnValidity[columnIndex];
            }
            catch
            {
                throw new Exception(String.Format("No column with name '{0}'", column));
            }
        }


        public void SetColumnValidity (string column, bool validity)
        {
            //TODO: Makes a column valid/invalid. Clear all the values if invalid, fill with default values if valid.
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns if the column is special (array/list). Only used in v2.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>A boolean</returns>
        public bool IsColumnSpecial (string column)
        {
            if (!ColumnNames.Contains(column)) throw new ColumnNotFoundException(String.Format("The column '{0}' does not exist in this table.", column));
            if (SpecialColumns != null)
            {
                int columnIndex = ColumnNames.IndexOf(column);
                return SpecialColumns[columnIndex];
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Searches all entries for matching names.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>An ArmpEntry list.</returns>
        public List<ArmpEntry> SearchByName (string name)
        {
            List<ArmpEntry> returnList = new List<ArmpEntry>();
            foreach(ArmpEntry entry in Entries)
            {
                if (entry.Name.Contains(name)) returnList.Add(entry);
            }
            return returnList;
        }


        /// <summary>
        /// Searches all entries for matching values in the specified column.
        /// </summary>
        /// <param name="column">The column containing the value to find.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>An ArmpEntry list.</returns>
        public List<ArmpEntry> SearchByValue (string column, object value)
        {
            if (!ColumnNames.Contains(column)) throw new ColumnNotFoundException(String.Format("The column '{0}' does not exist in this table.", column));
            int columnIndex = ColumnNames.IndexOf(column);
            List<Type> dataTypes = ColumnDataTypesAux; //Default for DE v1
            if (TableInfo.IsOldEngine || TableInfo.IsIshin || TableInfo.IsDragonEngineV2) dataTypes = ColumnDataTypes;
            value = Convert.ChangeType(value, dataTypes[columnIndex]);

            List<ArmpEntry> returnList = new List<ArmpEntry>();
            foreach (ArmpEntry entry in Entries)
            {
                if (value.Equals(entry.Data[column]))
                {
                    returnList.Add(entry);
                }
            }
            return returnList;
        }


        /// <summary>
        /// Adds an entry to the end of the table.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        public void AddEntry (ArmpEntry entry)
        {
            int id = Entries.Count;
            if (entry.Name != null) RowNames.Add(entry.Name);
            entry.ID = id;
            entry.Index = id;
            Entries.Add(entry);
            //TODO create additional entry related data
        }


        /// <summary>
        /// Inserts an entry into the table at the specified index. NOT IMPLEMENTED
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        /// <param name="id">Place in which the entry will be inserted.</param>
        public void InsertEntry (ArmpEntry entry, int id)
        {
            //TODO
            throw new NotImplementedException();
        }


        /// <summary>
        /// Copy a specified entry.
        /// </summary>
        /// <param name="id">The entry to copy.</param>
        public ArmpEntry CopyEntry (int id)
        {
            ArmpEntry entry;
            entry = GetEntry(id);
            return Util.DeepCopy<ArmpEntry>(entry);
        }


        /// <summary>
        /// Deletes the specified entry and updates the ids for any entries after it.
        /// </summary>
        /// <param name="id">The entry to delete.</param>
        public void DeleteEntry (int id)
        {
            //TODO
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sets the value of a specified column in the specified entry.
        /// </summary>
        /// <param name="id">The entry to modify.</param>
        /// <param name="column">The column name.</param>
        /// <param name="value">The value to write.</param>
        public void SetValue (int id, string column, object value)
        {
            int columnIndex = ColumnNames.IndexOf(column);
            List<Type> dataTypes = ColumnDataTypesAux; //Default for DE v1
            if (TableInfo.IsOldEngine || TableInfo.IsIshin || TableInfo.IsDragonEngineV2) dataTypes = ColumnDataTypes;

            if (columnIndex != -1)
            {
                if (value.GetType() == dataTypes[columnIndex])
                {
                    ArmpEntry entry = GetEntry(id);
                    entry.SetValueFromColumn(column, value);
                }
                else
                {
                    throw new Exception(String.Format("Type mismatch. Expected {0} and got {1}.", dataTypes[columnIndex], value.GetType()));
                }
            }
            else
            {
                throw new ColumnNotFoundException(String.Format("The column '{0}' does not exist.", column));
            }
        }


        /// <summary>
        /// Sets the selected column as string type. (This is only needed for Old Engine files with text).
        /// </summary>
        /// <param name="column">The column name.</param>
        public void SetTextColumnOE (string column)
        {
            int columnIndex = ColumnNames.IndexOf(column);
            if (columnIndex != -1)
            {
                ColumnDataTypes[columnIndex] = typeof(string);
                foreach (ArmpEntry entry in Entries)
                {
                    Int16 textIndex = (Int16)entry.Data[column];
                    if (textIndex != -1) entry.Data[column] = Text[textIndex];
                    else entry.Data[column] = null;
                }
            }
            else
            {
                throw new ColumnNotFoundException(String.Format("The column '{0}' does not exist.", column));
            }
        }


        /// <summary>
        /// Returns a dummy entry for the specified table. TODO: THIS IS A PLACEHOLDER
        /// </summary>
        public ArmpEntry GenerateTemplateArmpEntry()
        {
            //Entry 0 is always empty, dirty approach is to make a copy.
            //FIXME This wont work for empty armps (not a realistic case but an issue regardless) and subtables may have weird results.
            ArmpEntry entry0;
            entry0 = GetEntry(0);
            return Util.DeepCopy<ArmpEntry>(entry0);
        }
    }
}

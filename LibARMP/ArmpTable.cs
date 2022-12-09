using LibARMP.Exceptions;
using System;
using System.Collections.Generic;

namespace LibARMP
{
    [Serializable]
    public class ArmpTable
    {

        internal Dictionary<string, ArmpTableColumn> ColumnNameCache { get; set; }

        internal ArmpTable()
        {
            ColumnNameCache = new Dictionary<string, ArmpTableColumn>();
            Entries = new List<ArmpEntry>();
            Columns = new List<ArmpTableColumn>();
            EmptyValues = new Dictionary<int, List<bool>>();
            EmptyValuesIsNegativeOffset = new List<bool>();
        }

        /// <summary>
        /// Flags and general information.
        /// </summary>
        public ArmpTableInfo TableInfo { get; internal set; }

        /// <summary>
        /// Entry names.
        /// </summary>
        internal List<string> EntryNames { get; set; }

        /// <summary>
        /// Text.
        /// </summary>
        internal List<string> Text { get; set; }

        /// <summary>
        /// Entry validity. 
        /// </summary>
        internal List<bool> EntryValidity { get; set; }

        /// <summary>
        /// Entry indices.
        /// </summary>
        internal List<int> EntryIndices { get; set; }

        /// <summary>
        /// Columns.
        /// </summary>
        internal List<ArmpTableColumn> Columns { get; set; }

        /// <summary>
        /// List of entries.
        /// </summary>
        internal List<ArmpEntry> Entries { get; set; }

        /// <summary>
        /// Values marked as empty for specific columns (despite actually having a value) [column index, list<bool> (length = entry count)]
        /// </summary>
        internal Dictionary<int, List<bool>> EmptyValues { get; set; }

        /// <summary>
        /// DEBUG: Boolean list (length = column count) to indicate if the offset in the empty values offset list was -1. The difference between 0 and -1 is unknown.
        /// </summary>
        internal List<bool> EmptyValuesIsNegativeOffset { get; set; }

        /// <summary>
        /// PLACEHOLDER: Edited values for patcher. [column:list of entry ids]
        /// </summary>
        internal Dictionary<string, List<int>> EditedValues = new Dictionary<string, List<int>>();



        /// <summary>
        /// Refreshes the Column Name Cache.
        /// </summary>
        internal void RefreshColumnNameCache()
        {
            ColumnNameCache.Clear();
            foreach (ArmpTableColumn column in Columns)
            {
                ColumnNameCache.Add(column.Name, column);
            }
        }


        /// <summary>
        /// Returns all entries in the table.
        /// </summary>
        /// <returns>An ArmpEntry list.</returns>
        public List<ArmpEntry> GetAllEntries()
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
                throw new EntryNotFoundException($"No entry with ID {id}");
            }
        }


        /// <summary>
        /// Returns a specific entry in the table.
        /// </summary>
        /// <returns>An ArmpEntry list.</returns>
        public ArmpEntry GetEntry (string name)
        {
            try
            {
                foreach (ArmpEntry entry in Entries)
                {
                    if (entry.Name == name) return entry;
                }
                throw new EntryNotFoundException($"No entry with name '{name}'");
            }
            catch
            {
                throw new EntryNotFoundException($"No entry with name '{name}'");
            }
        }


        public bool SetEntry (ArmpEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("Entry cannot be null.");

            //TODO need to check that the entry columns and data match the current table
            Entries[entry.ID] = Util.DeepCopy<ArmpEntry>(entry);
            return true; //PLACEHOLDER
        }


        /// <summary>
        /// Gets all entry names.
        /// </summary>
        /// <returns>A string list.</returns>
        public List<string> GetEntryNames()
        {
            if (!TableInfo.HasEntryNames)
                throw new Exception("There are no entry names in this table.");

            List<string> returnList = new List<string>();
            foreach (ArmpEntry entry in Entries)
            {
                returnList.Add(entry.Name);
            }

            return returnList;
        }


        /// <summary>
        /// Gets the name of a specific entry.
        /// </summary>
        /// <param name="id">The entry ID.</param>
        /// <returns>A string.</returns>
        public string GetEntryName (int id)
        {
            if (!TableInfo.HasEntryNames)
                throw new Exception("There are no entry names in this table.");

            try
            {
                return Entries[id].Name;
            }
            catch
            {
                throw new Exception($"No entry with ID {id}");
            }
        }


        /// <summary>
        /// Gets a specific column in the table.
        /// </summary>
        /// <param name="id">The column ID.</param>
        /// <returns>An ArmpTableColumn.</returns>
        public ArmpTableColumn GetColumn (int id)
        {
            try
            {
                return Columns[id];
            }
            catch
            {
                throw new Exception($"No column with ID {id} in this table.");
            }
        }


        /// <summary>
        /// Gets a specific column in the table.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>An ArmpTableColumn.</returns>
        public ArmpTableColumn GetColumn(string columnName)
        {
            try
            {
                return ColumnNameCache[columnName];
            }
            catch
            {
                throw new Exception($"No column with name '{columnName}' in this table.");
            }
        }


        /// <summary>
        /// Gets the column names.
        /// </summary>
        /// <param name="includeSpecials">Include special columns? (Array data types. These columns do not contain data)</param>
        /// <returns>A string list.</returns>
        public List<string> GetColumnNames (bool includeSpecials = true)
        {
            List<string> returnList = new List<string>();

            foreach (ArmpTableColumn column in Columns)
            {
                if (column.IsSpecial && !includeSpecials) continue;
                returnList.Add(column.Name);
            }

            return returnList;
        }


        /// <summary>
        /// Gets the column name.
        /// </summary>
        /// <param name="index">The column index.</param>
        /// <returns>A string.</returns>
        public string GetColumnName (int index)
        {
            try
            {
                return Columns[index].Name;
            }
            catch
            {
                throw new ColumnNotFoundException($"No column with index {index}.");
            }
        }


        /// <summary>
        /// Gets the column's data type.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>The column Type.</returns>
        public Type GetColumnDataType (string columnName)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                return ColumnNameCache[columnName].ColumnType;
            }
            throw new ColumnNotFoundException($"The column '{columnName}' does not exist in this table.");   
        }


        /// <summary>
        /// Gets a list of column names matching the type.
        /// </summary>
        /// <param name="type">The Type to look for.</param>
        /// <returns>A string list.</returns>
        public List<string> GetColumnNamesByType (Type type)
        {
            List<string> returnList = new List<string>();
            foreach (ArmpTableColumn column in Columns)
            {
                if (column.ColumnType == type)
                    returnList.Add(column.Name);
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
            foreach (ArmpTableColumn column in Columns)
            {
                if (column.ColumnType == type) returnList.Add(column.ID);
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
        /// <param name="columnName">The column name.</param>
        /// <returns>The column index.</returns>
        public int GetColumnIndex (string columnName)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                return ColumnNameCache[columnName].ID;
            }
            throw new Exception($"No column with name '{columnName}'.");
        }


        /// <summary>
        /// Gets a column's override index.
        /// </summary>
        /// <param name="index">The base column index.</param>
        /// <returns>The override index.</returns>
        public int GetColumnOverrideIndex (int index)
        {
            if (!TableInfo.HasColumnIndices) throw new Exception("This table has no column index overrides.");

            try
            {
                return Columns[index].Index;
            }
            catch
            {
                throw new Exception($"There is no column with index {index}.");
            }
        }


        /// <summary>
        /// Gets a column's override index.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>The override index.</returns>
        public int GetColumnOverrideIndex (string columnName)
        {
            if (!TableInfo.HasColumnIndices) throw new Exception("This table has no column index overrides.");

            if (ColumnNameCache.ContainsKey(columnName))
            {
                return ColumnNameCache[columnName].Index;
            }
            throw new Exception($"There is no column with name '{columnName}'.");   
        }


        /// <summary>
        /// Sets a column's override index.
        /// </summary>
        /// <param name="index">The base column index.</param>
        /// <param name="newOverrideIndex">The new override index.</param>
        public void SetColumnOverrideIndex (int index, int newOverrideIndex)
        {
            if (!TableInfo.HasColumnIndices) throw new Exception("This table has no column index overrides.");

            try
            {
                Columns[index].Index = newOverrideIndex;
            }
            catch
            {
                throw new Exception($"There is no column with index {index}.");
            }
        }


        /// <summary>
        /// Sets a column's override index.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="newOverrideIndex">The new override index.</param>
        public void SetColumnOverrideIndex (string columnName, int newOverrideIndex)
        {
            if (!TableInfo.HasColumnIndices) throw new Exception("This table has no column index overrides.");

            try
            {
                int index = GetColumnIndex(columnName);
                Columns[index].Index = newOverrideIndex;
            }
            catch
            {
                throw new Exception($"There is no column with name '{columnName}'.");
            }
        }


        /// <summary>
        /// Gets a boolean indicating if the column is valid.
        /// </summary>
        /// <param name="columnIndex">The column index.</param>
        /// <returns>A boolean.</returns>
        public bool IsColumnValid (int columnIndex)
        {
            if (!TableInfo.HasColumnValidity) throw new Exception("This table has no column validity.");

            try
            {
                return (bool)Columns[columnIndex].IsValid;
            }
            catch
            {
                throw new Exception($"No column with index {columnIndex}");
            }
        }


        /// <summary>
        /// Gets a boolean indicating if the column is valid.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>A boolean.</returns>
        public bool IsColumnValid (string columnName)
        {
            if (!TableInfo.HasColumnValidity) throw new Exception("This table has no column validity.");

            try
            {
                return (bool)ColumnNameCache[columnName].IsValid;
            }
            catch
            {
                throw new Exception($"No column with name '{columnName}'");
            }
        }


        /// <summary>
        /// Make a column valid/invalid. All entry values of a valid column made invalid will be lost.
        /// All entry values of an invalid column made valid will be set to their defaults.
        /// </summary>
        /// <param name="column">The ArmpTableColumn.</param>
        /// <param name="isValid">The new column validity.</param>
        public void SetColumnValidity (ArmpTableColumn column, bool isValid)
        {
            if (TableInfo.HasColumnValidity)
            {
                if (isValid) //Set all values to default
                {
                    foreach(ArmpEntry entry in Entries)
                    {
                        entry.SetDefaultColumnContent(column.Name);
                    }
                    column.IsValid = true;
                }
                else //Remove all values
                {
                    foreach (ArmpEntry entry in Entries)
                    {
                        entry.RemoveColumnContent(column.Name);
                    }
                    column.IsValid = false;
                }
            }
            else
            {
                throw new Exception("This table has no column validity.");
            }
        }


        /// <summary>
        /// Make a column valid/invalid. All entry values of a valid column made invalid will be lost.
        /// All entry values of an invalid column made valid will be set to their defaults.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="isValid">The new column validity.</param>
        public void SetColumnValidity (string columnName, bool isValid)
        {
            try
            {
                ArmpTableColumn column = ColumnNameCache[columnName];
                SetColumnValidity(column, isValid);
            }
            catch 
            {
                throw new Exception($"The column '{columnName}' does not exist in this table.");
            }
        }


        /// <summary>
        /// Returns if the column is special (array/list). Only used in v2.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>A boolean</returns>
        public bool IsColumnSpecial (string columnName)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                return ColumnNameCache[columnName].IsSpecial;
            }
            throw new ColumnNotFoundException($"The column '{columnName}' does not exist in this table.");
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
        /// <param name="columnName">The column containing the value to find.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>An ArmpEntry list.</returns>
        public List<ArmpEntry> SearchByValue (string columnName, object value)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                ArmpTableColumn column = ColumnNameCache[columnName];
                value = Convert.ChangeType(value, column.ColumnType);

                List<ArmpEntry> returnList = new List<ArmpEntry>();
                foreach (ArmpEntry entry in Entries)
                {
                    if (value.Equals(entry.Data[columnName]))
                    {
                        returnList.Add(entry);
                    }
                }
                return returnList;
            }
            throw new ColumnNotFoundException($"The column '{columnName}' does not exist in this table.");
        }


        /// <summary>
        /// Creates and adds a new entry to the end of the table.
        /// </summary>
        /// <param name="name">The new entry name.</param>
        public ArmpEntry AddEntry (string name = "")
        {
            int id = Entries.Count;
            ArmpEntry entry = new ArmpEntry(this, id, name);
            entry.SetDefaultColumnContent();

            if (TableInfo.HasEntryIndices)
                entry.Index = id;

            if (TableInfo.HasEntryValidity)
                entry.IsValid = true;

            if (TableInfo.HasExtraFieldInfo && !TableInfo.IsDragonEngineV2)
                entry.Flags = new bool[8] { false, false, false, false, false, false, false, false };

            Entries.Add(entry);
            return entry;
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
        /// <param name="columnName">The column name.</param>
        /// <param name="value">The value to write.</param>
        public void SetValue (int id, string columnName, object value)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                ArmpTableColumn column = ColumnNameCache[columnName];
                if (value.GetType() == column.ColumnType)
                {
                    ArmpEntry entry = GetEntry(id);
                    entry.SetValueFromColumn(columnName, value);

                    //PLACEHOLDER PATCHER CODE
                    if (column.ColumnType != DataTypes.Types[DataTypes.ArmpType.String])
                    {
                        if (!EditedValues.ContainsKey(column.Name))
                        {
                            EditedValues.Add(column.Name, new List<int>());
                        }
                        EditedValues[column.Name].Add(id);
                    }
                }
                else
                {
                    throw new Exception($"Type mismatch. Expected {column.ColumnType} and got {value.GetType()}.");
                }
            }
            else
            {
                throw new ColumnNotFoundException($"The column '{columnName}' does not exist.");
            }
        }


        /// <summary>
        /// Sets the selected column as string type. (This is only needed for Old Engine files with text).
        /// </summary>
        /// <param name="columnName">The column name.</param>
        public void SetTextColumnOE (string columnName)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                ArmpTableColumn column = ColumnNameCache[columnName];
                column.ColumnType = typeof(string);
                foreach (ArmpEntry entry in Entries)
                {
                    Int16 textIndex = (Int16)entry.Data[columnName];
                    if (textIndex != -1) entry.Data[columnName] = Text[textIndex];
                    else entry.Data[columnName] = null;
                }
            }
            throw new ColumnNotFoundException($"The column '{columnName}' does not exist.");
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

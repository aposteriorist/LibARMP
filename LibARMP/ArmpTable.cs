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
        /// <exception cref="EntryNotFoundException">The table has no entries.</exception>
        public List<ArmpEntry> GetAllEntries()
        {
            try
            {
                return Entries;
            }
            catch
            {
                throw new EntryNotFoundException();
            }
        }


        /// <summary>
        /// Returns a specific entry in the table.
        /// </summary>
        /// <param name="id">The entry ID.</param>
        /// <returns>An ArmpEntry.</returns>
        /// <exception cref="EntryNotFoundException">The table has no entry with the specified ID.</exception>
        public ArmpEntry GetEntry (int id)
        {
            try
            {
                return Entries[id];
            }
            catch
            {
                throw new EntryNotFoundException(id);
            }
        }


        /// <summary>
        /// Returns a specific entry in the table.
        /// </summary>
        /// <param name="name">The entry name.</param>
        /// <returns>An ArmpEntry.</returns>
        /// <exception cref="EntryNotFoundException">The table has no entry with the specified name.</exception>
        public ArmpEntry GetEntry (string name)
        {
            try
            {
                foreach (ArmpEntry entry in Entries)
                {
                    if (entry.Name == name) return entry;
                }
                throw new EntryNotFoundException(name);
            }
            catch
            {
                throw new EntryNotFoundException(name);
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
        /// <exception cref="EntryNameNotFoundException">The table has no entry names.</exception>
        public List<string> GetEntryNames()
        {
            if (!TableInfo.HasEntryNames)
                throw new EntryNameNotFoundException();

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
        /// <exception cref="EntryNameNotFoundException">The table has no entry names.</exception>
        /// <exception cref="EntryNotFoundException">The table has no entry with the specified ID.</exception>
        public string GetEntryName (int id)
        {
            if (!TableInfo.HasEntryNames)
                throw new EntryNameNotFoundException();

            try
            {
                return Entries[id].Name;
            }
            catch
            {
                throw new EntryNotFoundException(id);
            }
        }


        /// <summary>
        /// Gets a specific column in the table.
        /// </summary>
        /// <param name="id">The column ID.</param>
        /// <returns>An ArmpTableColumn.</returns>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified ID.</exception>
        public ArmpTableColumn GetColumn (int id)
        {
            try
            {
                return Columns[id];
            }
            catch
            {
                throw new ColumnNotFoundException(id);
            }
        }


        /// <summary>
        /// Gets a specific column in the table.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>An ArmpTableColumn.</returns>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public ArmpTableColumn GetColumn (string columnName)
        {
            try
            {
                return ColumnNameCache[columnName];
            }
            catch
            {
                throw new ColumnNotFoundException(columnName);
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
        /// <param name="id">The column ID.</param>
        /// <returns>A string.</returns>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified ID.</exception>
        public string GetColumnName (int id)
        {
            try
            {
                return Columns[id].Name;
            }
            catch
            {
                throw new ColumnNotFoundException(id);
            }
        }


        /// <summary>
        /// Gets the column's data type.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>The column Type.</returns>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public Type GetColumnDataType (string columnName)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                return ColumnNameCache[columnName].Type.CSType;
            }
            throw new ColumnNotFoundException(columnName);   
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
                if (column.Type.CSType == type)
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
                if (column.Type.CSType == type) returnList.Add(column.ID);
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
        /// Gets the column ID by name.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>The column ID.</returns>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public int GetColumnID (string columnName)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                return ColumnNameCache[columnName].ID;
            }
            throw new ColumnNotFoundException(columnName);
        }


        /// <summary>
        /// Gets a column's index.
        /// </summary>
        /// <param name="id">The column ID.</param>
        /// <returns>The column index.</returns>
        /// <exception cref="ColumnNoIndexException">The table has no column indices.</exception>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified ID.</exception>
        public int GetColumnIndex (int id)
        {
            if (!TableInfo.HasColumnIndices) throw new ColumnNoIndexException();

            try
            {
                return Columns[id].Index;
            }
            catch
            {
                throw new ColumnNotFoundException(id);
            }
        }


        /// <summary>
        /// Gets a column's index.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>The column index.</returns>
        /// <exception cref="ColumnNoIndexException">The table has no column indices.</exception>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public int GetColumnIndex (string columnName)
        {
            if (!TableInfo.HasColumnIndices) throw new ColumnNoIndexException();

            if (ColumnNameCache.ContainsKey(columnName))
            {
                return ColumnNameCache[columnName].Index;
            }
            throw new ColumnNotFoundException(columnName);
        }


        /// <summary>
        /// Sets a column's index.
        /// </summary>
        /// <param name="id">The column ID.</param>
        /// <param name="newIndex">The new index.</param>
        /// <exception cref="ColumnNoIndexException">The table has no column indices.</exception>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified ID.</exception>
        public void SetColumnIndex (int id, int newIndex)
        {
            if (!TableInfo.HasColumnIndices) throw new ColumnNoIndexException();

            try
            {
                Columns[id].Index = newIndex;
            }
            catch
            {
                throw new ColumnNotFoundException(id);
            }
        }


        /// <summary>
        /// Sets a column's index.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="newIndex">The new index.</param>
        /// <exception cref="ColumnNoIndexException">The table has no column indices.</exception>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public void SetColumnIndex (string columnName, int newIndex)
        {
            if (!TableInfo.HasColumnIndices) throw new ColumnNoIndexException();

            try
            {
                int index = GetColumnID(columnName);
                Columns[index].Index = newIndex;
            }
            catch
            {
                throw new ColumnNotFoundException(columnName);
            }
        }


        /// <summary>
        /// Gets a boolean indicating if the column is valid.
        /// </summary>
        /// <param name="id">The column ID.</param>
        /// <returns>A boolean.</returns>
        /// <exception cref="ColumnNoValidityException">The table has no column validity.</exception>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified ID.</exception>
        public bool IsColumnValid (int id)
        {
            if (!TableInfo.HasColumnValidity) throw new ColumnNoValidityException();

            try
            {
                return (bool)Columns[id].IsValid;
            }
            catch
            {
                throw new ColumnNotFoundException(id);
            }
        }


        /// <summary>
        /// Gets a boolean indicating if the column is valid.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>A boolean.</returns>
        /// <exception cref="ColumnNoValidityException">The table has no column validity.</exception>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public bool IsColumnValid (string columnName)
        {
            if (!TableInfo.HasColumnValidity) throw new ColumnNoValidityException();

            try
            {
                return (bool)ColumnNameCache[columnName].IsValid;
            }
            catch
            {
                throw new ColumnNotFoundException(columnName);
            }
        }


        /// <summary>
        /// Make a column valid/invalid. All entry values of a valid column made invalid will be lost.
        /// All entry values of an invalid column made valid will be set to their defaults.
        /// </summary>
        /// <param name="column">The ArmpTableColumn.</param>
        /// <param name="isValid">The new column validity.</param>
        /// <exception cref="ColumnNoValidityException">The table has no column validity.</exception>
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
                throw new ColumnNoValidityException();
            }
        }


        /// <summary>
        /// Make a column valid/invalid. All entry values of a valid column made invalid will be lost.
        /// All entry values of an invalid column made valid will be set to their defaults.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="isValid">The new column validity.</param>
        /// <exception cref="ColumnNoValidityException">The table has no column validity.</exception>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public void SetColumnValidity (string columnName, bool isValid)
        {
            try
            {
                ArmpTableColumn column = ColumnNameCache[columnName];
                SetColumnValidity(column, isValid);
            }
            catch (ColumnNoValidityException ex)
            {
                throw ex;
            }
            catch
            {
                throw new ColumnNotFoundException(columnName);
            }
        }


        /// <summary>
        /// Returns if the column is special (array/list). Only used in v2.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>A boolean</returns>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public bool IsColumnSpecial (string columnName)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                return ColumnNameCache[columnName].IsSpecial;
            }
            throw new ColumnNotFoundException(columnName);
        }


        /// <summary>
        /// Sets the column's value to default for all entries.
        /// </summary>
        /// <param name="column">The ArmpTableColumn.</param>
        internal void SetDefaultColumnContent (ArmpTableColumn column)
        {
            foreach (ArmpEntry entry in Entries)
            {
                entry.SetDefaultColumnContent(column);
            }
        }


        /// <summary>
        /// Adds a new column to the table.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnType">The column type.</param>
        /// <returns>The newly created column.</returns>
        /// <exception cref="TypeNotSupportedException">The provided C# type is not supported by the armp format.</exception>
        public ArmpTableColumn AddColumn (string columnName, Type columnType)
        {
            int id = Columns.Count;
            ArmpType armpType = DataTypes.GetArmpTypeByCSType(columnType);
            ArmpTableColumn column = new ArmpTableColumn(id, columnName, armpType);
            if (TableInfo.HasColumnIndices) column.Index = id;
            if (TableInfo.HasColumnValidity) column.IsValid = true;
            column.IsNoData = false;

            if (TableInfo.FormatVersion == Version.DragonEngineV2)
            {
                //If the type is special
                if (DataTypes.SpecialTypes.Contains(armpType.CSType))
                {
                    column.IsSpecial = true;
                    column.SpecialSize = 0; //currently empty
                    column.Children = new List<ArmpTableColumn>();
                }

                //Check if the name matches an existing special column and assign parent/child
                //NOTE: unsure if allowing the creation of a column with child naming but no parent will cause problems down the line.
                if (columnName.Contains("[") && columnName.Contains("]"))
                {
                    string baseName = columnName.Split('[')[0];
                    if (ColumnNameCache.ContainsKey(baseName))
                    {
                        ArmpTableColumn parentColumn = ColumnNameCache[baseName];
                        parentColumn.Children.Add(column);
                        parentColumn.SpecialSize += 1;
                        column.Parent = parentColumn;
                    }
                }
            }

            Columns.Add(column);
            RefreshColumnNameCache();
            SetDefaultColumnContent(column);
            return column;
        }


        /// <summary>
        /// Adds a new column to the table.
        /// </summary>
        /// <typeparam name="T">The column type.</typeparam>
        /// <param name="columnName">The column name.</param>
        /// <returns>The newly created column.</returns>
        /// <exception cref="TypeNotSupportedException">The provided C# type is not supported by the armp format.</exception>
        public ArmpTableColumn AddColumn<T> (string columnName)
        {
            return AddColumn(columnName, typeof(T));
        }


        /// <summary>
        /// Deletes the specified column from the table.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <returns>A boolean indicating if the operation completed successfully.</returns>
        public bool DeleteColumn (string columnName)
        {
            //TODO: Deleting a column will break how the game reads subsequent columns. Indices may need to be updated. Make it an optional argument?
            if (ColumnNameCache.ContainsKey(columnName))
            {
                ArmpTableColumn column = ColumnNameCache[columnName];

                //Remove column from parent and children
                if (column.Parent != null)
                {
                    column.Parent.Children.Remove(column);
                    column.Parent.SpecialSize -= 1;
                    column.Parent = null;
                }

                foreach (ArmpTableColumn child in column.Children)
                {
                    child.Parent = null;
                }

                Columns.Remove(column);
                RefreshColumnNameCache();
                return true;
            }
            return false;
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
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public List<ArmpEntry> SearchByValue (string columnName, object value)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                ArmpTableColumn column = ColumnNameCache[columnName];
                value = Convert.ChangeType(value, column.Type.CSType);

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
            throw new ColumnNotFoundException(columnName);
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
            Entries.Add(entry);
            return entry;
        }


        /// <summary>
        /// Creates and inserts a new entry at the specified ID.
        /// </summary>
        /// <param name="id">The new entry ID.</param>
        /// <param name="name">The new entry name.</param>
        /// <exception cref="EntryInsertException">The specified ID is greater than the amount of entries in the table.</exception>
        public ArmpEntry InsertEntry (int id, string name = "")
        {
            if (id <= Entries.Count)
            {
                ArmpEntry entry = new ArmpEntry(this, id, name);
                entry.SetDefaultColumnContent();
                Entries.Insert(id, entry);

                if (Entries.Count > id)
                {
                    foreach (ArmpEntry e in Entries.GetRange(id + 1, Entries.Count - id - 1))
                    {
                        e.ID++;
                    }
                }
                return entry;
            }
            else
            {
                throw new EntryInsertException(id);
            }
        }


        /// <summary>
        /// Copy a specified entry.
        /// </summary>
        /// <param name="id">The entry to copy.</param>
        public ArmpEntry CopyEntry (int id) //TODO rewrite this to copy from one table to another
        {
            ArmpEntry entry;
            entry = GetEntry(id);
            return Util.DeepCopy<ArmpEntry>(entry);
        }


        /// <summary>
        /// Deletes the specified entry and updates the IDs for any entries after it.
        /// </summary>
        /// <param name="id">The ID of the entry to delete.</param>
        /// <exception cref="EntryNotFoundException">The table has no entry with the specified ID.</exception>
        public void DeleteEntry (int id)
        {
            if (id < Entries.Count)
            {
                Entries.RemoveAt(id);
                foreach (ArmpEntry entry in Entries.GetRange(id, Entries.Count - id))
                {
                    entry.ID--;
                }
            }
            else
            {
                throw new EntryNotFoundException(id);
            }
        }


        /// <summary>
        /// Deletes the specified entry and updates the IDs for any entries after it.
        /// </summary>
        /// <param name="name">The name of the entry to delete.</param>
        /// <exception cref="EntryNotFoundException">The table has no entry with the specified name.</exception>
        public void DeleteEntry (string name)
        {
            try
            {
                ArmpEntry entry = GetEntry(name);
                DeleteEntry(entry.ID);
            }
            catch
            {
                throw new EntryNotFoundException(name);
            }
        }


        /// <summary>
        /// Sets the value of a specified column in the specified entry.
        /// </summary>
        /// <param name="id">The entry to modify.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="TypeMismatchException">The column type does not match the type of the provided object.</exception>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public void SetValue (int id, string columnName, object value)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                ArmpTableColumn column = ColumnNameCache[columnName];
                if (value.GetType() == column.Type.CSType)
                {
                    ArmpEntry entry = GetEntry(id);
                    entry.SetValueFromColumn(columnName, value);

                    //PLACEHOLDER PATCHER CODE
                    if (column.Type.CSType != typeof(string))
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
                    throw new TypeMismatchException(column.Type.CSType, value.GetType());
                }
            }
            else
            {
                throw new ColumnNotFoundException(columnName);
            }
        }


        /// <summary>
        /// Sets the selected column as string type. (This is only needed for Old Engine files with text).
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <exception cref="ColumnNotFoundException">The table has no column with the specified name.</exception>
        public void SetTextColumnOE (string columnName)
        {
            if (ColumnNameCache.ContainsKey(columnName))
            {
                ArmpTableColumn column = ColumnNameCache[columnName];
                column.Type.CSType = typeof(string);
                foreach (ArmpEntry entry in Entries)
                {
                    Int16 textIndex = (Int16)entry.Data[columnName];
                    if (textIndex != -1) entry.Data[columnName] = Text[textIndex];
                    else entry.Data[columnName] = null;
                }
            }
            throw new ColumnNotFoundException(columnName);
        }
    }
}

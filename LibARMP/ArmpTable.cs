using LibARMP.Exceptions;
using System;
using System.Collections.Generic;

namespace LibARMP
{
    [Serializable]
    public class ArmpTable
    {
        public ArmpTable()
        {
            Entries = new List<ArmpEntry>();
            ColumnDataTypesAuxTable = new List<List<int>>(); //v2 only
        }

        /// <summary>
        /// Pointers, flags and general information.
        /// </summary>
        public ArmpTableInfo TableInfo { get; set; }

        /// <summary>
        /// Entry names.
        /// </summary>
        public List<string> RowNames { get; set; }

        /// <summary>
        /// Column names.
        /// </summary>
        public List<string> ColumnNames { get; set; }

        /// <summary>
        /// Text.
        /// </summary>
        public List<string> Text { get; set; }

        /// <summary>
        /// Row validity.
        /// </summary>
        public List<bool> RowValidity { get; set; }

        /// <summary>
        /// Column validity.
        /// </summary>
        public List<bool> ColumnValidity { get; set; }

        /// <summary>
        /// Row indices.
        /// </summary>
        public List<int> RowIndices { get; set; }

        /// <summary>
        /// Column indices.
        /// </summary>
        public List<int> ColumnIndices { get; set; }

        /// <summary>
        /// Column data types.
        /// </summary>
        public List<Type> ColumnDataTypes { get; set; }

        /// <summary>
        /// Column data types (auxiliary). Only used in storage mode 0.
        /// </summary>
        public List<Type> ColumnDataTypesAux { get; set; }

        /// <summary>
        /// Column data types (auxiliary). Only used in storage mode 1. [Type ID, Distance, Array Size, Unknown]
        /// </summary>
        public List<List<int>> ColumnDataTypesAuxTable { get; set; }

        /// <summary>
        /// List of booleans indicating if the column with the same index is special (arrays/lists).
        /// </summary>
        public List<bool> SpecialColumns { get; set; }

        /// <summary>
        /// List of ints used as column metadata. (OLD ENGINE ONLY)
        /// </summary>
        public List<int> ColumnMetadata { get; set; }

        /// <summary>
        /// List of entries.
        /// </summary>
        public List<ArmpEntry> Entries { get; set; }




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
            catch (Exception e)
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
            catch (Exception e) 
            {
                throw new EntryNotFoundException("No entry with ID '"+id+"'.");
            }
        }


        /// <summary>
        /// Gets the column names.
        /// </summary>
        /// <param name="includeSpecials">Include special columns? (Array data types)</param>
        /// <returns>A string list.</returns>
        public List<string> GetColumnNames(bool includeSpecials)
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
        /// Gets the columns of the selected type.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>A string list.</returns>
        public List<string> GetColumnNamesByType<T> ()
        {
            List<string> returnList = new List<string>();
            for (int i = 0; i < ColumnNames.Count; i++)
            {
                if (GetColumnDataType(ColumnNames[i]) == typeof(T))
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
        public Type GetColumnDataType (string column)
        {
            List<Type> dataTypes = ColumnDataTypesAux; //Default for DE v1
            if (TableInfo.IsOldEngine || TableInfo.IsIshin || TableInfo.IsDragonEngineV2) dataTypes = ColumnDataTypes;
            if (!ColumnNames.Contains(column)) throw new ColumnNotFoundException("The column '" + column + "' does not exist in this table.");
            int columnIndex = ColumnNames.IndexOf(column);
            return dataTypes[columnIndex];
        }


        /// <summary>
        /// Returns if the column is special (array/list). Only used in v2.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>A boolean</returns>
        public bool IsColumnSpecial (string column)
        {
            if (!ColumnNames.Contains(column)) throw new ColumnNotFoundException("The column '" + column + "' does not exist in this table.");
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
        /// Searches all entries for matching vales in the specified column.
        /// </summary>
        /// <param name="column">The column containing the value to find.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>An ArmpEntry list.</returns>
        public List<ArmpEntry> SearchByValue (string column, object value)
        {
            if (!ColumnNames.Contains(column)) throw new ColumnNotFoundException("The column '"+column+"' does not exist in this table.");
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
        /// <param name="id">The entry to copy</param>
        public ArmpEntry CopyEntry (int id)
        {
            ArmpEntry entry;
            entry = GetEntry(id);
            return Util.DeepCopy<ArmpEntry>(entry);
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
                    throw new Exception("Type mismatch. Expected '"+ dataTypes[columnIndex] + "' and got '"+value.GetType()+"'.");
                }
            }
            else
            {
                throw new ColumnNotFoundException("The column '" + column + "' does not exist.");
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
                throw new ColumnNotFoundException("The column '" + column + "' does not exist.");
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

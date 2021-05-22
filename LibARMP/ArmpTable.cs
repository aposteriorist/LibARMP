using System;
using System.Collections.Generic;
using System.Text;

namespace LibARMP
{
    public class ArmpTable
    {
        public ArmpTable()
        {

        }

        /// <summary>
        /// Entry names.
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
        /// Column data types (auxiliary).
        /// </summary>
        public List<Type> ColumnDataTypesAux { get; set; }
    }
}

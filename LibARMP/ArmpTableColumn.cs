using System;
using System.Collections.Generic;

namespace LibARMP
{
    [Serializable]
    public class ArmpTableColumn
    {
        internal ArmpTableColumn(int id)
        {
            this.ID = id;
            Children = new List<ArmpTableColumn>();
        }

        internal ArmpTableColumn(int id, string name, Type type) : this(id)
        {
            this.Name = name;
            this.ColumnType = type;
        }

        /// <summary>
        /// Gets or sets the column ID.
        /// </summary>
        public int ID { get; internal set; }

        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets or sets the column type.
        /// </summary>
        public Type ColumnType { get; internal set; }

        /// <summary>
        /// Gets or sets the auxiliary column type.
        /// </summary>
        internal Type ColumnTypeAux { get; set; }

        /// <summary>
        /// Gets or sets the column index. Can be NULL
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets if the column is valid. Can be NULL
        /// </summary>
        public bool? IsValid { get; set; }

        /// <summary>
        /// Gets or sets if the column contains no data despite being valid.
        /// </summary>
        internal bool IsNoData { get; set; }

        /// <summary>
        /// Gets or sets if the column is special.
        /// </summary>
        public bool IsSpecial { get; internal set; }

        /// <summary>
        /// Amount of elements in the array if the column is special.
        /// </summary>
        internal int SpecialSize { get; set; }

        /// <summary>
        /// Distance between data if the table uses StorageMode 1.
        /// </summary>
        internal int Distance { get; set; }

        /// <summary>
        /// Gets or sets the unknown metadata. TODO
        /// </summary>
        public int UnknownMetadata0x40 { get; set; }

        /// <summary>
        /// Gets or sets the unknown metadata. TODO
        /// </summary>
        public int UnknownMetadata0x4C { get; set; }

        /// <summary>
        /// Gets or sets the column's children. Only used if the column is special. (v2)
        /// </summary>
        internal List<ArmpTableColumn> Children { get; set; }

        /// <summary>
        /// Gets or sets the column's parent. Only used if the column is child of a special. (v2)
        /// </summary>
        internal ArmpTableColumn Parent { get; set; }
    }
}

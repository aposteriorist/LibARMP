using System;

namespace LibARMP
{
    public class ArmpTableInfo
    {

        public ArmpTableInfo ()
        {
            this.RowCount = 0;
            this.ColumnCount = 0;
            this.TextCount = 0;
            this.RowValidator = 0;
            this.ColumnValidator = 0;
            this.TableID = 0;
            this.StorageMode = 0;
            this.HasSubTable = false;
            this.ptrRowNamesOffsetTable = 0;
            this.ptrRowValidity = 0;
            this.ptrColumnDataTypes = 0;
            this.ptrColumnContentOffsetTable = 0;
            this.ptrTextOffsetTable = 0;
            this.ptrColumnNamesOffsetTable = 0;
            this.ptrRowIndices = 0;
            this.ptrColumnIndices = 0;
            this.ptrColumnValidity = 0;
            this.ptrSubTable = 0;
            this.ptrEmptyValuesOffsetTable = 0;
            this.ptrColumnDataTypesAux = 0;
            this.ptrFieldInfo = 0;
            this.ptrFieldID = 0; //TODO verify
        }

        /// <summary>
        /// Gets or sets the number of rows.
        /// </summary>
        public Int32 RowCount { get; set; }

        /// <summary>
        /// Gets or sets the number of columns.
        /// </summary>
        public Int32 ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the number of strings (field values).
        /// </summary>
        public Int32 TextCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the validity of rows.
        /// </summary>
        public Int32 RowValidator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the validity of columns.
        /// </summary>
        public Int32 ColumnValidator { get; set; }

        /// <summary>
        /// Gets or sets the table ID (Int24).
        /// </summary>
        public Int32 TableID { get; set; }

        /// <summary>
        /// Gets or sets the storage Mode (only used in version 2).
        /// </summary>
        public byte StorageMode { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has a SubTable.
        /// </summary>
        public bool HasSubTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the String Offset Table.
        /// </summary>
        public Int32 ptrRowNamesOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Row Validity bitmask.
        /// </summary>
        public Int32 ptrRowValidity { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Data Types.
        /// </summary>
        public Int32 ptrColumnDataTypes { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Content Offset Table.
        /// </summary>
        public Int32 ptrColumnContentOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Text Offset Table.
        /// </summary>
        public Int32 ptrTextOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Names Offset Table.
        /// </summary>
        public Int32 ptrColumnNamesOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Row Indices int array.
        /// </summary>
        public Int32 ptrRowIndices { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Indices int array.
        /// </summary>
        public Int32 ptrColumnIndices { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Validity bitmask.
        /// </summary>
        public Int32 ptrColumnValidity { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the SubTable.
        /// </summary>
        public Int32 ptrSubTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Empty Values Offset Table.
        /// </summary>
        public Int32 ptrEmptyValuesOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Auxiliary Column Data Type Table.
        /// </summary>
        public Int32 ptrColumnDataTypesAux { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the additional Field Info.
        /// </summary>
        public Int32 ptrFieldInfo { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Field ID list.
        /// </summary>
        public Int32 ptrFieldID { get; set; }

    }
}

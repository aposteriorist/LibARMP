using System;

namespace LibARMP
{
    class ArmpTableInfo
    {

        public ArmpTableInfo ()
        {
            this.rowCount = 0;
            this.columnCount = 0;
            this.textCount = 0;
            this.rowValidator = 0;
            this.columnValidator = 0;
            this.tableID = 0;
            this.storageMode = 0;
            this.hasSubTable = false;
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
        public Int32 rowCount { get; set; }

        /// <summary>
        /// Gets or sets the number of columns.
        /// </summary>
        public Int32 columnCount { get; set; }

        /// <summary>
        /// Gets or sets the number of strings (field values).
        /// </summary>
        public Int32 textCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the validity of rows.
        /// </summary>
        public Int32 rowValidator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the validity of columns.
        /// </summary>
        public Int32 columnValidator { get; set; }

        /// <summary>
        /// Gets or sets the table ID (Int24).
        /// </summary>
        public Int32 tableID { get; set; }

        /// <summary>
        /// Gets or sets the storage Mode.
        /// </summary>
        public byte storageMode { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has a SubTable.
        /// </summary>
        public bool hasSubTable { get; set; }

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

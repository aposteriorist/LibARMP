using System;

namespace LibARMP
{
    [Serializable]
    public class ArmpTableInfo
    {

        public ArmpTableInfo ()
        {
            //General Info
            this.RowCount = 0;
            this.ColumnCount = 0;
            this.TextCount = 0;
            this.RowValidator = 0;
            this.ColumnValidator = 0;
            this.TableID = 0;
            this.StorageMode = 0;

            //Pointers
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
            this.ptrExtraFieldInfo = 0;
            this.ptrColumnMetadata = 0;

            //Flags
            this.HasText = false;
            this.HasSubTable = false;
            this.HasRowNames = false;
            this.HasColumnNames = false;
            this.HasColumnDataTypesAux = false;
            this.HasRowValidity = false;
            this.HasColumnValidity = false;
            this.HasRowIndices = false;
            this.HasColumnIndices = false;
            this.HasExtraFieldInfo = false;

            //Extra data
            this.IsOldEngine = false;
            this.IsIshin = false;
            this.IsDragonEngineV2 = false;
        }


        //General Info

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
        /// Gets or sets a value indicating the validity of rows. (DRAGON ENGINE ONLY)
        /// </summary>
        public Int32 RowValidator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the validity of columns. (DRAGON ENGINE ONLY)
        /// </summary>
        public Int32 ColumnValidator { get; set; }

        /// <summary>
        /// Gets or sets the table ID (Int24). (DRAGON ENGINE ONLY)
        /// </summary>
        public Int32 TableID { get; set; }

        /// <summary>
        /// Gets or sets the storage Mode (only used in version 2). (DRAGON ENGINE ONLY)
        /// </summary>
        public byte StorageMode { get; set; }




        //Pointers

        /// <summary>
        /// Gets or sets the pointer to the String Offset Table.
        /// </summary>
        internal UInt32 ptrRowNamesOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Row Validity bitmask.
        /// </summary>
        internal UInt32 ptrRowValidity { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Data Types.
        /// </summary>
        internal UInt32 ptrColumnDataTypes { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Content Offset Table.
        /// </summary>
        internal UInt32 ptrColumnContentOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Text Offset Table.
        /// </summary>
        internal UInt32 ptrTextOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Names Offset Table.
        /// </summary>
        internal UInt32 ptrColumnNamesOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Row Indices int array. (DRAGON ENGINE ONLY)
        /// </summary>
        internal UInt32 ptrRowIndices { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Indices int array. (DRAGON ENGINE ONLY)
        /// </summary>
        internal UInt32 ptrColumnIndices { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Validity bitmask. (DRAGON ENGINE ONLY)
        /// </summary>
        internal UInt32 ptrColumnValidity { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the SubTable. (DRAGON ENGINE ONLY)
        /// </summary>
        internal UInt32 ptrSubTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Empty Values Offset Table. (DRAGON ENGINE ONLY)
        /// </summary>
        internal UInt32 ptrEmptyValuesOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Auxiliary Column Data Type Table. (DRAGON ENGINE ONLY)
        /// </summary>
        internal UInt32 ptrColumnDataTypesAux { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the additional Field Info. (DRAGON ENGINE ONLY)
        /// </summary>
        internal UInt32 ptrExtraFieldInfo { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Column Metadata.
        /// </summary>
        internal UInt32 ptrColumnMetadata { get; set; }



        //Flags

        /// <summary>
        /// Gets or sets the boolean indicating if the table has text.
        /// </summary>
        public bool HasText { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has a SubTable. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasSubTable { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has row names.
        /// </summary>
        public bool HasRowNames { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has column names.
        /// </summary>
        public bool HasColumnNames { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has an auxiliary column data types list. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasColumnDataTypesAux { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has validity flags for rows. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasRowValidity { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has validity flags for columns. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasColumnValidity { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has row indices. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasRowIndices { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has column indices. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasColumnIndices { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has an Empty Values Table. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasEmptyValues { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has additional field info (varies between format versions). (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasExtraFieldInfo { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has column metadata.
        /// </summary>
        public bool HasColumnMetadata { get; set; }



        //Extra Data

        /// <summary>
        /// Gets or sets the boolean indicating if the table uses the Old Engine (0-K1-FOTNS) version.
        /// </summary>
        public bool IsOldEngine { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table uses the Old Engine (Ishin) version.
        /// </summary>
        public bool IsIshin { get; set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table uses the Dragon Engine version 2.
        /// </summary>
        public bool IsDragonEngineV2 { get; set; }
    }
}

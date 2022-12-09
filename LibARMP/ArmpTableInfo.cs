using System;

namespace LibARMP
{
    [Serializable]
    public class ArmpTableInfo
    {

        internal ArmpTableInfo ()
        {
            //General Info
            this.EntryCount = 0;
            this.ColumnCount = 0;
            this.TextCount = 0;
            this.EntryValidator = 0;
            this.ColumnValidator = 0;
            this.TableID = 0;
            this.StorageMode = 0;

            //Pointers
            this.ptrEntryNamesOffsetTable = 0;
            this.ptrEntryValidity = 0;
            this.ptrColumnDataTypes = 0;
            this.ptrColumnContentOffsetTable = 0;
            this.ptrTextOffsetTable = 0;
            this.ptrColumnNamesOffsetTable = 0;
            this.ptrEntryIndices = 0;
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
            this.HasEntryNames = false;
            this.HasColumnNames = false;
            this.HasColumnDataTypesAux = false;
            this.HasEntryValidity = false;
            this.HasColumnValidity = false;
            this.HasEntryIndices = false;
            this.HasColumnIndices = false;
            this.HasExtraFieldInfo = false;

            //Extra data
            this.IsOldEngine = false;
            this.IsIshin = false;
            this.IsDragonEngineV2 = false;
        }


        //General Info

        /// <summary>
        /// Gets the number of entries.
        /// </summary>
        public Int32 EntryCount { get; internal set; }

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        public Int32 ColumnCount { get; internal set; }

        /// <summary>
        /// Gets the number of strings.
        /// </summary>
        public Int32 TextCount { get; internal set; }

        /// <summary>
        /// Gets a value indicating the validity of entries. (DRAGON ENGINE ONLY)
        /// </summary>
        public Int32 EntryValidator { get; internal set; }

        /// <summary>
        /// Gets a value indicating the validity of columns. (DRAGON ENGINE ONLY)
        /// </summary>
        public Int32 ColumnValidator { get; internal set; }

        /// <summary>
        /// Gets the table ID (Int24). (DRAGON ENGINE ONLY)
        /// </summary>
        public Int32 TableID { get; internal set; }

        /// <summary>
        /// Gets the storage Mode (only used in version 2). (DRAGON ENGINE ONLY)
        /// </summary>
        public byte StorageMode { get; internal set; }




        //Pointers

        /// <summary>
        /// Gets or sets the pointer to the Main Table. DEBUG
        /// </summary>
        internal UInt32 ptrMainTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the String Offset Table.
        /// </summary>
        internal UInt32 ptrEntryNamesOffsetTable { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the Entry Validity bitmask.
        /// </summary>
        internal UInt32 ptrEntryValidity { get; set; }

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
        /// Gets or sets the pointer to the Entry Indices int array. (DRAGON ENGINE ONLY)
        /// </summary>
        internal UInt32 ptrEntryIndices { get; set; }

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
        /// Gets a boolean indicating if the table has text.
        /// </summary>
        public bool HasText { get; internal set; }

        /// <summary>
        /// Gets a boolean indicating if the table has a SubTable. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasSubTable { get; internal set; }

        /// <summary>
        /// Gets a boolean indicating if the table has entry names.
        /// </summary>
        public bool HasEntryNames { get; internal set; }

        /// <summary>
        /// Gets a boolean indicating if the table has column names.
        /// </summary>
        public bool HasColumnNames { get; internal set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has an auxiliary column data types list. (DRAGON ENGINE ONLY)
        /// </summary>
        internal bool HasColumnDataTypesAux { get; set; }

        /// <summary>
        /// Gets a boolean indicating if the table has validity flags for entries. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasEntryValidity { get; internal set; }

        /// <summary>
        /// Gets a boolean indicating if the table has validity flags for columns. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasColumnValidity { get; internal set; }

        /// <summary>
        /// Gets a boolean indicating if the table has entry indices. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasEntryIndices { get; internal set; }

        /// <summary>
        /// Gets a boolean indicating if the table has column indices. (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasColumnIndices { get; internal set; }

        /// <summary>
        /// Gets or sets the boolean indicating if the table has an Empty Values Table. (DRAGON ENGINE ONLY)
        /// </summary>
        internal bool HasEmptyValues { get; set; }

        /// <summary>
        /// Gets a boolean indicating if the table has additional field info (varies between format versions). (DRAGON ENGINE ONLY)
        /// </summary>
        public bool HasExtraFieldInfo { get; internal set; }

        /// <summary>
        /// Gets a boolean indicating if the table has column metadata.
        /// </summary>
        public bool HasColumnMetadata { get; internal set; }



        //Version Data

        /// <summary>
        /// Gets a boolean indicating if the table uses the Old Engine (0-K1-FOTNS) version.
        /// </summary>
        public bool IsOldEngine { get; internal set; }

        /// <summary>
        /// Gets a boolean indicating if the table uses the Old Engine (Ishin) version.
        /// </summary>
        public bool IsIshin { get; internal set; }

        /// <summary>
        /// Gets a boolean indicating if the table uses the Dragon Engine version 2.
        /// </summary>
        public bool IsDragonEngineV2 { get; internal set; }
    }
}

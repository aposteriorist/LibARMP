namespace LibARMP
{
    /// <summary>
    /// How column pointer tables are used to shortcut data storage (perhaps only boolean).
    /// </summary>
    /// <remarks><para></para>Only relevant for StorageMode.Column.</para></remarks>
    public enum ColumnShortcutType
    {
        /// <summary>
        /// All rows shortcut to true in this column.
        /// </summary>
        /// <remarks><para></para>For boolean type only.</para></remarks>
        BoolAllTrue = -1,

        /// <summary>
        /// All rows shortcut to false in this column.
        /// </summary>
        /// <remarks><para></para>For boolean type only.</para></remarks>
        BoolAllFalse = 0,

        /// <summary>
        /// The column does not shortcut and has normal value retrieval via pointer.
        /// </summary>
        None = 1,
    }
}

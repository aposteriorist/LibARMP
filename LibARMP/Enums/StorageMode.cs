namespace LibARMP
{
    /// <summary>
    /// How the data is stored.
    /// </summary>
    public enum StorageMode
    {
        /// <summary>
        /// Data is stored on a per column basis.
        /// </summary>
        Column,

        /// <summary>
        /// Data is stored on a per entry basis.
        /// </summary>
        /// <remarks><b>DRAGON ENGINE V2 ONLY</b></remarks>
        Entry,
    }
}

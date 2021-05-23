using System;
using System.Collections.Generic;
using Yarhl.FileFormat;
using Yarhl.IO;

namespace LibARMP
{
    public class ArmpEntry
    {

        public ArmpEntry()
        {

        }

        public ArmpEntry(int id, string name, int index)
        {
            ID = id;
            Name = name;
            Index = index;
            Data = new Dictionary<string, object>();
        }


        /// <summary>
        /// Gets or sets the entry ID.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the entry name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Data per column. (column, value)
        /// </summary>
        public IDictionary<string, object> Data { get; set; }

        /// <summary>
        /// Gets or sets the entry index.
        /// </summary>
        public int Index { get; set; }
    }
}

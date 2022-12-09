using System;
using System.IO;

namespace LibARMP
{
    public class ARMP
    {

        internal ARMP ()
        {

        }


        /// <summary>
        /// Gets the version number.
        /// </summary>
        public Int16 Version { get; internal set; }

        /// <summary>
        /// Gets the revision number.
        /// </summary>
        public Int16 Revision { get; internal set; }

        /// <summary>
        /// Is it using the Old Engine version of the format? (it shares the same version and revision numbers with Dragon Engine v1).
        /// </summary>
        public bool IsOldEngine { get; internal set; }

        /// <summary>
        /// Gets the main table.
        /// </summary>
        public ArmpTableMain MainTable { get; internal set; }

        /// <summary>
        /// Original armp file for patching.
        /// </summary>
        internal MemoryStream File = new MemoryStream();
    }
}

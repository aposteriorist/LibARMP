using System;
using System.Collections.Generic;

namespace LibARMP
{
    public class ARMP
    {

        public ARMP ()
        {

        }


        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        public Int16 Version { get; set; }

        /// <summary>
        /// Gets or sets the revision number.
        /// </summary>
        public Int16 Revision { get; set; }

        /// <summary>
        /// Is it using the Old Engine version of the format? (it shares the same version and revision numbers with DE).
        /// </summary>
        public bool IsOldEngine { get; set; }

        /// <summary>
        /// Gets or sets the main table.
        /// </summary>
        public ArmpTableMain MainTable { get; set; }
    }
}

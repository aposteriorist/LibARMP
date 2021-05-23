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
        /// Gets or sets the main table.
        /// </summary>
        public ArmpTable MainTable { get; set; }

        /// <summary>
        /// Gets or sets the subTable.
        /// </summary>
        public ArmpTable SubTable { get; set; }
    }
}

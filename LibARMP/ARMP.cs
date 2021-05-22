using System;
using System.Collections.Generic;

namespace LibARMP
{
    public class ARMP
    {

        public ARMP ()
        {

        }


        //public ARMP (string path)
        //{
        //    loadARMP(path);
        //}


        /// <summary>
        /// Reads an armp file and returns and ARMP object
        /// </summary>
        /// <param name="path">The path to the armp file.</param>
        public void loadARMP (string path)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Saves the current ARMP object to an armp file.
        /// </summary>
        /// <param name="path">The path where the armp file will be written.</param>
        public void saveARMP(string path)
        {
            //TODO
            throw new NotImplementedException();
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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LibARMP
{
    [Serializable]
    public class ArmpTableMain : ArmpTable
    {
        public ArmpTableMain() : base()
        {

        }


        /// <summary>
        /// Creates a new ArmpTableMain object and populates it with the properties of the source ArmpTable.
        /// </summary>
        /// <param name="armpTable">The source ArmpTable object.</param>
        public ArmpTableMain(ArmpTable armpTable) : this()
        {
            var srcProperties = armpTable.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var dstProperties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var srcProp in srcProperties)
            {
                foreach (var dstProp in dstProperties)
                {
                    if (srcProp.Name == dstProp.Name && srcProp.PropertyType == dstProp.PropertyType)
                    {
                        dstProp.SetValue(this, srcProp.GetValue(armpTable));
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// The subtable (indexer) of this table.
        /// </summary>
        public ArmpTableSub SubTable { get; set; }
    }
}

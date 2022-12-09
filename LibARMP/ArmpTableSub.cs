using System;
using System.Reflection;

namespace LibARMP
{
    [Serializable]
    public class ArmpTableSub : ArmpTable
    {
        internal ArmpTableSub(ArmpTableMain parentTable) : base()
        {
            this.ParentTable = parentTable;
        }


        /// <summary>
        /// Creates a new ArmpTableSub object and populates it with the properties of the source ArmpTable.
        /// </summary>
        /// <param name="parentTable">The parent ArmpTableMain.</param>
        /// <param name="armpTable">The source ArmpTable object.</param>
        internal ArmpTableSub(ArmpTableMain parentTable, ArmpTable armpTable) : this(parentTable)
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
        /// The parent table of this subtable (indexer).
        /// </summary>
        public ArmpTableMain ParentTable { get; internal set; }
    }
}

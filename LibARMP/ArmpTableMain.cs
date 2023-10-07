using System;
using System.Reflection;

namespace LibARMP
{
    [Serializable]
    public class ArmpTableMain : ArmpTable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArmpTableMain"/> class.
        /// </summary>
        internal ArmpTableMain() : base()
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ArmpTableMain"/> class and populates it with the properties of the source <see cref="ArmpTable"/>.
        /// </summary>
        /// <param name="armpTable">The source <see cref="ArmpTable"/> object.</param>
        internal ArmpTableMain(ArmpTable armpTable) : this()
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
        public ArmpTableSub SubTable { get; internal set; }
    }
}

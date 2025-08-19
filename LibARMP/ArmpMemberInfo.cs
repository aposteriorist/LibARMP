using System;
using System.Collections.Generic;

namespace LibARMP
{
    [Serializable]
    public class ArmpMemberInfo
    {
        /// <summary>
        /// The field's type.
        /// </summary>
        internal ArmpType Type { get; set; }

        /// <summary>
        /// The field's position in the member.
        /// </summary>
        internal int Position { get; set; }

        /// <summary>
        /// Size of the array, if this field begins an array.
        /// </summary>
        internal uint ArraySize { get; set; }

        /// <summary>
        /// Indices of the array elements.
        /// </summary>
        internal List<int> ArrayIndices { get; set; }

        /// <summary>
        /// Index of the associated column.
        /// </summary>
        internal int ColumnIndex { get; set; }
    }
}

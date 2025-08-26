using System;
using System.Collections.Generic;

namespace LibARMP
{
    [Serializable]
    public class ArmpTableColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArmpTableColumn"/> class.
        /// </summary>
        /// <param name="id">The column ID.</param>
        internal ArmpTableColumn(uint id)
        {
            this.ID = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArmpTableColumn"/> class.
        /// </summary>
        /// <param name="id">The column ID.</param>
        /// <param name="name">The column name.</param>
        /// <param name="type">The column type.</param>
        internal ArmpTableColumn(uint id, string name, ArmpType type) : this(id)
        {
            this.Name = name;
            this.Type = type;
        }

        /// <summary>
        /// Gets the column ID.
        /// </summary>
        public uint ID { get; internal set; }

        /// <summary>
        /// Gets the column name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the column type.
        /// </summary>
        internal ArmpType Type { get; set; }

        /// <summary>
        /// Gets the member info, if applicable.
        /// </summary>
        /// <remarks>This reference is kept to avoid unnecessarily sorting the table's MemberInfo List.</remarks>
        internal ArmpMemberInfo MemberInfo { get; set; }

        /// <summary>
        /// Gets or sets the column index.
        /// </summary>
        /// <remarks><para>Can be null if unused.</para></remarks>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets if the column is valid.
        /// </summary>
        /// <remarks><para>Can be null if unused.</para></remarks>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the unknown metadata.
        /// </summary>
        /// <remarks><para>TODO</para></remarks>
        public int UnknownMetadata0x40 { get; set; }

        /// <summary>
        /// Gets or sets the column's validity for a particular entry, rather than for the entire column.
        /// </summary>
        public List<bool> SpecificValidity { get; set; }

        /// <summary>
        /// Gets or sets the column's children.
        /// </summary>
        /// <remarks><para>Only used if the column is of an array type.</para><para><b>DRAGON ENGINE V2 ONLY</b></para></remarks>
        internal List<ArmpTableColumn> Children { get; set; }

        /// <summary>
        /// Gets or sets the column's parent.
        /// </summary>
        /// <remarks><para>Only used if the column is child of an array-type column.</para><para><b>DRAGON ENGINE V2 ONLY</b></para></remarks>
        internal ArmpTableColumn Parent { get; set; }



        /// <summary>
        /// Gets the column's data type.
        /// </summary>
        /// <returns>The column <see cref="System.Type"/>.</returns>
        public Type GetDataType()
        {
            return Type.CSType;
        }


        /// <summary>
        /// Creates a copy of this column.
        /// </summary>
        /// <returns>A copy of this <see cref="ArmpTableColumn"/>.</returns>
        public ArmpTableColumn Copy()
        {
            ArmpTableColumn copy = new ArmpTableColumn(ID, Name, Type);
            copy.Index = Index;
            copy.IsValid = IsValid;
            copy.UnknownMetadata0x40 = UnknownMetadata0x40;

            return copy;
        }
    }
}

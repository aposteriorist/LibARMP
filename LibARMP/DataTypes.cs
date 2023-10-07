using LibARMP.Exceptions;
using System;
using System.Collections.Generic;

namespace LibARMP
{
    internal class DataTypes
    {

        internal static List<ArmpType> Types = new List<ArmpType>();


        internal static List<Type> SpecialTypes = new List<Type>()
        {
            typeof(List<byte>),
            typeof(List<UInt16>),
            typeof(List<UInt32>),
            typeof(List<UInt64>),
            typeof(List<sbyte>),
            typeof(List<Int16>),
            typeof(List<Int32>),
            typeof(List<Int64>),
            typeof(List<string>),
            typeof(List<ArmpTableMain>),
            typeof(List<float>),
            typeof(List<double>),
            typeof(float[]),
            typeof(List<bool>),
        };


        static DataTypes()
        {
            Init();
        }


        /// <summary>
        /// Initializes all armp type information.
        /// </summary>
        private static void Init()
        {
            Types.Add(new ArmpType() { CSType = null, IDv1 = -1, IDAuxv1 = -1, IDv2 = -1, IDAuxv2 = 0, IDOE = -1, Size = 0x0, DefaultValue = null }); //Invalid
            Types.Add(new ArmpType() { CSType = typeof(byte), IDv1 = 2, IDAuxv1 = 0, IDv2 = 2, IDAuxv2 = 4, IDOE = 5, Size = 0x1, DefaultValue = (byte) 0 }); //uint8
            Types.Add(new ArmpType() { CSType = typeof(UInt16), IDv1 = 1, IDAuxv1 = 1, IDv2 = 1, IDAuxv2 = 3, IDOE = 4, Size = 0x2, DefaultValue = (UInt16) 0 }); //uint16
            Types.Add(new ArmpType() { CSType = typeof(UInt32), IDv1 = 0, IDAuxv1 = 2, IDv2 = 0, IDAuxv2 = 2, IDOE = 3, Size = 0x4, DefaultValue = (UInt32) 0 }); //uint32
            Types.Add(new ArmpType() { CSType = typeof(UInt64), IDv1 = 8, IDAuxv1 = 3, IDv2 = 8, IDAuxv2 = 1, IDOE = -1, Size = 0x8, DefaultValue = (UInt64) 0 }); //uint64
            Types.Add(new ArmpType() { CSType = typeof(sbyte), IDv1 = 5, IDAuxv1 = 4, IDv2 = 5, IDAuxv2 = 8, IDOE = 2, Size = 0x1, DefaultValue = (sbyte) 0 }); //int8
            Types.Add(new ArmpType() { CSType = typeof(Int16), IDv1 = 4, IDAuxv1 = 5, IDv2 = 4, IDAuxv2 = 7, IDOE = 1, Size = 0x2, DefaultValue = (Int16) 0 }); //int16
            Types.Add(new ArmpType() { CSType = typeof(Int32), IDv1 = 3, IDAuxv1 = 6, IDv2 = 3, IDAuxv2 = 6, IDOE = 0, Size = 0x4, DefaultValue = (Int32) 0 }); //int32
            Types.Add(new ArmpType() { CSType = typeof(Int64), IDv1 = 10, IDAuxv1 = 7, IDv2 = 10, IDAuxv2 = 5, IDOE = -1, Size = 0x8, DefaultValue = (Int64) 0 }); //int64
            Types.Add(new ArmpType() { CSType = typeof(float), IDv1 = 7, IDAuxv1 = 9, IDv2 = 7, IDAuxv2 = 10, IDOE = -1, Size = 0x4, DefaultValue = (float) 0 }); //float32
            Types.Add(new ArmpType() { CSType = typeof(double), IDv1 = -1, IDAuxv1 = -1, IDv2 = 11, IDAuxv2 = 9, IDOE = -1, Size = 0x8, DefaultValue = (double) 0 }); //float64
            Types.Add(new ArmpType() { CSType = typeof(bool), IDv1 = 6, IDAuxv1 = 11, IDv2 = 6, IDAuxv2 = 4, IDOE = 6, Size = 0x1, DefaultValue = false }); //boolean
            Types.Add(new ArmpType() { CSType = typeof(string), IDv1 = 0, IDAuxv1 = 12, IDv2 = 13, IDAuxv2 = 12, IDOE = 1, Size = 0x8, DefaultValue = string.Empty }); //string
            Types.Add(new ArmpType() { CSType = typeof(ArmpTableMain), IDv1 = 9, IDAuxv1 = 13, IDv2 = 9, IDAuxv2 = 13, IDOE = -1, Size = 0x8, DefaultValue = null }); //table

            Types.Add(new ArmpType() { CSType = typeof(List<byte>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 14, IDAuxv2 = 14, IDOE = -1, Size = 0x0, DefaultValue = null }); //uint8 array
            Types.Add(new ArmpType() { CSType = typeof(List<UInt16>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 15, IDAuxv2 = 15, IDOE = -1, Size = 0x0, DefaultValue = null }); //uint16 array
            Types.Add(new ArmpType() { CSType = typeof(List<UInt32>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 16, IDAuxv2 = 16, IDOE = -1, Size = 0x0, DefaultValue = null }); //uint32 array
            Types.Add(new ArmpType() { CSType = typeof(List<UInt64>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 17, IDAuxv2 = 17, IDOE = -1, Size = 0x0, DefaultValue = null }); //uint64 array
            Types.Add(new ArmpType() { CSType = typeof(List<sbyte>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 18, IDAuxv2 = 18, IDOE = -1, Size = 0x0, DefaultValue = null }); //int8 array
            Types.Add(new ArmpType() { CSType = typeof(List<Int16>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 19, IDAuxv2 = 19, IDOE = -1, Size = 0x0, DefaultValue = null }); //int16 array
            Types.Add(new ArmpType() { CSType = typeof(List<Int32>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 20, IDAuxv2 = 20, IDOE = -1, Size = 0x0, DefaultValue = null }); //int32 array
            Types.Add(new ArmpType() { CSType = typeof(List<Int64>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 21, IDAuxv2 = 21, IDOE = -1, Size = 0x0, DefaultValue = null }); //int64 array
            Types.Add(new ArmpType() { CSType = typeof(List<string>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 22, IDAuxv2 = 25, IDOE = -1, Size = 0x0, DefaultValue = null }); //string array
            Types.Add(new ArmpType() { CSType = typeof(List<ArmpTableMain>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 23, IDAuxv2 = 26, IDOE = -1, Size = 0x0, DefaultValue = null }); //table array
            Types.Add(new ArmpType() { CSType = typeof(List<float>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 25, IDAuxv2 = 23, IDOE = -1, Size = 0x0, DefaultValue = null }); //float32 array
            Types.Add(new ArmpType() { CSType = typeof(List<double>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 26, IDAuxv2 = 24, IDOE = -1, Size = 0x0, DefaultValue = null }); //float64 array
            Types.Add(new ArmpType() { CSType = typeof(float[]), IDv1 = -1, IDAuxv1 = -1, IDv2 = 27, IDAuxv2 = 27, IDOE = -1, Size = 0x0, DefaultValue = null }); //VF128
            Types.Add(new ArmpType() { CSType = typeof(List<bool>), IDv1 = -1, IDAuxv1 = -1, IDv2 = 29, IDAuxv2 = 29, IDOE = -1, Size = 0x0, DefaultValue = null }); //boolean array
        }


        /// <summary>
        /// Gets the <see cref="ArmpType"/> matching the provided C# Type.
        /// </summary>
        /// <param name="cstype">The C# type.</param>
        /// <returns>An <see cref="ArmpType"/>.</returns>
        /// <exception cref="TypeNotSupportedException">The provided C# type is not supported by the armp format.</exception>
        internal static ArmpType GetArmpTypeByCSType(Type cstype)
        {
            foreach (ArmpType type in Types)
            {
                if (type.CSType == cstype) return type;
            }
            throw new TypeNotSupportedException(cstype);
        }
    }
}

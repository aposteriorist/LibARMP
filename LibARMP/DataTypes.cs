using System;
using System.Collections.Generic;

namespace LibARMP
{
    internal class DataTypes
    {
        /// <summary>
        /// Supported file types.
        /// </summary>
        internal enum ArmpType
        {
            Invalid,
            UInt8,
            UInt16,
            UInt32,
            UInt64,
            Int8,
            Int16,
            Int32,
            Int64,
            Float32,
            Float64,
            Boolean,
            String,
            Table,
            UInt8_Array,
            UInt16_Array,
            UInt32_Array,
            UInt64_Array,
            Int8_Array,
            Int16_Array,
            Int32_Array,
            Int64_Array,
            String_Array,
            Table_Array,
            Float32_Array,
            Float64_Array,
            VF128,
            Boolean_Array,
        }


        /// <summary>
        /// ARMP types to C# types.
        /// </summary>
        internal static IDictionary<ArmpType, Type> Types = new Dictionary<ArmpType, Type>
        {
            { ArmpType.Invalid, null }, // Invalid
            { ArmpType.UInt8, typeof(byte) }, // uint8
            { ArmpType.UInt16, typeof(UInt16) }, // uint16
            { ArmpType.UInt32, typeof(UInt32) }, // uint32
            { ArmpType.UInt64, typeof(UInt64) }, // uint64
            { ArmpType.Int8, typeof(sbyte) }, // int8
            { ArmpType.Int16, typeof(Int16) }, // int16
            { ArmpType.Int32, typeof(Int32) }, // int32
            { ArmpType.Int64, typeof(Int64) }, // int64
            { ArmpType.Float32, typeof(float) }, // float32
            { ArmpType.Float64, typeof(double) }, // float64
            { ArmpType.Boolean, typeof(bool) }, // boolean
            { ArmpType.String, typeof(string) }, // string
            { ArmpType.Table, typeof(ArmpTableMain) }, // Table
            { ArmpType.UInt8_Array, typeof(List<byte>) }, // uint8 array
            { ArmpType.UInt16_Array, typeof(List<UInt16>) }, // uint16 array
            { ArmpType.UInt32_Array, typeof(List<UInt32>) }, // uint32 array
            { ArmpType.UInt64_Array, typeof(List<UInt64>) }, // uint64 array
            { ArmpType.Int8_Array, typeof(List<sbyte>) }, // int8 array
            { ArmpType.Int16_Array, typeof(List<Int16>) }, // int16 array
            { ArmpType.Int32_Array, typeof(List<Int32>) }, // int32 array
            { ArmpType.Int64_Array, typeof(List<Int64>) }, // int64 array
            { ArmpType.String_Array, typeof(List<string>) }, // string array
            { ArmpType.Table_Array, typeof(List<ArmpTableMain>) }, // table array
            { ArmpType.Float32_Array, typeof(List<float>) }, // float32 array
            { ArmpType.Float64_Array, typeof(List<double>) }, // float64 array
            { ArmpType.VF128, typeof(float[]) }, // VF128
            { ArmpType.Boolean_Array, typeof(List<bool>) }, // bool array
        };


        /// <summary>
        /// C# types to ARMP types.
        /// </summary>
        internal static IDictionary<Type, ArmpType> TypesReverse = new Dictionary<Type, ArmpType>
        {
            //{ null, ArmpType.Invalid }, // Invalid
            { typeof(byte), ArmpType.UInt8 }, // uint8
            { typeof(UInt16) , ArmpType.UInt16 }, // uint16
            { typeof(UInt32) , ArmpType.UInt32 }, // uint32
            { typeof(UInt64) , ArmpType.UInt64 }, // uint64
            { typeof(sbyte) , ArmpType.Int8 }, // int8
            { typeof(Int16) , ArmpType.Int16 }, // int16
            { typeof(Int32) , ArmpType.Int32 }, // int32
            { typeof(Int64) , ArmpType.Int64 }, // int64
            { typeof(float) , ArmpType.Float32 }, // float32
            { typeof(double) , ArmpType.Float64 }, // float64
            { typeof(bool) , ArmpType.Boolean }, // boolean
            { typeof(string) , ArmpType.String }, // string
            { typeof(ArmpTableMain) , ArmpType.Table }, // Table
            { typeof(List < byte >) , ArmpType.UInt8_Array }, // uint8 array
            { typeof(List < UInt16 >) , ArmpType.UInt16_Array }, // uint16 array
            { typeof(List < UInt32 >) , ArmpType.UInt32_Array }, // uint32 array
            { typeof(List < UInt64 >) , ArmpType.UInt64_Array }, // uint64 array
            { typeof(List < sbyte >) , ArmpType.Int8_Array }, // int8 array
            { typeof(List < Int16 >) , ArmpType.Int16_Array }, // int16 array
            { typeof(List < Int32 >) , ArmpType.Int32_Array }, // int32 array
            { typeof(List < Int64 >) , ArmpType.Int64_Array }, // int64 array
            { typeof(List < string >) , ArmpType.String_Array }, // string array
            { typeof(List < ArmpTableMain >) , ArmpType.Table_Array }, // table array
            { typeof(List < float >) , ArmpType.Float32_Array }, // float32 array
            { typeof(List < double >) , ArmpType.Float64_Array }, // float64 array
            { typeof(float[]) , ArmpType.VF128 }, // VF128
            { typeof(List < bool >) , ArmpType.Boolean_Array }, // bool array
        };


        /// <summary>
        /// Default values for armp types.
        /// </summary>
        internal static IDictionary<ArmpType, object> DefaultValues = new Dictionary<ArmpType, object>()
        {
            { ArmpType.Invalid, null },
            { ArmpType.UInt8, (byte) 0 },
            { ArmpType.UInt16, (UInt16) 0 },
            { ArmpType.UInt32, (UInt32) 0 },
            { ArmpType.UInt64, (UInt64) 0 },
            { ArmpType.Int8, (sbyte) 0 },
            { ArmpType.Int16, (Int16) 0 },
            { ArmpType.Int32, (Int32) 0 },
            { ArmpType.Int64, (Int64) 0 },
            { ArmpType.Float32, (float) 0 },
            { ArmpType.Float64, (double) 0 },
            { ArmpType.Boolean, false },
            { ArmpType.String, "" },
            { ArmpType.Table, null },
            { ArmpType.UInt8_Array, null },
            { ArmpType.UInt16_Array, null },
            { ArmpType.UInt32_Array, null },
            { ArmpType.UInt64_Array, null },
            { ArmpType.Int8_Array, null },
            { ArmpType.Int16_Array, null },
            { ArmpType.Int32_Array, null },
            { ArmpType.Int64_Array, null },
            { ArmpType.String_Array, null },
            { ArmpType.Table_Array, null },
            { ArmpType.Float32_Array, null },
            { ArmpType.Float64_Array, null },
            { ArmpType.VF128, null },
            { ArmpType.Boolean_Array, null },
        };


        /// <summary>
        /// Types for version 1.
        /// </summary>
        internal static IDictionary<sbyte, ArmpType> TypesV1 = new Dictionary<sbyte, ArmpType>
        {
            { -1, ArmpType.Invalid }, // Invalid
            { 2, ArmpType.UInt8 }, // uint8
            { 1, ArmpType.UInt16 }, // uint16
            { 0, ArmpType.UInt32 }, // uint32
            { 8, ArmpType.UInt64 }, // uint64
            { 5, ArmpType.Int8 }, // int8
            { 4, ArmpType.Int16 }, // int16
            { 3, ArmpType.Int32 }, // int32
            { 10, ArmpType.Int64 }, // int64
            { 7, ArmpType.Float32 }, // float32
            { 6, ArmpType.Boolean }, // boolean
            //{ 0, ArmpTypes.String }, // string
            { 9, ArmpType.Table } // Table
        };


        /// <summary>
        /// Types for version 1. Reverse table for writes.
        /// </summary>
        internal static IDictionary<ArmpType, sbyte> TypesV1Reverse = new Dictionary<ArmpType, sbyte>
        {
            { ArmpType.Invalid, -1 }, // Invalid
            { ArmpType.UInt8, 2 }, // uint8
            { ArmpType.UInt16, 1 }, // uint16
            { ArmpType.UInt32, 0 }, // uint32
            { ArmpType.UInt64, 8 }, // uint64
            { ArmpType.Int8, 5 }, // int8
            { ArmpType.Int16, 4 }, // int16
            { ArmpType.Int32, 3 }, // int32
            { ArmpType.Int64, 10 }, // int64
            { ArmpType.Float32, 7 }, // float32
            { ArmpType.Boolean, 6 }, // boolean
            { ArmpType.String, 0 }, // string
            { ArmpType.Table, 9 } // Table
        };


        /// <summary>
        /// Types for version 1 (auxiliary table).
        /// </summary>
        internal static IDictionary<sbyte, ArmpType> TypesV1Aux = new Dictionary<sbyte, ArmpType>
        {
            { -1, ArmpType.Invalid }, // Invalid
            { 0, ArmpType.UInt8 }, // uint8
            { 1, ArmpType.UInt16 }, // uint16
            { 2, ArmpType.UInt32 }, // uint32
            { 3, ArmpType.UInt64 }, // uint64
            { 4, ArmpType.Int8 }, // int8
            { 5, ArmpType.Int16 }, // int16
            { 6, ArmpType.Int32 }, // int32
            { 7, ArmpType.Int64 }, // int64
            { 9, ArmpType.Float32 }, // float32
            { 11, ArmpType.Boolean }, // boolean
            { 12, ArmpType.String }, // string
            { 13, ArmpType.Table } // Table
        };


        /// <summary>
        /// Types for version 1 (auxiliary table). Reverse table for writes.
        /// </summary>
        internal static IDictionary<ArmpType, sbyte> TypesV1AuxReverse = new Dictionary<ArmpType, sbyte>
        {
            { ArmpType.Invalid, -1 }, // Invalid
            { ArmpType.UInt8, 0 }, // uint8
            { ArmpType.UInt16, 1 }, // uint16
            { ArmpType.UInt32, 2 }, // uint32
            { ArmpType.UInt64, 3 }, // uint64
            { ArmpType.Int8, 4 }, // int8
            { ArmpType.Int16, 5 }, // int16
            { ArmpType.Int32, 6 }, // int32
            { ArmpType.Int64, 7 }, // int64
            { ArmpType.Float32, 9 }, // float32
            { ArmpType.Boolean, 11 }, // boolean
            { ArmpType.String, 12 }, // string
            { ArmpType.Table, 13 } // Table
        };


        /// <summary>
        /// Types for version 2.
        /// </summary>
        internal static IDictionary<sbyte, ArmpType> TypesV2 = new Dictionary<sbyte, ArmpType>
        {
            { -1, ArmpType.Invalid }, // Invalid
            { 2, ArmpType.UInt8 }, // uint8
            { 1, ArmpType.UInt16 }, // uint16
            { 0, ArmpType.UInt32 }, // uint32
            { 8, ArmpType.UInt64 }, // uint64
            { 5, ArmpType.Int8 }, // int8
            { 4, ArmpType.Int16 }, // int16
            { 3, ArmpType.Int32 }, // int32
            { 10, ArmpType.Int64 }, // int64
            { 7, ArmpType.Float32 }, // float32
            { 11, ArmpType.Float64 }, // float64
            { 6, ArmpType.Boolean }, // boolean
            { 13, ArmpType.String }, // string
            { 9, ArmpType.Table }, // Table
            { 14, ArmpType.UInt8_Array }, // uint8 array
            { 15, ArmpType.UInt16_Array }, // uint16 array
            { 16, ArmpType.UInt32_Array }, // uint32 array
            { 17, ArmpType.UInt64_Array }, // uint64 array
            { 18, ArmpType.Int8_Array }, // int8 array
            { 19, ArmpType.Int16_Array }, // int16 array
            { 20, ArmpType.Int32_Array }, // int32 array
            { 21, ArmpType.Int64_Array }, // int64 array
            { 22, ArmpType.String_Array }, // string array
            { 23, ArmpType.Table_Array }, // table array
            { 25, ArmpType.Float32_Array }, // float32 array
            { 26, ArmpType.Float64_Array }, // float64 array
            { 27, ArmpType.VF128 }, // VF128
            { 29, ArmpType.Boolean_Array }, // bool array
        };


        /// <summary>
        /// Types for version 2. Reverse table for writes.
        /// </summary>
        internal static IDictionary<ArmpType, sbyte> TypesV2Reverse = new Dictionary<ArmpType, sbyte>
        {
            { ArmpType.Invalid, -1 }, // Invalid
            { ArmpType.UInt8, 2 }, // uint8
            { ArmpType.UInt16, 1 }, // uint16
            { ArmpType.UInt32, 0 }, // uint32
            { ArmpType.UInt64, 8 }, // uint64
            { ArmpType.Int8, 5 }, // int8
            { ArmpType.Int16, 4 }, // int16
            { ArmpType.Int32, 3 }, // int32
            { ArmpType.Int64, 10 }, // int64
            { ArmpType.Float32, 7 }, // float32
            { ArmpType.Float64, 11 }, // float64
            { ArmpType.Boolean, 6 }, // boolean
            { ArmpType.String, 13 }, // string
            { ArmpType.Table, 9 }, // Table
            { ArmpType.UInt8_Array, 14 }, // uint8 array
            { ArmpType.UInt16_Array, 15 }, // uint16 array
            { ArmpType.UInt32_Array, 16 }, // uint32 array
            { ArmpType.UInt64_Array, 17 }, // uint64 array
            { ArmpType.Int8_Array, 18 }, // int8 array
            { ArmpType.Int16_Array, 19 }, // int16 array
            { ArmpType.Int32_Array, 20 }, // int32 array
            { ArmpType.Int64_Array, 21 }, // int64 array
            { ArmpType.String_Array, 22 }, // string array
            { ArmpType.Table_Array, 23 }, // table array
            { ArmpType.Float32_Array, 25 }, // float32 array
            { ArmpType.Float64_Array, 26 }, // float64 array
            { ArmpType.VF128, 27 }, // VF128
            { ArmpType.Boolean_Array, 29 }, // bool array
        };


        /// <summary>
        /// Types for version 2. (auxiliary table)
        /// </summary>
        internal static IDictionary<sbyte, ArmpType> TypesV2Aux = new Dictionary<sbyte, ArmpType>
        {
            { -1, ArmpType.Invalid }, // Invalid
            { 0, ArmpType.Invalid }, // Invalid
            { 4, ArmpType.UInt8 }, // uint8
            { 3, ArmpType.UInt16 }, // uint16
            { 2, ArmpType.UInt32 }, // uint32
            { 1, ArmpType.UInt64 }, // uint64
            { 8, ArmpType.Int8 }, // int8
            { 7, ArmpType.Int16 }, // int16
            { 6, ArmpType.Int32 }, // int32
            { 5, ArmpType.Int64 }, // int64
            { 10, ArmpType.Float32 }, // float32
            { 9, ArmpType.Float64 }, // float64
            //Note: booleans are stored as uint8
            //{ 4, ArmpTypes.Boolean }, // boolean
            { 12, ArmpType.String }, // string
            { 13, ArmpType.Table }, // Table
            { 14, ArmpType.UInt8_Array }, // uint8 array
            { 15, ArmpType.UInt16_Array }, // uint16 array
            { 16, ArmpType.UInt32_Array }, // uint32 array
            { 17, ArmpType.UInt64_Array }, // uint64 array
            { 18, ArmpType.Int8_Array }, // int8 array
            { 19, ArmpType.Int16_Array }, // int16 array
            { 20, ArmpType.Int32_Array }, // int32 array
            { 21, ArmpType.Int64_Array }, // int64 array
            { 25, ArmpType.String_Array }, // string array
            { 26, ArmpType.Table_Array }, // table array
            { 23, ArmpType.Float32_Array }, // float32 array
            { 24, ArmpType.Float64_Array }, // float64 array
            { 27, ArmpType.VF128 }, // VF128
            { 29, ArmpType.Boolean_Array }, // bool array
        };


        /// <summary>
        /// Types for version 2 (auxiliary table). [ID : AuxID].
        /// </summary>
        internal static IDictionary<sbyte, sbyte> TypeIDsV2Aux = new Dictionary<sbyte, sbyte>
        {
            { -1, 0 }, // Invalid
            { 2, 4 }, // uint8
            { 1, 3 }, // uint16
            { 0, 2 }, // uint32
            { 8, 1 }, // uint64
            { 5, 8 }, // int8
            { 4, 7 }, // int16
            { 3, 6 }, // int32
            { 10, 5 }, // int64
            { 7, 10 }, // float32
            { 11, 9 }, // float64
            { 6, 4 }, // boolean
            { 13, 12 }, // string
            { 9, 13 }, // Table
            { 14, 14 }, // uint8 array
            { 15, 15 }, // uint16 array
            { 16, 16 }, // uint32 array
            { 17, 17 }, // uint64 array
            { 18, 18 }, // int8 array
            { 19, 19 }, // int16 array
            { 20, 20 }, // int32 array
            { 21, 21 }, // int64 array
            { 22, 25 }, // string array
            { 23, 26 }, // table array
            { 25, 23 }, // float32 array
            { 26, 24 }, // float64 array
            { 27, 27 }, // VF128
            { 29, 29 }, // bool array
        };


        internal static List<Type> TypesV2Specials = new List<Type>
        {
            typeof(List<byte>), // uint8 array
            typeof(List<UInt16>), // uint16 array
            typeof(List<UInt32>), // uint32 array
            typeof(List<UInt64>), // uint64 array
            typeof(List<sbyte>), // int8 array
            typeof(List<Int16>), // int16 array
            typeof(List<Int32>), // int32 array
            typeof(List<Int64>), // int64 array
            typeof(List<string>), // string array
            typeof(List<ArmpTableMain>), // table array
            typeof(List<float>), // float32 array
            typeof(List<double>), // float64 array
            typeof(float[]), // VF128
            typeof(List<bool>), // bool array
        };


        /// <summary>
        /// Sizes for types (V2). [TypeID : Size]
        /// </summary>
        internal static IDictionary<sbyte, sbyte> TypesV2Sizes = new Dictionary<sbyte, sbyte>
        {
            { -1, 0x0 }, // Invalid
            { 2, 0x1 }, // uint8
            { 1, 0x2 }, // uint16
            { 0, 0x4 }, // uint32
            { 8, 0x8 }, // uint64
            { 5, 0x1 }, // int8
            { 4, 0x2 }, // int16
            { 3, 0x4 }, // int32
            { 10, 0x8 }, // int64
            { 7, 0x4 }, // float32
            { 11, 0x8 }, // float64
            { 6, 0x1 }, // boolean
            { 13, 0x8 }, // string
            { 9, 0x8 }, // Table
            { 14, 0x0 }, // uint8 array
            { 15, 0x0 }, // uint16 array
            { 16, 0x0 }, // uint32 array
            { 17, 0x0 }, // uint64 array
            { 18, 0x0 }, // int8 array
            { 19, 0x0 }, // int16 array
            { 20, 0x0 }, // int32 array
            { 21, 0x0 }, // int64 array
            { 22, 0x0 }, // string array
            { 23, 0x0 }, // table array
            { 25, 0x0 }, // float32 array
            { 26, 0x0 }, // float64 array
            { 27, 0x0 }, // VF128
            { 29, 0x0 }, // bool array
        };


        /// <summary>
        /// Types for Old Engine.
        /// </summary>
        internal static IDictionary<int, Type> TypesOE = new Dictionary<int, Type>
        {
            //TODO figure out type IDs
            { -1, null }, // Invalid
            { 2, typeof(sbyte) }, // int8
            { 1, typeof(Int16) }, // int16
            { 0, typeof(Int32) }, // int32
            { 5, typeof(byte) }, // uint8
            { 4, typeof(UInt16) }, // uint16
            { 3, typeof(UInt32) }, // uint32
            { 6, typeof(bool) }, // boolean
            //{ 8, typeof(??) }, // ??
        };


        /// <summary>
        /// Types for Old Engine. Reverse table for writes.
        /// </summary>
        internal static IDictionary<Type, int> TypesOEReverse = new Dictionary<Type, int>
        {
            //{ null, -1 }, // Invalid
            { typeof(sbyte), 2 }, // int8
            { typeof(Int16), 1 }, // int16
            { typeof(Int32), 0 }, // int32
            { typeof(byte), 5 }, // uint8
            { typeof(UInt16), 4 }, // uint16
            { typeof(UInt32), 3 }, // uint32
            { typeof(bool), 6 }, // boolean
            { typeof(string), 1 }, // string (indices as int16)
            //{ typeof(??), 8 }, // ??
        };



        internal static ArmpType IdToArmpType (sbyte id, bool isAux = false , int version = 2)
        {
            if (version == 1)
            {
                if (isAux)
                    return TypesV1Aux[id];
                else
                    return TypesV1[id];
            }
            else
            {
                if (isAux)
                    return TypesV2Aux[id];
                else
                    return TypesV2[id];
            }
        }


        internal static Type IdToType (sbyte id, bool isAux = false, int version = 2)
        {
            if (version == 1)
            {
                if (isAux)
                    return Types[TypesV1Aux[id]];
                else
                    return Types[TypesV1[id]];
            }
            else
            {
                if (isAux)
                    return Types[TypesV2Aux[id]];
                else
                    return Types[TypesV2[id]];
            }
        }


        internal static sbyte TypeToId (Type type, bool isAux = false, int version = 2)
        {
            if (version == 1)
            {
                if (isAux)
                    return TypesV1AuxReverse[TypesReverse[type]];
                else
                    return TypesV1Reverse[TypesReverse[type]];
            }
            else
            {
                if (isAux)
                    return TypeIDsV2Aux[TypesV2Reverse[TypesReverse[type]]];
                else
                    return TypesV2Reverse[TypesReverse[type]];
            }
        }
    }
}

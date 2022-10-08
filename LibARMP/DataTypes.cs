using System;
using System.Collections.Generic;

namespace LibARMP
{
    internal class DataTypes
    {
        //TODO add v2 types here
        internal static IDictionary<string, Type> Types = new Dictionary<string, Type>
        {
            { "invalid", null }, // Invalid
            { "uint8", typeof(byte) }, // uint8
            { "uint16", typeof(UInt16) }, // uint16
            { "uint32", typeof(UInt32) }, // uint32
            { "uint64", typeof(UInt64) }, // uint64
            { "int8", typeof(sbyte) }, // int8
            { "int16", typeof(Int16) }, // int16
            { "int32", typeof(Int32) }, // int32
            { "int64", typeof(Int64) }, // int64
            { "float32", typeof(float) }, // float32
            { "float64", typeof(double) }, // float64
            { "boolean", typeof(bool) }, // boolean
            { "string", typeof(string) }, // string
            { "table", typeof(ArmpTableMain) }, // Table
            { "uint8_array", typeof(List<byte>) }, // uint8 array
            { "uint16_array", typeof(List<UInt16>) }, // uint16 array
            { "uint32_array", typeof(List<UInt32>) }, // uint32 array
            { "uint64_array", typeof(List<UInt64>) }, // uint64 array
            { "int8_array", typeof(List<sbyte>) }, // int8 array
            { "int16_array", typeof(List<Int16>) }, // int16 array
            { "int32_array", typeof(List<Int32>) }, // int32 array
            { "int64_array", typeof(List<Int64>) }, // int64 array
            { "string_array", typeof(List<string>) }, // string array
            { "table_array", typeof(List<ArmpTableMain>) }, // table array
            { "float32_array", typeof(List<float>) }, // float32 array
            { "float64_array", typeof(List<double>) }, // float64 array
            { "vf128_array", typeof(float[]) }, // VF128
            { "bool_array", typeof(List<bool>) }, // bool array
        };


        /// <summary>
        /// Types for version 1.
        /// </summary>
        internal static IDictionary<sbyte, Type> TypesV1 = new Dictionary<sbyte, Type>
        {
            { -1, null }, // Invalid
            { 2, typeof(byte) }, // uint8
            { 1, typeof(UInt16) }, // uint16
            { 0, typeof(UInt32) }, // uint32
            { 8, typeof(UInt64) }, // uint64
            { 5, typeof(sbyte) }, // int8
            { 4, typeof(Int16) }, // int16
            { 3, typeof(Int32) }, // int32
            { 10, typeof(Int64) }, // int64
            { 7, typeof(float) }, // float32
            { 6, typeof(bool) }, // boolean
            //{ 0, typeof(string) }, // string
            { 9, typeof(ArmpTableMain) } // Table
        };


        /// <summary>
        /// Types for version 1. Reverse table for writes.
        /// </summary>
        internal static IDictionary<Type, sbyte> TypesV1Reverse = new Dictionary<Type, sbyte>
        {
            //{ null, -1 }, // Invalid
            { typeof(byte), 2 }, // uint8
            { typeof(UInt16), 1 }, // uint16
            { typeof(UInt32), 0 }, // uint32
            { typeof(UInt64), 8 }, // uint64
            { typeof(sbyte), 5 }, // int8
            { typeof(Int16), 4 }, // int16
            { typeof(Int32), 3 }, // int32
            { typeof(Int64), 10 }, // int64
            { typeof(float), 7 }, // float32
            { typeof(bool), 6 }, // boolean
            { typeof(string), 0 }, // string
            { typeof(ArmpTableMain), 9 } // Table
        };


        /// <summary>
        /// Types for version 1 (auxiliary table).
        /// </summary>
        internal static IDictionary<sbyte, Type> TypesV1Aux = new Dictionary<sbyte, Type>
        {
            { -1, null }, // Invalid
            { 0, typeof(byte) }, // uint8
            { 1, typeof(UInt16) }, // uint16
            { 2, typeof(UInt32) }, // uint32
            { 3, typeof(UInt64) }, // uint64
            { 4, typeof(sbyte) }, // int8
            { 5, typeof(Int16) }, // int16
            { 6, typeof(Int32) }, // int32
            { 7, typeof(Int64) }, // int64
            { 9, typeof(float) }, // float32
            { 11, typeof(bool) }, // boolean
            { 12, typeof(string) }, // string
            { 13, typeof(ArmpTableMain) } // Table
        };


        /// <summary>
        /// Types for version 1 (auxiliary table). Reverse table for writes.
        /// </summary>
        internal static IDictionary<Type, sbyte> TypesV1AuxReverse = new Dictionary<Type, sbyte>
        {
            //{ null, -1 }, // Invalid
            { typeof(byte), 0 }, // uint8
            { typeof(UInt16), 1 }, // uint16
            { typeof(UInt32), 2 }, // uint32
            { typeof(UInt64), 3 }, // uint64
            { typeof(sbyte), 4 }, // int8
            { typeof(Int16), 5 }, // int16
            { typeof(Int32), 6 }, // int32
            { typeof(Int64), 7 }, // int64
            { typeof(float), 9 }, // float32
            { typeof(bool), 11 }, // boolean
            { typeof(string), 12 }, // string
            { typeof(ArmpTableMain), 13 } // Table
        };


        /// <summary>
        /// Types for version 2.
        /// </summary>
        internal static IDictionary<sbyte, Type> TypesV2 = new Dictionary<sbyte, Type>
        {
            { -1, null }, // Invalid
            { 2, typeof(byte) }, // uint8
            { 1, typeof(UInt16) }, // uint16
            { 0, typeof(UInt32) }, // uint32
            { 8, typeof(UInt64) }, // uint64
            { 5, typeof(sbyte) }, // int8
            { 4, typeof(Int16) }, // int16
            { 3, typeof(Int32) }, // int32
            { 10, typeof(Int64) }, // int64
            { 7, typeof(float) }, // float32
            { 11, typeof(double) }, // float64
            { 6, typeof(bool) }, // boolean
            { 13, typeof(string) }, // string
            { 9, typeof(ArmpTableMain) }, // Table
            { 14, typeof(List<byte>) }, // uint8 array
            { 15, typeof(List<UInt16>) }, // uint16 array
            { 16, typeof(List<UInt32>) }, // uint32 array
            { 17, typeof(List<UInt64>) }, // uint64 array
            { 18, typeof(List<sbyte>) }, // int8 array
            { 19, typeof(List<Int16>) }, // int16 array
            { 20, typeof(List<Int32>) }, // int32 array
            { 21, typeof(List<Int64>) }, // int64 array
            { 22, typeof(List<string>) }, // string array
            { 23, typeof(List<ArmpTableMain>) }, // table array
            { 25, typeof(List<float>) }, // float32 array
            { 26, typeof(List<double>) }, // float64 array
            { 27, typeof(float[]) }, // VF128
            { 29, typeof(List<bool>) }, // bool array
        };


        /// <summary>
        /// Types for version 2. Reverse table for writes.
        /// </summary>
        internal static IDictionary<Type, sbyte> TypesV2Reverse = new Dictionary<Type, sbyte>
        {
            //{ null, -1 }, // Invalid
            { typeof(byte), 2 }, // uint8
            { typeof(UInt16), 1 }, // uint16
            { typeof(UInt32), 0 }, // uint32
            { typeof(UInt64), 8 }, // uint64
            { typeof(sbyte), 5 }, // int8
            { typeof(Int16), 4 }, // int16
            { typeof(Int32), 3 }, // int32
            { typeof(Int64), 10 }, // int64
            { typeof(float), 7 }, // float32
            { typeof(double), 11 }, // float64
            { typeof(bool), 6 }, // boolean
            { typeof(string), 13 }, // string
            { typeof(ArmpTableMain), 9 }, // Table
            { typeof(List<byte>), 14 }, // uint8 array
            { typeof(List<UInt16>), 15 }, // uint16 array
            { typeof(List<UInt32>), 16 }, // uint32 array
            { typeof(List<UInt64>), 17 }, // uint64 array
            { typeof(List<sbyte>), 18 }, // int8 array
            { typeof(List<Int16>), 19 }, // int16 array
            { typeof(List<Int32>), 20 }, // int32 array
            { typeof(List<Int64>), 21 }, // int64 array
            { typeof(List<string>), 22 }, // string array
            { typeof(List<ArmpTableMain>), 23 }, // table array
            { typeof(List<float>), 25 }, // float32 array
            { typeof(List<double>), 26 }, // float64 array
            { typeof(float[]), 27 }, // VF128
            { typeof(List<bool>), 29 }, // bool array
        };


        /// <summary>
        /// Types for version 2. (auxiliary table)
        /// </summary>
        internal static IDictionary<sbyte, Type> TypesV2Aux = new Dictionary<sbyte, Type>
        {
            { -1, null }, // Invalid
            { 0, null }, // Invalid
            { 4, typeof(byte) }, // uint8
            { 3, typeof(UInt16) }, // uint16
            { 2, typeof(UInt32) }, // uint32
            { 1, typeof(UInt64) }, // uint64
            { 8, typeof(sbyte) }, // int8
            { 7, typeof(Int16) }, // int16
            { 6, typeof(Int32) }, // int32
            { 5, typeof(Int64) }, // int64
            { 10, typeof(float) }, // float32
            { 9, typeof(double) }, // float64
            //Note: booleans are stored as uint8
            //{ 4, typeof(bool) }, // boolean
            { 12, typeof(string) }, // string
            { 13, typeof(ArmpTableMain) }, // Table
            { 14, typeof(List<byte>) }, // uint8 array
            { 15, typeof(List<UInt16>) }, // uint16 array
            { 16, typeof(List<UInt32>) }, // uint32 array
            { 17, typeof(List<UInt64>) }, // uint64 array
            { 18, typeof(List<sbyte>) }, // int8 array
            { 19, typeof(List<Int16>) }, // int16 array
            { 20, typeof(List<Int32>) }, // int32 array
            { 21, typeof(List<Int64>) }, // int64 array
            { 25, typeof(List<string>) }, // string array
            { 26, typeof(List<ArmpTableMain>) }, // table array
            { 23, typeof(List<float>) }, // float32 array
            { 24, typeof(List<double>) }, // float64 array
            { 27, typeof(float[]) }, // VF128
            { 29, typeof(List<bool>) }, // bool array
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
            { 27, 0x0 }, // VF128 ?? TODO
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
    }
}

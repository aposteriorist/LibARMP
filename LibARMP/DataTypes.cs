using System;
using System.Collections.Generic;

namespace LibARMP
{
    public class DataTypes
    {
        //TODO add v2 types here
        public static IDictionary<string, Type> Types = new Dictionary<string, Type>
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
            { "table", typeof(ArmpTable) } // Table
        };


        /// <summary>
        /// Types for version 1.
        /// </summary>
        public static IDictionary<sbyte, Type> TypesV1 = new Dictionary<sbyte, Type>
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
            { 9, typeof(ArmpTable) } // Table
        };


        /// <summary>
        /// Types for version 1. Reverse table for writes.
        /// </summary>
        public static IDictionary<Type, sbyte> TypesV1Reverse = new Dictionary<Type, sbyte>
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
            { typeof(ArmpTable), 9 } // Table
        };


        /// <summary>
        /// Types for version 1 (auxiliary table).
        /// </summary>
        public static IDictionary<sbyte, Type> TypesV1Aux = new Dictionary<sbyte, Type>
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
            { 13, typeof(ArmpTable) } // Table
        };


        /// <summary>
        /// Types for version 1 (auxiliary table). Reverse table for writes.
        /// </summary>
        public static IDictionary<Type, sbyte> TypesV1AuxReverse = new Dictionary<Type, sbyte>
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
            { typeof(ArmpTable), 13 } // Table
        };


        /// <summary>
        /// Types for version 2.
        /// </summary>
        public static IDictionary<sbyte, Type> TypesV2 = new Dictionary<sbyte, Type>
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
            { 9, typeof(ArmpTable) }, // Table
            { 14, typeof(List<byte>) }, // uint8 array
            { 15, typeof(List<UInt16>) }, // uint16 array
            { 16, typeof(List<UInt32>) }, // uint32 array
            { 17, typeof(List<UInt64>) }, // uint64 array
            { 18, typeof(List<sbyte>) }, // int8 array
            { 19, typeof(List<Int16>) }, // int16 array
            { 20, typeof(List<Int32>) }, // int32 array
            { 21, typeof(List<Int64>) }, // int64 array
            { 22, typeof(List<string>) }, // string array
            { 23, typeof(List<ArmpTable>) }, // table array
            { 25, typeof(List<float>) }, // float32 array
            { 26, typeof(List<double>) }, // float64 array
            { 27, typeof(List<float>) }, // VF128 ?? TODO
            { 29, typeof(List<bool>) }, // bool array
        };



        /// <summary>
        /// Types for version 2. (auxiliary table)
        /// </summary>
        public static IDictionary<sbyte, Type> TypesV2Aux = new Dictionary<sbyte, Type>
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
            { 13, typeof(ArmpTable) }, // Table
            { 14, typeof(byte) }, // uint8 array
            { 15, typeof(UInt16) }, // uint16 array
            { 16, typeof(UInt32) }, // uint32 array
            { 17, typeof(UInt64) }, // uint64 array
            { 18, typeof(sbyte) }, // int8 array
            { 19, typeof(Int16) }, // int16 array
            { 20, typeof(Int32) }, // int32 array
            { 21, typeof(Int64) }, // int64 array
            { 25, typeof(string) }, // string array
            { 26, typeof(ArmpTable) }, // table array
            { 23, typeof(float) }, // float32 array
            { 24, typeof(double) }, // float64 array
            { 27, typeof(float) }, // VF128 ?? TODO
            { 29, typeof(bool) }, // bool array
        };


        public static List<Type> TypesV2Specials = new List<Type>
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
            typeof(List<ArmpTable>), // table array
            typeof(List<float>), // float32 array
            typeof(List<double>), // float64 array
            //typeof(List<float>), // VF128 ?? TODO
            typeof(List<bool>), // bool array
        };


        /// <summary>
        /// Types for Old Engine.
        /// </summary>
        public static IDictionary<int, Type> TypesOE = new Dictionary<int, Type>
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
            //{ 0, typeof(string) }, // string
        };


        /// <summary>
        /// Types for Old Engine. Reverse table for writes.
        /// </summary>
        public static IDictionary<Type, int> TypesOEReverse = new Dictionary<Type, int>
        {
            //{ null, -1 }, // Invalid
            { typeof(sbyte), 2 }, // int8
            { typeof(Int16), 1 }, // int16
            { typeof(Int32), 0 }, // int32
            { typeof(byte), 5 }, // uint8
            { typeof(UInt16), 4 }, // uint16
            { typeof(UInt32), 3 }, // uint32
            { typeof(bool), 6 }, // boolean
        };
    }
}

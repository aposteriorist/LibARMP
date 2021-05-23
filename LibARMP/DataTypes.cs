using System;
using System.Collections.Generic;

namespace LibARMP
{
    public class DataTypes
    {

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





        public enum TypesV2
        {
            Invalid = -1,
            Uint8 = 2,
            Uint16 = 1,
            Uint32 = 0,
            Uint64 = 8,
            Int8 = 5,
            Int16 = 4,
            Int32 = 3,
            Int64 = 10,
            Float32 = 7,
            Float64 = 11,
            Boolean = 6,
            String = 13,
            Table = 9,
            Uint8_Array = 14,
            Uint16_Array = 15,
            Uint32_Array = 16,
            Uint64_Array = 17,
            Int8_Array = 18,
            Int16_Array = 19,
            Int32_Array = 20,
            Int64_Array = 21,
            Float32_Array = 25,
            Float64_Array = 26,
            VF128 = 27,
            Boolean_Array = 29,
            String_Array = 22,
            Table_Array = 23
        }


        public enum TypesV2Aux
        {
            Invalid = -1,
            Uint8 = 4,
            Uint16 = 3,
            Uint32 = 2,
            Uint64 = 1,
            Int8 = 8,
            Int16 = 7,
            Int32 = 6,
            Int64 = 5,
            Float32 = 10,
            Float64 = 9,
            Boolean = 4,
            String = 12,
            Table = 13,
            Uint8_Array = 14,
            Uint16_Array = 15,
            Uint32_Array = 16,
            Uint64_Array = 17,
            Int8_Array = 18,
            Int16_Array = 19,
            Int32_Array = 20,
            Int64_Array = 21,
            Float32_Array = 23,
            Float64_Array = 24,
            VF128 = 27,
            Boolean_Array = 29,
            String_Array = 25,
            Table_Array = 26
        }

    }
}

using System;
using System.Collections.Generic;
using Yarhl.IO;

namespace LibARMP
{
    class Util
    {
        /// <summary>
        /// Stores a determined amount of offsets into a list.
        /// </summary>
        /// <param name="reader">The DataStream Reader.</param>
        /// <param name="ptrOffsetList">Pointer to the Offset List.</param>
        /// <param name="amount">Amount of offsets to store.</param>
        /// <returns>An int list.</returns>
        public static List<int> IterateOffsetList (DataReader reader, int ptrOffsetList, int amount)
        {
            List<int> offsetList = new List<int>();
            reader.Stream.Seek(ptrOffsetList);

            for (int i=0; i < amount; i++)
            {
                offsetList.Add(reader.ReadInt32());
            }
            return offsetList;
        }


        /// <summary>
        /// Reads strings based on an offset list.
        /// </summary>
        /// <param name="reader">The DataStream Reader.</param>
        /// <param name="offsetList">The String Offset List.</param>
        /// <returns>A string list.</returns>
        public static List<string> IterateStringList (DataReader reader, List<int> offsetList)
        {
            List<string> stringList = new List<string>();

            foreach (int offset in offsetList) 
            {
                reader.Stream.Seek(offset);
                stringList.Add(reader.ReadString());
            }
            return stringList;
        }


        /// <summary>
        /// Reads a bitmask.
        /// </summary>
        /// <param name="reader">The DataStream Reader.</param>
        /// <param name="ptrBitmask">The pointer to the bitmask.</param>
        /// <param name="amount">The amount of values in the bitmask.</param>
        /// <returns>A bool list.</returns>
        public static List<bool> IterateBooleanBitmask (DataReader reader, int ptrBitmask, int amount)
        {
            List<bool> booleanList = new List<bool>();

            reader.Stream.Seek(ptrBitmask);
            for (int i = 0; i < Math.Ceiling((float)amount/8); i++)
            {
                byte b = reader.ReadByte();
                var bitstring = Convert.ToString(b, 2);
                bitstring = ReverseString(bitstring);
                foreach(char c in bitstring)
                {
                    if (booleanList.Count < amount)
                    {
                        bool boolvalue = false;
                        if (c == '1') boolvalue = true;
                        booleanList.Add(boolvalue);
                    }
                }
            }
            return booleanList;
        }


        /// <summary>
        /// Reverses a string.
        /// </summary>
        /// <param name="str">The string to reverse.</param>
        /// <returns>A reversed string.</returns>
        public static string ReverseString (string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }


        /// <summary>
        /// Reads an array of type T.
        /// </summary>
        /// <param name="reader">The DataStream Reader.</param>
        /// <param name="ptrArray">The pointer to the array.</param>
        /// <param name="amount">The amount of values in the array.</param>
        /// <returns>A list.</returns>
        public static dynamic IterateArray<T> (DataReader reader, int ptrArray, int amount)
        {
            List<T> returnList = new List<T>();
            reader.Stream.Seek(ptrArray);

            for (int i=0; i<amount; i++)
            {
                T val = reader.Read<T>();
                returnList.Add(val);
            } 

            return returnList;
        }

    }
}

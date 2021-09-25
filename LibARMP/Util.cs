using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Yarhl.IO;

namespace LibARMP
{
    public static class Util
    {
        /// <summary>
        /// Stores a determined amount of offsets into a list.
        /// </summary>
        /// <param name="reader">The DataStream Reader.</param>
        /// <param name="ptrOffsetList">Pointer to the Offset List.</param>
        /// <param name="amount">Amount of offsets to store.</param>
        /// <returns>An int list.</returns>
        public static List<UInt32> IterateOffsetList (DataReader reader, UInt32 ptrOffsetList, int amount)
        {
            List<UInt32> offsetList = new List<UInt32>();
            reader.Stream.Seek(ptrOffsetList);

            for (int i=0; i < amount; i++)
            {
                offsetList.Add(reader.ReadUInt32());
            }
            return offsetList;
        }


        /// <summary>
        /// Reads strings based on an offset list.
        /// </summary>
        /// <param name="reader">The DataStream Reader.</param>
        /// <param name="offsetList">The String Offset List.</param>
        /// <returns>A string list.</returns>
        public static List<string> IterateStringList (DataReader reader, List<UInt32> offsetList)
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
        public static List<bool> IterateBooleanBitmask (DataReader reader, UInt32 ptrBitmask, int amount)
        {
            List<bool> booleanList = new List<bool>();

            reader.Stream.Seek(ptrBitmask);
            for (int i = 0; i < Math.Ceiling((float)amount/8); i++)
            {
                byte b = reader.ReadByte();
                var bitstring = Convert.ToString(b, 2).PadLeft(8, '0');
                bitstring = ReverseString(bitstring);
                foreach (char c in bitstring)
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
        public static dynamic IterateArray<T> (DataReader reader, UInt32 ptrArray, int amount)
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


        /// <summary>
        /// Writes a boolean list to a stream.
        /// </summary>
        /// <param name="writer">The DataWriter.</param>
        /// <param name="boolList">The boolean list.</param>
        public static void WriteBooleanBitmask (DataWriter writer, List<bool> boolList)
        {
            string bitstring = "";
            foreach (bool boolValue in boolList)
            {
                bitstring += Convert.ToByte(boolValue).ToString();
                if (bitstring.Length == 8)
                {
                    byte value = Convert.ToByte(ReverseString(bitstring), 2);
                    bitstring = "";
                    writer.Write(value);
                }
            }
            if (bitstring != "")
            {
                byte value = Convert.ToByte(ReverseString(bitstring).PadLeft(8, '0'), 2);
                writer.Write(value);
            }
        }


        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="writer">The DataWriter.</param>
        /// <param name="textList">The text list.</param>
        /// <returns>The pointer to the text offset table.</returns>
        public static int WriteText (DataWriter writer, List<string> textList)
        {
            List<int> ptrList = new List<int>();
            
            foreach (string text in textList)
            {
                ptrList.Add((int)writer.Stream.Position);
                writer.Write(text, true);
            }
            writer.WritePadding(0x00, 8);
            int ptrOffsetTable = (int)writer.Stream.Position;

            foreach(int pointer in ptrList)
            {
                writer.Write(pointer);
            }
            writer.WritePadding(0x00, 8);
            return ptrOffsetTable;
        }


        /// <summary>
        /// Deep copy an object.
        /// </summary>
        public static T DeepCopy<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

    }
}

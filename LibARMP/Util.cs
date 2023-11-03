using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Yarhl.IO;

namespace LibARMP
{
    internal static class Util
    {
        /// <summary>
        /// Stores a determined amount of offsets into a list.
        /// </summary>
        /// <param name="reader">The <see cref="DataStream"/> Reader.</param>
        /// <param name="ptrOffsetList">Pointer to the Offset List.</param>
        /// <param name="amount">Amount of offsets to store.</param>
        /// <returns>A <see cref="uint"/> list.</returns>
        internal static List<UInt32> IterateOffsetList (DataReader reader, UInt32 ptrOffsetList, int amount)
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
        /// <param name="reader">The <see cref="DataReader"/>.</param>
        /// <param name="offsetList">The String Offset List.</param>
        /// <returns>A <see cref="string"/> list.</returns>
        internal static List<string> IterateStringList (DataReader reader, List<UInt32> offsetList, Encoding encoding = null)
        {
            List<string> stringList = new List<string>();

            foreach (int offset in offsetList) 
            {
                reader.Stream.Seek(offset);
                stringList.Add(reader.ReadString(encoding));
            }
            return stringList;
        }


        /// <summary>
        /// Reads a bitmask.
        /// </summary>
        /// <param name="reader">The <see cref="DataReader"/>.</param>
        /// <param name="ptrBitmask">The pointer to the bitmask.</param>
        /// <param name="amount">The amount of values in the bitmask.</param>
        /// <returns>A <see cref="Boolean"/> list.</returns>
        internal static List<bool> IterateBooleanBitmask (DataReader reader, UInt32 ptrBitmask, int amount)
        {
            List<bool> boolList = new List<bool>();

            reader.Stream.Seek(ptrBitmask);

            for (int i = 0; i < Math.Ceiling((float)amount/8); i++)
            {
                int bitmask = reader.ReadInt32();

                for (int j = 0; j < 32 && boolList.Count < amount; j++)
                {
                    boolList.Add((bitmask & (1 << j)) != 0);
                }
            }

            return boolList;
        }


        /// <summary>
        /// Reverses a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to reverse.</param>
        /// <returns>A reversed <see cref="string"/>.</returns>
        internal static string ReverseString (string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }


        /// <summary>
        /// Reads an array of type T.
        /// </summary>
        /// <param name="reader">The <see cref="DataReader"/>.</param>
        /// <param name="ptrArray">The pointer to the array.</param>
        /// <param name="amount">The amount of values in the array.</param>
        /// <returns>A list of type T.</returns>
        internal static dynamic IterateArray<T> (DataReader reader, UInt32 ptrArray, int amount)
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
        /// Writes a <see cref="Boolean"/> list to a stream.
        /// </summary>
        /// <param name="writer">The <see cref="DataWriter"/>.</param>
        /// <param name="boolList">The <see cref="Boolean"/> list.</param>
        internal static void WriteBooleanBitmask (DataWriter writer, List<bool> boolList)
        {
            int index = 0;

            while (index < boolList.Count)
            {
                int bitmask = 0;

                for (int i = 0; i < 32 && index < boolList.Count; i++)
                {
                    if (boolList[index])
                    {
                        bitmask |= (1 << i);
                    }
                    index++;
                }

                writer.Write(bitmask);
            }
        }


        /// <summary>
        /// Writes a text list and its offset table to the stream.
        /// </summary>
        /// <param name="writer">The <see cref="DataWriter"/>.</param>
        /// <param name="textList">The text list.</param>
        /// <returns>The pointer to the text offset table.</returns>
        internal static int WriteText (DataWriter writer, List<string> textList, Encoding encoding = null)
        {
            List<int> ptrList = new List<int>();
            
            foreach (string text in textList)
            {
                ptrList.Add((int)writer.Stream.Position);
                writer.Write(text, true, encoding);
            }
            writer.WritePadding(0x00, 0x10);
            int ptrOffsetTable = (int)writer.Stream.Position;

            foreach(int pointer in ptrList)
            {
                writer.Write(pointer);
            }

            return ptrOffsetTable;
        }


        /// <summary>
        /// Counts the amount of occurences of a substring in a <see cref="string"/> list.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to look for.</param>
        /// <param name="strList">The <see cref="string"/> list to look for occurrences in.</param>
        /// <returns>The amount of occurrences.</returns>
        internal static int CountStringOccurrences(string str, List<string> strList)
        {
            int count = 0;

            foreach(string str2 in strList)
            {
                if (str2.Contains(str)) count++;
            }

            return count;
        }


        /// <summary>
        /// Deep copy an object.
        /// </summary>
        internal static T DeepCopy<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }



        ///// EXTENSIONS /////

        /// <summary>
        /// Writes the complete <see cref="DataStream"/> into a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The destination <see cref="Stream"/>.</param>
        internal static void WriteTo(this DataStream ds, Stream stream)
        {
            ds.Seek(0);
            byte[] temp = new byte[ds.Length];
            ds.Read(temp, 0, (int)ds.Length);
            stream.Write(temp, 0, temp.Length);
            stream.Seek(0, SeekOrigin.Begin);
        }


        /// <summary>
        /// Returns the contents of the <see cref="DataStream"/> as a byte array.
        /// </summary>
        internal static byte[] ToArray(this DataStream ds)
        {
            ds.Seek(0);
            byte[] array = new byte[ds.Length];
            ds.Read(array, 0, (int)ds.Length);
            return array;
        }


        /// <summary>
        /// Push the current position into a stack and move to a new one, write the specified 32-bit signed value,
        /// pop the last position in the stack and move to it.
        /// </summary>
        /// <param name="dw">The DataWriter.</param>
        /// <param name="val">32-bits signed value.</param>
        /// <param name="shift">Distance to move position.</param>
        /// <param name="mode">Mode to move position.</param>
        internal static void PushWritePop(this DataWriter dw, int val, long shift, SeekMode mode = SeekMode.Start)
        {
            dw.Stream.PushToPosition(shift, mode);
            dw.Write(val);
            dw.Stream.PopPosition();
        }


        /// <summary>
        /// Push the current position into a stack and move to a new one, write the specified 32-bit signed values,
        /// pop the last position in the stack and move to it.
        /// </summary>
        /// <param name="dw">The DataWriter.</param>
        /// <param name="vals">32-bits signed value array.</param>
        /// <param name="shift">Distance to move position.</param>
        /// <param name="mode">Mode to move position.</param>
        internal static void PushWritePop(this DataWriter dw, int[] vals, long shift, SeekMode mode = SeekMode.Start)
        {
            dw.Stream.PushToPosition(shift, mode);
            foreach (int val in vals)
            {
                dw.Write(val);
            }
            dw.Stream.PopPosition();
        }


        /// <summary>
        /// Attempts to add the specified key and value to the dictionary.
        /// </summary>
        /// <param name="dictionary">The Dictionary.</param>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. It can be <c>null</c>.</param>
        /// <returns><c>true</c> if the key/value pair was added to the dictionary successfully; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        internal static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace LibARMP.UnitTests
{
    [TestClass]
    public class APITests
    {
        ///// ArmpEntry /////
        
        [TestMethod]
        public void ArmpEntry_ID()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            int id = armp.MainTable.GetEntry("value").ID;
            Assert.AreEqual(1, id);
        }


        [TestMethod]
        public void ArmpEntry_Name()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            string name = armp.MainTable.GetEntry(1).Name;
            Assert.AreEqual("value", name);
        }


        [TestMethod]
        public void ArmpEntry_Index()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            int index = armp.MainTable.GetEntry("value").Index;
            Assert.AreEqual(1, index);
        }


        [TestMethod]
        public void ArmpEntry_IsValid()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            bool valid = armp.MainTable.GetEntry("value").IsValid;
            Assert.IsTrue(valid);
        }


        [TestMethod]
        public void ArmpEntry_Flags()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            bool[] flags = armp.MainTable.GetEntry("value").Flags;
            Assert.IsFalse(flags[0]);
            Assert.IsTrue(flags[1]);
            Assert.IsTrue(flags[2]);
            Assert.IsFalse(flags[3]);
            Assert.IsFalse(flags[4]);
            Assert.IsFalse(flags[5]);
            Assert.IsFalse(flags[6]);
            Assert.IsFalse(flags[7]);
        }


        [TestMethod]
        public void ArmpEntry_GetValueFromColumn()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            ArmpEntry entry = armp.MainTable.GetEntry("value");
            ArmpTableColumn column = armp.MainTable.GetColumn("u16_");
            Assert.AreEqual((UInt16)800, entry.GetValueFromColumn<UInt16>("u16_"));
            Assert.AreEqual((UInt16)800, entry.GetValueFromColumn<UInt16>(2));
            Assert.AreEqual((UInt16)800, entry.GetValueFromColumn<UInt16>(column));
            Assert.AreEqual((UInt16)800, entry.GetValueFromColumn("u16_"));
            Assert.AreEqual((UInt16)800, entry.GetValueFromColumn(2));
            Assert.AreEqual((UInt16)800, entry.GetValueFromColumn(column));
        }


        [TestMethod]
        public void ArmpEntry_SetValueFromColumn()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            ArmpEntry entry = armp.MainTable.GetEntry("value");
            UInt64 expected = (UInt64)77777777777;
            entry.SetValueFromColumn("u64_", expected);
            var result = entry.GetValueFromColumn("u64_");
            Assert.AreEqual(expected, result);
        }



        ///// ArmpTable /////

        [TestMethod]
        public void ArmpTable_GetAllEntries()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            List<ArmpEntry> entries = armp.MainTable.GetAllEntries();
            Assert.AreEqual(4, entries.Count);
            Assert.AreEqual("", entries[0].Name);
            Assert.AreEqual("value", entries[1].Name);
            Assert.AreEqual("min_value", entries[2].Name);
            Assert.AreEqual("max_value", entries[3].Name);
        }


        [TestMethod]
        public void ArmpTable_GetEntry()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            ArmpEntry entry = armp.MainTable.GetEntry(1);
            Assert.AreEqual("value", entry.Name);
            entry = armp.MainTable.GetEntry("value");
            Assert.AreEqual(1, entry.ID);
        }


        [TestMethod]
        public void ArmpTable_GetEntryNames()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            List<string> names = armp.MainTable.GetEntryNames();
            Assert.AreEqual(4, names.Count);
            Assert.AreEqual("", names[0]);
            Assert.AreEqual("value", names[1]);
            Assert.AreEqual("min_value", names[2]);
            Assert.AreEqual("max_value", names[3]);
        }


        [TestMethod]
        public void ArmpTable_GetEntryName()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            string name = armp.MainTable.GetEntryName(1);
            Assert.AreEqual("value", name);
        }


        [TestMethod]
        public void ArmpTable_GetColumn()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            ArmpTableColumn column = armp.MainTable.GetColumn(4);
            Assert.AreEqual("u64_", column.Name);
            column = armp.MainTable.GetColumn("f32_");
            Assert.AreEqual(9, column.ID);
        }


        [TestMethod]
        public void ArmpTable_GetColumnNames()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            List<string> names = armp.MainTable.GetColumnNames();
            Assert.AreEqual(70, names.Count);
            names = armp.MainTable.GetColumnNames(false);
            Assert.AreEqual(55, names.Count);
        }


        [TestMethod]
        public void ArmpTable_GetColumnName()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            string name = armp.MainTable.GetColumnName(4);
            Assert.AreEqual("u64_", name);
        }


        [TestMethod]
        public void ArmpTable_GetColumnDataType()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            Type type = armp.MainTable.GetColumnDataType("f64_");
            Assert.AreEqual(typeof(double), type);
        }


        [TestMethod]
        public void ArmpTable_GetColumnNamesByType()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            List<string> names = armp.MainTable.GetColumnNamesByType(typeof(Int64));
            Assert.AreEqual(3, names.Count);
            names = armp.MainTable.GetColumnNamesByType<Int64>();
            Assert.AreEqual(3, names.Count);
        }


        [TestMethod]
        public void ArmpTable_GetColumnIndicesByType()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            List<int> indices = armp.MainTable.GetColumnIndicesByType(typeof(Int64));
            Assert.AreEqual(3, indices.Count);
            Assert.AreEqual(8, indices[0]);
            indices = armp.MainTable.GetColumnIndicesByType<Int64>();
            Assert.AreEqual(3, indices.Count);
            Assert.AreEqual(8, indices[0]);
        }


        [TestMethod]
        public void ArmpTable_GetColumnIndex()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            int index = armp.MainTable.GetColumnIndex("s32_");
            Assert.AreEqual(7, index);
        }


        [TestMethod]
        public void ArmpTable_GetColumnOverrideIndex()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            int index = armp.MainTable.GetColumnOverrideIndex("s32_");
            Assert.AreEqual(8, index);
            index = armp.MainTable.GetColumnOverrideIndex(7);
            Assert.AreEqual(8, index);
        }


        [TestMethod]
        public void ArmpTable_SetColumnOverrideIndex()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.SetColumnOverrideIndex("s32_", 123);
            int index = armp.MainTable.GetColumnOverrideIndex("s32_");
            Assert.AreEqual(123, index);
            armp.MainTable.SetColumnOverrideIndex(7, 321);
            index = armp.MainTable.GetColumnOverrideIndex(7);
            Assert.AreEqual(321, index);
        }


        [TestMethod]
        public void ArmpTable_IsColumnValid()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            bool valid = armp.MainTable.IsColumnValid("f32_");
            Assert.IsTrue(valid);
            valid = armp.MainTable.IsColumnValid(0);
            Assert.IsFalse(valid);
        }


        [TestMethod]
        public void ArmpTable_IsColumnSpecial()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            bool special = armp.MainTable.IsColumnSpecial("vf128_");
            Assert.IsTrue(special);
            special = armp.MainTable.IsColumnSpecial("f32_");
            Assert.IsFalse(special);
        }


        [TestMethod]
        public void ArmpTable_SearchByName()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            List<ArmpEntry> entries = armp.MainTable.SearchByName("value");
            Assert.AreEqual(3, entries.Count);
            entries = armp.MainTable.SearchByName("min_");
            Assert.AreEqual(1, entries.Count);
        }


        [TestMethod]
        public void ArmpTable_SearchByValue()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            List<ArmpEntry> entries = armp.MainTable.SearchByValue("s16_", (Int16)32767);
            Assert.AreEqual(1, entries.Count);
            entries = armp.MainTable.SearchByValue("u64_array[0]", (UInt64)0);
            Assert.AreEqual(3, entries.Count);
        }
    }
}

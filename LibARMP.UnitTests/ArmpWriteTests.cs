using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LibARMP.UnitTests
{
    [TestClass]
    public class ArmpWriteTests
    {
        ///// V1 /////

        [TestMethod]
        public void WriteARMP_v1()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp.Version, armp_new.Version);
            Assert.AreEqual(armp.Revision, armp_new.Revision);
            Assert.AreEqual(armp.IsOldEngine, armp_new.IsOldEngine);
            CollectionAssert.AreEqual(armp.MainTable.GetRowNames(), armp_new.MainTable.GetRowNames());
            CollectionAssert.AreEqual(armp.MainTable.GetColumnNames(), armp_new.MainTable.GetColumnNames());
            Assert.AreEqual(armp.MainTable.GetEntry(1).GetValueFromColumn("string"), armp_new.MainTable.GetEntry(1).GetValueFromColumn("string"));
        }


        [TestMethod]
        public void u8_v1AllTypes()
        {
            byte expectedValue = 123;
            string columnName = "u8";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void u16_v1AllTypes()
        {
            UInt16 expectedValue = 12345;
            string columnName = "u16";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void u32_v1AllTypes()
        {
            uint expectedValue = 123456789;
            string columnName = "u32";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void u64_v1AllTypes()
        {
            UInt64 expectedValue = 1234567891011121314;
            string columnName = "u64";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s8_v1AllTypes()
        {
            sbyte expectedValue = -123;
            string columnName = "s8";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s16_v1AllTypes()
        {
            short expectedValue = -12345;
            string columnName = "s16";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s32_v1AllTypes()
        {
            int expectedValue = -123456789;
            string columnName = "s32";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s64_v1AllTypes()
        {
            Int64 expectedValue = -1234567891011121314;
            string columnName = "s64";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void f32_v1AllTypes()
        {
            float expectedValue = 1.2345f;
            string columnName = "f32";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void bool_v1AllTypes()
        {
            bool expectedValue = false;
            string columnName = "bool";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void string_v1AllTypes()
        {
            string expectedValue = "this is a test string";
            string columnName = "string";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void table_v1AllTypes()
        {
            UInt64 expectedValue = 999999999;
            string columnName = "table";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            ArmpTable table = (ArmpTable)armp.MainTable.GetEntry("value").GetValueFromColumn(columnName);
            table.GetEntry(1).Name = "new_name";
            table.GetEntry(1).SetValueFromColumn("u64", expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            ArmpTable table_new = (ArmpTable)armp_new.MainTable.GetEntry("value").GetValueFromColumn(columnName);
            Assert.AreEqual(table_new.GetEntry(1).GetValueFromColumn("u64"), expectedValue);
            Assert.AreEqual(table_new.GetEntry(1).Name, "new_name");
        }


        [TestMethod]
        public void entryValidity_v1AllTypes()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").IsValid = false;
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.IsFalse(armp_new.MainTable.GetEntry("value").IsValid);
        }


        [TestMethod]
        public void entryIndex_v1AllTypes()
        {
            int expectedValue = 123;
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").Index = expectedValue;
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry("value").Index, expectedValue);
        }


        [TestMethod]
        public void entryName_v1AllTypes()
        {
            string expectedValue = "test_name";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").Name = expectedValue;
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(expectedValue).Name, expectedValue);
        }


        [TestMethod]
        public void entryFlags_v1AllTypes()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v1AllTypes);
            armp.MainTable.GetEntry("value").Flags[0] = true;
            armp.MainTable.GetEntry("value").Flags[1] = false;
            armp.MainTable.GetEntry("value").Flags[2] = true;
            armp.MainTable.GetEntry("value").Flags[3] = false;
            armp.MainTable.GetEntry("value").Flags[4] = true;
            armp.MainTable.GetEntry("value").Flags[5] = false;
            armp.MainTable.GetEntry("value").Flags[6] = true;
            armp.MainTable.GetEntry("value").Flags[7] = false;
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.IsTrue(armp_new.MainTable.GetEntry("value").Flags[0]);
            Assert.IsFalse(armp_new.MainTable.GetEntry("value").Flags[1]);
            Assert.IsTrue(armp_new.MainTable.GetEntry("value").Flags[2]);
            Assert.IsFalse(armp_new.MainTable.GetEntry("value").Flags[3]);
            Assert.IsTrue(armp_new.MainTable.GetEntry("value").Flags[4]);
            Assert.IsFalse(armp_new.MainTable.GetEntry("value").Flags[5]);
            Assert.IsTrue(armp_new.MainTable.GetEntry("value").Flags[6]);
            Assert.IsFalse(armp_new.MainTable.GetEntry("value").Flags[7]);
        }



        ///// V2 MODE 0 /////

        [TestMethod]
        public void WriteARMP_v2Mode0()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp.Version, armp_new.Version);
            Assert.AreEqual(armp.Revision, armp_new.Revision);
            Assert.AreEqual(armp.IsOldEngine, armp_new.IsOldEngine);
            Assert.AreEqual(armp.MainTable.TableInfo.StorageMode, armp_new.MainTable.TableInfo.StorageMode);
            CollectionAssert.AreEqual(armp.MainTable.GetRowNames(), armp_new.MainTable.GetRowNames());
            CollectionAssert.AreEqual(armp.MainTable.GetColumnNames(), armp_new.MainTable.GetColumnNames());
            Assert.AreEqual(armp.MainTable.GetEntry(1).GetValueFromColumn("string"), armp_new.MainTable.GetEntry(1).GetValueFromColumn("string"));
        }


        [TestMethod]
        public void u8_v2AllTypesMode0()
        {
            byte expectedValue = 123;
            string columnName = "u8_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void u16_v2AllTypesMode0()
        {
            UInt16 expectedValue = 12345;
            string columnName = "u16_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void u32_v2AllTypesMode0()
        {
            uint expectedValue = 123456789;
            string columnName = "u32_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void u64_v2AllTypesMode0()
        {
            UInt64 expectedValue = 1234567891011121314;
            string columnName = "u64_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s8_v2AllTypesMode0()
        {
            sbyte expectedValue = -123;
            string columnName = "s8_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s16_v2AllTypesMode0()
        {
            short expectedValue = -12345;
            string columnName = "s16_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s32_v2AllTypesMode0()
        {
            int expectedValue = -123456789;
            string columnName = "s32_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s64_v2AllTypesMode0()
        {
            Int64 expectedValue = -1234567891011121314;
            string columnName = "s64_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void f32_v2AllTypesMode0()
        {
            float expectedValue = 1.2345f;
            string columnName = "f32_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void f64_v2AllTypesMode0()
        {
            double expectedValue = 1.23456789;
            string columnName = "f64_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void bool_v2AllTypesMode0()
        {
            bool expectedValue = false;
            string columnName = "bool_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void string_v2AllTypesMode0()
        {
            string expectedValue = "this is a test string";
            string columnName = "string";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void table_v2AllTypesMode0()
        {
            byte expectedValue = 123;
            string columnName = "table";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            ArmpTable table = (ArmpTable)armp.MainTable.GetEntry("value").GetValueFromColumn(columnName);
            table.GetEntry(1).Name = "new_name";
            table.GetEntry(1).SetValueFromColumn("u8", expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            ArmpTable table_new = (ArmpTable)armp_new.MainTable.GetEntry("value").GetValueFromColumn(columnName);
            Assert.AreEqual(table_new.GetEntry(1).GetValueFromColumn("u8"), expectedValue);
            Assert.AreEqual(table_new.GetEntry(1).Name, "new_name");
        }


        [TestMethod]
        public void entryValidity_v2AllTypesMode0()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").IsValid = false;
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.IsFalse(armp_new.MainTable.GetEntry("value").IsValid);
        }


        [TestMethod]
        public void entryIndex_v2AllTypesMode0()
        {
            int expectedValue = 123;
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").Index = expectedValue;
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry("value").Index, expectedValue);
        }


        [TestMethod]
        public void entryName_v2AllTypesMode0()
        {
            string expectedValue = "test_name";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode0);
            armp.MainTable.GetEntry("value").Name = expectedValue;
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(expectedValue).Name, expectedValue);
        }



        ///// V2 MODE 1 /////

        [TestMethod]
        public void WriteARMP_v2Mode1()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp.Version, armp_new.Version);
            Assert.AreEqual(armp.Revision, armp_new.Revision);
            Assert.AreEqual(armp.IsOldEngine, armp_new.IsOldEngine);
            Assert.AreEqual(armp.MainTable.TableInfo.StorageMode, armp_new.MainTable.TableInfo.StorageMode);
            CollectionAssert.AreEqual(armp.MainTable.GetRowNames(), armp_new.MainTable.GetRowNames());
            CollectionAssert.AreEqual(armp.MainTable.GetColumnNames(), armp_new.MainTable.GetColumnNames());
            Assert.AreEqual(armp.MainTable.GetEntry(1).GetValueFromColumn("string"), armp_new.MainTable.GetEntry(1).GetValueFromColumn("string"));
        }


        [TestMethod]
        public void u8_v2AllTypesMode1()
        {
            byte expectedValue = 123;
            string columnName = "u8_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void u16_v2AllTypesMode1()
        {
            UInt16 expectedValue = 12345;
            string columnName = "u16_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void u32_v2AllTypesMode1()
        {
            uint expectedValue = 123456789;
            string columnName = "u32_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void u64_v2AllTypesMode1()
        {
            UInt64 expectedValue = 1234567891011121314;
            string columnName = "u64_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s8_v2AllTypesMode1()
        {
            sbyte expectedValue = -123;
            string columnName = "s8_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s16_v2AllTypesMode1()
        {
            short expectedValue = -12345;
            string columnName = "s16_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s32_v2AllTypesMode1()
        {
            int expectedValue = -123456789;
            string columnName = "s32_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void s64_v2AllTypesMode1()
        {
            Int64 expectedValue = -1234567891011121314;
            string columnName = "s64_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void f32_v2AllTypesMode1()
        {
            float expectedValue = 1.2345f;
            string columnName = "f32_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void f64_v2AllTypesMode1()
        {
            double expectedValue = 1.23456789;
            string columnName = "f64_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void bool_v2AllTypesMode1()
        {
            bool expectedValue = false;
            string columnName = "bool_";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void string_v2AllTypesMode1()
        {
            string expectedValue = "this is a test string";
            string columnName = "string";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").SetValueFromColumn(columnName, expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(1).GetValueFromColumn(columnName), expectedValue);
        }


        [TestMethod]
        public void table_v2AllTypesMode1()
        {
            byte expectedValue = 123;
            string columnName = "table";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            ArmpTable table = (ArmpTable)armp.MainTable.GetEntry("value").GetValueFromColumn(columnName);
            table.GetEntry(1).Name = "new_name";
            table.GetEntry(1).SetValueFromColumn("u8", expectedValue);
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            ArmpTable table_new = (ArmpTable)armp_new.MainTable.GetEntry("value").GetValueFromColumn(columnName);
            Assert.AreEqual(table_new.GetEntry(1).GetValueFromColumn("u8"), expectedValue);
            Assert.AreEqual(table_new.GetEntry(1).Name, "new_name");
        }


        [TestMethod]
        public void entryValidity_v2AllTypesMode1()
        {
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").IsValid = false;
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.IsFalse(armp_new.MainTable.GetEntry("value").IsValid);
        }


        [TestMethod]
        public void entryIndex_v2AllTypesMode1()
        {
            int expectedValue = 123;
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").Index = expectedValue;
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry("value").Index, expectedValue);
        }


        [TestMethod]
        public void entryName_v2AllTypesMode1()
        {
            string expectedValue = "test_name";
            ARMP armp = ArmpFileReader.ReadARMP(TestFiles.v2AllTypesMode1);
            armp.MainTable.GetEntry("value").Name = expectedValue;
            Stream stream = ArmpFileWriter.WriteARMPToStream(armp);
            ARMP armp_new = ArmpFileReader.ReadARMP(stream);
            Assert.AreEqual(armp_new.MainTable.GetEntry(expectedValue).Name, expectedValue);
        }
    }
}

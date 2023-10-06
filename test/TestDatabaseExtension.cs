using System;
using System.Linq;
#if GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
#else
using GrxCAD.DatabaseServices;
#endif
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestDatabaseExtension
    {
        [Test]
        public void TestGetTopTransaction()
        {
            using (var trans = Active.StartTransaction())
            {
                Assert.AreEqual(Active.Database.GetTopTransaction(), trans);
            }
        }

        [Test]
        public void TestGetNamedObjectsDictionary()
        {
            using (var trans = Active.StartTransaction())
            {
                Assert.AreEqual(Active.Database.GetNamedObjectsDictionary(),
                    trans.GetObject(Active.Database.NamedObjectsDictionaryId, OpenMode.ForRead));
            }
        }

        [Test]
        public void TestGetModelSpace()
        {
            using (var trans = Active.StartTransaction())
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                Assert.NotNull(blockTable);
                var blockTableRecord = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                Assert.AreEqual(Active.Database.GetModelSpace(), blockTableRecord);
            }
        }

        [Test]
        public void TestGetCurrentSpace()
        {
            using (var trans = Active.StartTransaction())
            {
                Assert.AreEqual(Active.Database.GetCurrentSpace(),
                    trans.GetObject(Active.Database.CurrentSpaceId, OpenMode.ForRead));
            }
        }

        [Test]
        public void TestGetLayouts()
        {
            using (Active.StartTransaction())
            {
                Assert.GreaterOrEqual(Active.Database.GetLayouts(false).Count(), 2);
                Assert.GreaterOrEqual(Active.Database.GetLayouts().Count(), 1);
            }
        }

        [Test]
        public void TestGetLayoutNames()
        {
            using (Active.StartTransaction())
            {
                Assert.AreEqual(Active.Database.GetLayoutNames(false)
                    .Count(x => string.Equals("Model", x, StringComparison.OrdinalIgnoreCase)), 1);
            }
        }

        [Test]
        public void TestGetLayoutBlockTableRecords()
        {
            using (Active.StartTransaction())
            {
                var records = Active.Database.GetLayoutBlockTableRecords(false).ToList();
                Assert.AreNotEqual(records.FindIndex(x =>
                    string.Equals(BlockTableRecord.ModelSpace, x.Name, StringComparison.OrdinalIgnoreCase)), -1);
                Assert.AreNotEqual(records.FindIndex(x =>
                    string.Equals(BlockTableRecord.PaperSpace, x.Name, StringComparison.OrdinalIgnoreCase)), -1);

                records = Active.Database.GetLayoutBlockTableRecords().ToList();
                Assert.AreEqual(records.FindIndex(x =>
                    string.Equals(BlockTableRecord.ModelSpace, x.Name, StringComparison.OrdinalIgnoreCase)), -1);
                Assert.IsTrue(records.All(x =>
                    x.Name.StartsWith(BlockTableRecord.PaperSpace, StringComparison.OrdinalIgnoreCase)));
            }
        }
    }
}

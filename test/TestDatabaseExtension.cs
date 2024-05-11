using System;
using System.Collections.Generic;
using System.Linq;
#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
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

        [Test]
        public void TestGetCustomProperty()
        {
            var db = Active.Database;
            string noneExisting = db.GetCustomProperty("none_existing");
            Assert.IsNull(noneExisting);

            var builder = new DatabaseSummaryInfoBuilder(db.SummaryInfo);
            var table = builder.CustomPropertyTable;
            try
            {
                table.Add("prop1", "");
                table.Add("prop2", "prop2_val");
                db.SummaryInfo = builder.ToDatabaseSummaryInfo();

                string prop1 = db.GetCustomProperty("prop1");
                Assert.AreEqual(string.Empty, prop1);

                string prop2 = db.GetCustomProperty("prop2");
                Assert.AreEqual("prop2_val", prop2);
            }
            finally
            {
                table.Clear();
                db.SummaryInfo = builder.ToDatabaseSummaryInfo();
            }
        }

        [Test]
        public void TestGetCustomProperties()
        {
            var db = Active.Database;
            var all = db.GetCustomProperties();
            Assert.IsEmpty(all);

            var builder = new DatabaseSummaryInfoBuilder(db.SummaryInfo);
            var table = builder.CustomPropertyTable;
            try
            {
                table.Add("prop1", "");
                table.Add("prop2", "prop2_val");
                db.SummaryInfo = builder.ToDatabaseSummaryInfo();

                all = db.GetCustomProperties();

                Assert.AreEqual(2, all.Count);
                Assert.IsTrue(all.ContainsKey("prop1"));
                Assert.AreEqual("prop2_val", all["prop2"]);
            }
            finally
            {
                table.Clear();
                db.SummaryInfo = builder.ToDatabaseSummaryInfo();
            }
        }

        [Test]
        public void TestSetCustomProperty()
        {
            var db = Active.Database;
            var old = new DatabaseSummaryInfoBuilder(db.SummaryInfo).ToDatabaseSummaryInfo();

            try
            {
                db.SetCustomProperty("new_prop", "new_val");
                Assert.AreEqual("new_val",
                    new DatabaseSummaryInfoBuilder(db.SummaryInfo).CustomPropertyTable["new_prop"]);

                db.SetCustomProperty("new_prop", "another_val");
                Assert.AreEqual("another_val",
                    new DatabaseSummaryInfoBuilder(db.SummaryInfo).CustomPropertyTable["new_prop"]);
            }
            finally
            {
                db.SummaryInfo = old;
            }
        }

        [Test]
        public void TestSetCustomProperties()
        {
            var db = Active.Database;
            var old = new DatabaseSummaryInfoBuilder(db.SummaryInfo).ToDatabaseSummaryInfo();

            try
            {
                Assert.Catch<ArgumentNullException>(() => db.SetCustomProperties(null));
                Assert.DoesNotThrow(() => db.SetCustomProperties());

                db.SetCustomProperties(new KeyValuePair<string, string>("prop1", "val1"));
                Assert.AreEqual(1, new DatabaseSummaryInfoBuilder(db.SummaryInfo).CustomPropertyTable.Count);

                db.SetCustomProperties(new KeyValuePair<string, string>("prop2", "val2"),
                    new KeyValuePair<string, string>("prop3", "val3"));
                Assert.AreEqual(3, new DatabaseSummaryInfoBuilder(db.SummaryInfo).CustomPropertyTable.Count);

                db.SetCustomProperties(new KeyValuePair<string, string>("prop1", "replaced_val1"),
                    new KeyValuePair<string, string>("prop3", "replaced_val3"));
                var table = new DatabaseSummaryInfoBuilder(db.SummaryInfo).CustomPropertyTable;
                Assert.AreEqual(3, table.Count);
                Assert.AreEqual(new[] { "replaced_val1", "val2", "replaced_val3" },
                    new[] { table["prop1"], table["prop2"], table["prop3"] });
            }
            finally
            {
                db.SummaryInfo = old;
            }
        }
    }
}

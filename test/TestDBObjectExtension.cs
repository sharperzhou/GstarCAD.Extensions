using System;
using System.Linq;
using GrxCAD.DatabaseServices;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestDBObjectExtension
    {
        [Test]
        public void TestTryGetExtensionDictionary()
        {
            using (var trans = Active.StartTransaction())
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                Assert.NotNull(blockTable);
                Assert.IsFalse(blockTable.TryGetExtensionDictionary(out var nullExtensionDictionary));
                Assert.IsNull(nullExtensionDictionary);

                blockTable.CreateExtensionDictionary();
                Assert.IsTrue(blockTable.TryGetExtensionDictionary(out var validExtensionDictionary));
                Assert.NotNull(validExtensionDictionary);
            }
        }

        [Test]
        public void TestGetOrCreateExtensionDictionary()
        {
            using (var trans = Active.StartTransaction())
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                Assert.NotNull(blockTable);
                var dictionary = blockTable.GetOrCreateExtensionDictionary(OpenMode.ForWrite);
                Assert.IsNotNull(dictionary);
                Assert.IsTrue(dictionary.IsWriteEnabled);
            }
        }

        [Test]
        public void TestSetXDataForApplication()
        {
            using (Active.StartTransaction())
            {
                var circle = new Circle();
                Active.Database.GetModelSpace(OpenMode.ForWrite).AppendEntity(circle);
                Assert.IsTrue(circle.ObjectId.IsValid);

                circle.SetXDataForApplication(new ResultBuffer(
                    new TypedValue(1001, "application1"),
                    new TypedValue(1040, 12)));
                Assert.NotNull(circle.GetXDataForApplication("application1"));

                circle.SetXDataForApplication("application2", new TypedValue(1000, "abc"));
                var array = circle.XData.AsArray();
                Assert.NotNull(array.FirstOrDefault(x =>
                    string.Equals(x.Value.ToString(), "application1", StringComparison.OrdinalIgnoreCase)));
                Assert.NotNull(array.FirstOrDefault(x =>
                    string.Equals(x.Value.ToString(), "application2", StringComparison.OrdinalIgnoreCase)));

                // remove xdata
                circle.SetXDataForApplication("application1");
                Assert.IsNull(circle.GetXDataForApplication("application1"));

                // modify xdata
                circle.SetXDataForApplication("application2",
                    new TypedValue(1005, new Handle()),
                    new TypedValue(1000, "new_val"));
                Assert.AreEqual(circle.GetXDataForApplication("application2").AsArray().Length, 3);
            }
        }

        [Test]
        public void TestUpgradeWrite()
        {
            using (var trans = Active.StartTransaction())
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                Assert.NotNull(blockTable);

                Assert.IsTrue(blockTable.UpgradeWrite().IsWriteEnabled);
            }
        }
    }
}

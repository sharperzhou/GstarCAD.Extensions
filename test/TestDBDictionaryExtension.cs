using System.Linq;
using GrxCAD.DatabaseServices;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestDBDictionaryExtension
    {
        [Test]
        public void TestTryGetObjectAndNamedDictionary()
        {
            using (var trans = Active.StartTransaction())
            {
                var namedDictionary =
                    trans.GetObject(Active.Database.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                Assert.NotNull(namedDictionary);
                Assert.IsFalse(namedDictionary.TryGetNamedDictionary("not_exist", out var subDictionary));
                Assert.IsNull(subDictionary);

                var circle = new Circle(default, default, 100);
                Active.Database.GetModelSpace(OpenMode.ForWrite).AppendEntity(circle);
                circle.CreateExtensionDictionary();
                var extensionDictionary =
                    trans.GetObject(circle.ExtensionDictionary, OpenMode.ForWrite) as DBDictionary;
                Assert.NotNull(extensionDictionary);
                using (var xrecord = new Xrecord { Data = new ResultBuffer(new TypedValue(1000, "hello")) })
                {
                    extensionDictionary.SetAt("new_xrecord", xrecord);
                }

                extensionDictionary.DowngradeOpen();

                Assert.IsTrue(extensionDictionary.TryGetObject("new_xrecord", out Xrecord record));
                Assert.NotNull(record);
                Assert.AreEqual(record.Data.Cast<TypedValue>().First(x => x.TypeCode == 1000).Value, "hello");
            }
        }

        [Test]
        public void TestGetObjects()
        {
            using (var trans = Active.StartTransaction())
            {
                var namedDictionary =
                    trans.GetObject(Active.Database.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                Assert.NotNull(namedDictionary);
                foreach (DBDictionary dictionary in namedDictionary.GetObjects<DBDictionary>())
                {
                    Assert.NotNull(dictionary);
                }
            }
        }

        [Test]
        public void TestGetOrCreateNamedDictionary()
        {
            using (var trans = Active.StartTransaction())
            {
                var namedDictionary =
                    trans.GetObject(Active.Database.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                Assert.NotNull(namedDictionary);
                Assert.NotNull(namedDictionary.GetOrCreateNamedDictionary("not_exist_dictionary"));
            }
        }

        [Test]
        public void TestGetXrecordData()
        {
            using (var trans = Active.StartTransaction())
            {
                var namedDictionary =
                    trans.GetObject(Active.Database.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                Assert.NotNull(namedDictionary);
                Assert.IsNull(namedDictionary.GetXrecordData("not_exist_dictionary"));

                namedDictionary.UpgradeOpen();
                namedDictionary.SetAt("new_record",
                    new Xrecord { Data = new ResultBuffer(new TypedValue(1000, "sharper")) });
                Assert.NotNull(namedDictionary.GetXrecordData("new_record"));
            }
        }

        [Test]
        public void TestSetXrecordData()
        {
            using (var trans = Active.StartTransaction())
            {
                var namedDictionary =
                    trans.GetObject(Active.Database.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                Assert.NotNull(namedDictionary);

                const string key = "new_record";
                namedDictionary.SetXrecordData(key, new ResultBuffer(new TypedValue(1000, "test")));
                Assert.NotNull(namedDictionary[key]);

                namedDictionary.SetXrecordData(key, new TypedValue(1000, "new_val"));
                var record = trans.GetObject(namedDictionary.GetAt(key), OpenMode.ForRead) as Xrecord;
                Assert.NotNull(record);
                Assert.NotNull(record.Data);
                Assert.AreEqual(record.Data.Cast<TypedValue>().First(x => x.TypeCode == 1000).Value, "new_val");
            }
        }
    }
}

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
    public class TestEnumerableExtension
    {
        [Test]
        public void TestGetObjects()
        {
            using (var trans = Active.StartTransaction())
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                Assert.NotNull(blockTable);

                var ids = new List<ObjectId>();
                foreach (ObjectId objectId in blockTable)
                {
                    ids.Add(objectId);
                }

                Assert.GreaterOrEqual(ids.GetObjects<BlockTableRecord>().Count(), 2);
            }
        }

        [Test]
        public void TestUpgradeWrite()
        {
            using (var trans = Active.StartTransaction())
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                Assert.NotNull(blockTable);

                var blockTableRecords = blockTable.Cast<ObjectId>().GetObjects<BlockTableRecord>().UpgradeWrite();
                Assert.IsTrue(blockTableRecords.All(x => x.IsWriteEnabled));
            }
        }

        [Test]
        public void TestDisposeAll()
        {
            using (var trans = Active.StartTransaction())
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                Assert.NotNull(blockTable);

                var blockTableRecords = blockTable.Cast<ObjectId>().GetObjects<BlockTableRecord>().ToList();

                blockTableRecords.DisposeAll();

                Assert.IsTrue(blockTableRecords.All(x => x.IsDisposed));
            }
        }
    }
}

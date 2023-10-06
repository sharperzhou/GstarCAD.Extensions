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
    public class TestObjectIdCollectionExtension
    {
        [Test]
        public void TestGetObjects()
        {
            using (var trans = Active.StartTransaction())
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                Assert.NotNull(blockTable);

                ObjectIdCollection collection = new ObjectIdCollection(blockTable.Cast<ObjectId>().ToArray());


                var blockTableRecords = collection.GetObjects<BlockTableRecord>().ToList();
                Assert.GreaterOrEqual(blockTableRecords.Count, 2);

                Assert.IsTrue(blockTableRecords.All(x => !x.IsWriteEnabled));
            }
        }
    }
}

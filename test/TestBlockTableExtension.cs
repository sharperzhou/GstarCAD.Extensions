using GrxCAD.DatabaseServices;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestBlockTableExtension
    {
        [Test]
        public void TestGetBlockDefinition()
        {
            using (var trans = Active.StartTransaction())
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                var modelSpaceId = blockTable.GetBlockDefinition(BlockTableRecord.ModelSpace);
                Assert.AreEqual(modelSpaceId, blockTable?[BlockTableRecord.ModelSpace]);

                // External block in GstarCAD/ExtendCmd folder
                var externalId = blockTable.GetBlockDefinition("_dimzb1");
                Assert.IsTrue(externalId.IsValid);
                Assert.IsTrue(blockTable.Has(externalId));
            }
        }
    }
}

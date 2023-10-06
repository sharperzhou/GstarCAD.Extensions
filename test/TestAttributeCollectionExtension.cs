using System.Linq;
#if GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
#endif
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestAttributeCollectionExtension
    {
        private readonly RandomBlockDefinition _randomBlockDefinition = new RandomBlockDefinition();

        [SetUp]
        public void Setup()
        {
            _randomBlockDefinition.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            _randomBlockDefinition.TearDown();
        }

        [Test]
        public void TestEmptyAttributeReference()
        {
            using (var blockReference = new BlockReference(Point3d.Origin, _randomBlockDefinition.BlockDefinitionId))
            {
                var objects = blockReference.AttributeCollection.GetObjects();
                Assert.NotNull(objects);
            }
        }

        [Test]
        public void TestAttributeReferences()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            {
                var blockDefinition =
                    trans.GetObject(_randomBlockDefinition.BlockDefinitionId, OpenMode.ForRead) as BlockTableRecord;
                Assert.NotNull(blockDefinition);

                #region Block reference

                using (var blockReference = new BlockReference(new Point3d(100, 100, 0), blockDefinition.ObjectId))
                {
                    foreach (ObjectId id in blockDefinition)
                    {
                        var attributeDefinition = trans.GetObject(id, OpenMode.ForRead) as AttributeDefinition;
                        if (attributeDefinition == null) continue;

                        var attributeReference = new AttributeReference();
                        attributeReference.SetAttributeFromBlock(attributeDefinition, blockReference.BlockTransform);
                        blockReference.AttributeCollection.AppendAttribute(attributeReference);
                        trans.AddNewlyCreatedDBObject(attributeReference, true);
                    }

                    var modelSpace =
                        trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database),
                            OpenMode.ForWrite) as BlockTableRecord;
                    Assert.NotNull(modelSpace);
                    modelSpace.AppendEntity(blockReference);
                    trans.AddNewlyCreatedDBObject(blockReference, true);

                    Assert.NotNull(blockReference);
                    var objects = blockReference.AttributeCollection.GetObjects();
                    Assert.NotNull(objects);
                    Assert.AreEqual(objects.Count(), _randomBlockDefinition.AttributeCount);

                    trans.Commit();
                }

                #endregion
            }
        }
    }
}

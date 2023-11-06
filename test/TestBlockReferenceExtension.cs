using System;
using System.Collections.Generic;
using System.Linq;
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using GrxCAD.Runtime;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;
using Exception = GrxCAD.Runtime.Exception;

namespace GstarCAD.Extensions.Test
{
    public class TestBlockReferenceExtension
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
        public void TestGetEffectiveName()
        {
            using (var blockReference =
                   new BlockReference(new Point3d(200, 200, 0), _randomBlockDefinition.BlockDefinitionId))
            using (Active.Database.TransactionManager.StartTransaction())
            {
                // TODO: How to create dynamic block?
                var ex = Assert.Catch<Exception>(() => blockReference.GetEffectiveName());
                Assert.AreEqual(ex?.ErrorStatus, ErrorStatus.NullObjectId);
            }
        }

        [Test]
        public void TestGetAttributePairs()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            using (var blockReference = new BlockReference(default, _randomBlockDefinition.BlockDefinitionId))
            {
                var modelSpace =
                    trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database), OpenMode.ForWrite) as
                        BlockTableRecord;
                Assert.NotNull(modelSpace);
                modelSpace.AppendEntity(blockReference);

                var blockDefinition =
                    trans.GetObject(_randomBlockDefinition.BlockDefinitionId, OpenMode.ForRead) as BlockTableRecord;
                Assert.NotNull(blockDefinition);
                foreach (ObjectId id in blockDefinition)
                {
                    var attributeDefinition = trans.GetObject(id, OpenMode.ForRead) as AttributeDefinition;
                    if (attributeDefinition == null) continue;

                    var attributeReference = new AttributeReference();
                    attributeReference.SetAttributeFromBlock(attributeDefinition, blockReference.BlockTransform);
                    blockReference.AttributeCollection.AppendAttribute(attributeReference);
                }

                var pairs = blockReference.GetAttributePairs().ToList();
                Assert.AreEqual(pairs.Count, _randomBlockDefinition.AttributeCount);
                Assert.IsFalse(pairs.Any(p => p.Value == null));

                blockReference.AttributeCollection.AppendAttribute(
                    new AttributeReference(default, "", "extra_tag", default));
                pairs = blockReference.GetAttributePairs().ToList();
                Assert.IsTrue(pairs.Count(p => p.Value == null) == 1);
                Assert.IsTrue(pairs.First(p => p.Value == null).Key.Id ==
                              blockReference.AttributeCollection[blockReference.AttributeCollection.Count - 1]);
            }
        }

        [Test]
        public void TestGetAttributesByTag()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            using (var blockReference = new BlockReference(default, _randomBlockDefinition.BlockDefinitionId))
            {
                var modelSpace =
                    trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database), OpenMode.ForWrite) as
                        BlockTableRecord;
                Assert.NotNull(modelSpace);
                modelSpace.AppendEntity(blockReference);

                Assert.AreEqual(blockReference.GetAttributesByTag().Count(), 0);

                AttributeReference[] attributeReferences =
                {
                    new AttributeReference(), new AttributeReference(), new AttributeReference(),
                    new AttributeReference(), new AttributeReference(),
                };
                foreach (AttributeReference attributeReference in attributeReferences)
                {
                    blockReference.AttributeCollection.AppendAttribute(attributeReference);
                }

                var zip = blockReference.GetAttributesByTag().Select(x => x.Value)
                    .Zip(attributeReferences, (x, y) => x.Id == y.Id);
                Assert.True(zip.All(x => x));
            }
        }

        [Test]
        public void TestGetAttributeValues()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            using (var blockReference = new BlockReference(default, _randomBlockDefinition.BlockDefinitionId))
            {
                var modelSpace =
                    trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database), OpenMode.ForWrite) as
                        BlockTableRecord;
                modelSpace?.AppendEntity(blockReference);

                Assert.AreEqual(blockReference.GetAttributeValues().Count, 0);

                blockReference.AttributeCollection.AppendAttribute(
                    new AttributeReference(default, "val1", "tag1", default));
                Assert.AreEqual(blockReference.GetAttributeValues()["tag1"], "val1");
            }
        }

        [Test]
        public void TestSetAttributeValue()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            using (var blockReference = new BlockReference(default, _randomBlockDefinition.BlockDefinitionId))
            {
                var modelSpace =
                    trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database), OpenMode.ForWrite) as
                        BlockTableRecord;
                modelSpace?.AppendEntity(blockReference);

                blockReference.AttributeCollection.AppendAttribute(
                    new AttributeReference(default, "val1", "tag1", default));
                Assert.AreEqual(blockReference.SetAttributeValue("tag1", "111"), "111");
                Assert.IsNull(blockReference.SetAttributeValue("not_exist", "new_val"));
            }
        }

        [Test]
        public void TestSetAttributeValues()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            using (var blockReference = new BlockReference(default, _randomBlockDefinition.BlockDefinitionId))
            {
                var modelSpace =
                    trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database), OpenMode.ForWrite) as
                        BlockTableRecord;
                modelSpace?.AppendEntity(blockReference);

                var attributes = new[]
                {
                    new AttributeReference(), new AttributeReference(default, null, "tag1", default),
                    new AttributeReference(default, "", "", default),
                    new AttributeReference(default, "1", "tag2", default),
                };

                foreach (AttributeReference attributeReference in attributes)
                {
                    blockReference.AttributeCollection.AppendAttribute(attributeReference);
                }

                blockReference.SetAttributeValues(new Dictionary<string, string>
                {
                    { "", "empty_tag_val" }, { "tag1", "val1" }, { "tag2", "val2" }
                });

                Assert.AreEqual(attributes[0].TextString, "empty_tag_val");
                Assert.AreEqual(attributes[1].TextString, "val1");
                Assert.AreEqual(attributes[2].TextString, "empty_tag_val");
                Assert.AreEqual(attributes[3].TextString, "val2");
            }
        }

        [Test]
        public void TestAddAttributeReferences()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            using (var blockReference = new BlockReference(default, _randomBlockDefinition.BlockDefinitionId))
            {
                var modelSpace =
                    trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database), OpenMode.ForWrite) as
                        BlockTableRecord;
                modelSpace?.AppendEntity(blockReference);

                var ret = blockReference.AddAttributeReferences(
                    new Dictionary<string, string> { { "Tag_2", "new_val2" }, { "no_exist", "no_exist_val" } });

                Assert.AreEqual(ret.Count, _randomBlockDefinition.AttributeCount);
                Assert.AreEqual(ret["Tag_2"].TextString, "new_val2");
                Assert.IsFalse(ret.ContainsKey("no_exist"));
            }
        }

        [Test]
        public void TestGetOrSetDynamicPropertyAndValue()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            using (var blockReference = new BlockReference(default, _randomBlockDefinition.BlockDefinitionId))
            {
                var modelSpace =
                    trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database), OpenMode.ForWrite) as
                        BlockTableRecord;
                modelSpace?.AppendEntity(blockReference);

                // TODO: How to create dynamic block?
                const string mocPropertyName = "dynamic_name";
                Assert.IsNull(blockReference.GetDynamicProperty(mocPropertyName));
                Assert.IsNull(blockReference.GetDynamicPropertyValue(mocPropertyName));

                blockReference.SetDynamicPropertyValue(mocPropertyName, "new_value");
                var ex = Assert.Throws<ArgumentNullException>(() =>
                    blockReference.SetDynamicPropertyValue(mocPropertyName, null));
                Assert.NotNull(ex);
                var ex2 = Assert.Throws<ArgumentException>(() => blockReference.SetDynamicPropertyValue(" ", ""));
                Assert.IsTrue(ex2?.Message.StartsWith("eNullOrWhiteSpace"));
            }
        }

        [Test]
        public void TestMirror()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            using (var blockReference = new BlockReference(default, _randomBlockDefinition.BlockDefinitionId))
            {
                var modelSpace =
                    trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database), OpenMode.ForWrite) as
                        BlockTableRecord;
                modelSpace?.AppendEntity(blockReference);

                var blockDefinition =
                    trans.GetObject(_randomBlockDefinition.BlockDefinitionId, OpenMode.ForRead) as BlockTableRecord;
                Assert.NotNull(blockDefinition);
                foreach (ObjectId id in blockDefinition)
                {
                    var attributeDefinition = trans.GetObject(id, OpenMode.ForRead) as AttributeDefinition;
                    if (attributeDefinition == null) continue;

                    using (var attributeReference = new AttributeReference())
                    {
                        attributeReference.SetAttributeFromBlock(attributeDefinition, blockReference.BlockTransform);
                        blockReference.AttributeCollection.AppendAttribute(attributeReference);
                    }
                }

                var newBlockReferenceId =
                    blockReference.Mirror(new Line3d(new Point3d(0, 30, 0), new Point3d(30, 0, 0)), false);
                Assert.AreNotEqual(newBlockReferenceId, blockReference.Id);

                var selfId = blockReference.Mirror(new Line3d(new Point3d(50, 0, 0), Vector3d.YAxis), true);
                Assert.AreEqual(selfId, blockReference.Id);

                trans.Commit();
            }
        }
    }
}

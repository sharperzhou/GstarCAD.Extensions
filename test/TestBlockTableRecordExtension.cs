using System;
using System.Collections.Generic;
using System.Linq;
#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.Runtime;
using Exception = Gssoft.Gscad.Runtime.Exception;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using GrxCAD.Runtime;
using Exception = GrxCAD.Runtime.Exception;
#endif
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestBlockTableRecordExtension
    {
        [Test]
        public void TestGetObjects()
        {
            using (var trans = Active.StartTransaction())
            {
                var modelSpace = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database),
                    OpenMode.ForWrite) as BlockTableRecord;
                Assert.NotNull(modelSpace);
                using (var text = new DBText { TextString = "text" })
                using (var attributeDefinition = new AttributeDefinition { TextString = "att" })
                {
                    modelSpace.AppendEntity(text);
                    modelSpace.AppendEntity(attributeDefinition);
                }

                Assert.AreEqual(modelSpace.GetObjects<Entity>().Count(), 2);
                Assert.AreEqual(modelSpace.GetObjects<DBText>().Count(), 2);
                Assert.AreEqual(modelSpace.GetObjects<AttributeDefinition>().Count(), 1);
                Assert.AreEqual(modelSpace.GetObjects<Curve>().Count(), 0);

                Assert.AreEqual(modelSpace.GetObjects<DBText>(matchExact: true).Count(), 1);
                Assert.AreEqual(modelSpace.GetObjects<Entity>(matchExact: true).Count(), 0);
            }
        }

        [Test]
        public void TestAdd()
        {
            using (var trans = Active.StartTransaction())
            using (var blockDefinition = new BlockTableRecord() { Name = Guid.NewGuid().ToString("N") })
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                Assert.NotNull(blockTable);
                blockTable.Add(blockDefinition);

                Assert.True(blockDefinition.Add(new Line(default, new Point3d(100, 100, 0))).IsValid);
                Assert.AreEqual(blockDefinition.Add(
                    new Circle(),
                    new Arc(),
                    new DBText()
                ).Count, 3);

                var list = new List<Entity> { new Ellipse(), new Hatch(), new Leader(), new Polyline(3) };
                Assert.AreEqual(blockDefinition.Add(list).Count, list.Count);

                var array = new Entity[] { new Circle(), new Solid3d() };
                Assert.AreEqual(blockDefinition.Add(array).Count, array.Length);
            }
        }

        [Test]
        public void TestInsertBlockFrom()
        {
            using (var trans = Active.StartTransaction())
            {
                var modelSpace = trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Active.Database),
                    OpenMode.ForWrite) as BlockTableRecord;
                Assert.NotNull(modelSpace);

                var ex = Assert.Catch<Exception>(() => modelSpace.InsertBlockFrom("not_exist", Point3d.Origin));
                Assert.AreEqual(ex?.ErrorStatus, ErrorStatus.NullObjectId);

                // External block in GstarCAD/ExtendCmd folder
                var blockReferenceFromExternal = modelSpace.InsertBlockFrom("_dimzb1", default);
                Assert.NotNull(blockReferenceFromExternal);

                // Test again, external block has became an internal block
                var blockReferenceFromInternal = modelSpace.InsertBlockFrom("_dimzb1", default);
                Assert.NotNull(blockReferenceFromInternal);
            }
        }

        private IList<ObjectId> MakeBlockReferences(RandomBlockDefinition randomBlockDefinition)
        {
            using (Transaction trans = Active.StartTransaction())
            {
                BlockTableRecord modelSpace = trans.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(Active.Database),
                    OpenMode.ForWrite) as BlockTableRecord;
                Assert.NotNull(modelSpace);


                BlockTableRecord blockDefinition =
                    trans.GetObject(randomBlockDefinition.BlockDefinitionId, OpenMode.ForRead) as BlockTableRecord;
                Assert.NotNull(blockDefinition);

                Random random = new Random();
                var ret = new List<ObjectId>();
                for (int i = 0, n = random.Next(3, 6); i < n; i++)
                {
                    using (BlockReference blockReference =
                           new BlockReference(default, randomBlockDefinition.BlockDefinitionId))
                    {
                        ret.Add(modelSpace.AppendEntity(blockReference));
                        trans.AddNewlyCreatedDBObject(blockReference, true);
                        foreach (ObjectId id in blockDefinition)
                        {
                            AttributeDefinition attributeDefinition =
                                trans.GetObject(id, OpenMode.ForWrite) as AttributeDefinition;
                            if (attributeDefinition == null) continue;

                            using (AttributeReference attributeReference = new AttributeReference())
                            {
                                attributeReference.SetAttributeFromBlock(attributeDefinition,
                                    blockReference.BlockTransform);
                                attributeReference.TextString = random.Next(100, 1000).ToString();
                                blockReference.AttributeCollection.AppendAttribute(attributeReference);
                                trans.AddNewlyCreatedDBObject(attributeReference, true);
                            }
                        }
                    }
                }

                trans.Commit();

                return ret;
            }
        }

        [Test]
        public void TestSynchronizeAttributes()
        {
            RandomBlockDefinition randomBlockDefinition = new RandomBlockDefinition();
            try
            {
                randomBlockDefinition.Setup();
                var blockReference = MakeBlockReferences(randomBlockDefinition);
                Assert.Greater(blockReference.Count, 0);
                using (var trans = Active.StartTransaction())
                {
                    var blockDefinition =
                        trans.GetObject(randomBlockDefinition.BlockDefinitionId, OpenMode.ForRead) as BlockTableRecord;
                    blockDefinition.SynchronizeAttributes();
                }
            }
            finally
            {
                randomBlockDefinition.TearDown();
            }
        }
    }
}

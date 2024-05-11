using System;
#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
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
    internal class RandomBlockDefinition
    {
        public ObjectId BlockDefinitionId { get; private set; } = ObjectId.Null;

        public string BlockDefinitionName { get; } = Guid.NewGuid().ToString("N");

        public int AttributeCount { get; } = new Random().Next(5, 20);

        public void Setup()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            using (var blockDefinition = new BlockTableRecord() { Name = BlockDefinitionName })
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                Assert.NotNull(blockTable);

                #region Block definition

                for (int i = 0; i < AttributeCount; i++)
                {
                    using (var attributeDefinition = new AttributeDefinition(Point3d.Origin,
                               $"Value_{i + 1}", $"Tag_{i + 1}", $"Prompt_{i + 1}",
                               Active.Database.Textstyle))
                    {
                        blockDefinition.AppendEntity(attributeDefinition);
                        trans.AddNewlyCreatedDBObject(attributeDefinition, true);
                    }
                }

                BlockDefinitionId = blockTable.Add(blockDefinition);
                trans.AddNewlyCreatedDBObject(blockDefinition, true);

                #endregion

                trans.Commit();
            }
        }

        public void TearDown()
        {
            using (var trans = Active.Database.TransactionManager.StartTransaction())
            {
                var blockTable = trans.GetObject(Active.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                Assert.NotNull(blockTable);

                var blockDefinition =
                    trans.GetObject(BlockDefinitionId, OpenMode.ForWrite) as BlockTableRecord;
                Assert.NotNull(blockDefinition);

                foreach (ObjectId id in blockDefinition.GetBlockReferenceIds(true, true))
                {
                    trans.GetObject(id, OpenMode.ForWrite)?.Erase(true);
                }

                blockDefinition.Erase(true);

                BlockDefinitionId = ObjectId.Null;

                trans.Commit();
            }
        }
    }
}

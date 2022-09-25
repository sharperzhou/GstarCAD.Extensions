using System.Collections.Generic;
using System.Linq;
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using GrxCAD.Runtime;

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the BlockTableRecord type.
    /// </summary>
    public static class BlockTableRecordExtension
    {
        /// <summary>
        /// Opens the entities which type matches to the given one, and return them.
        /// </summary>
        /// <typeparam name="T">Type of objects to return.</typeparam>
        /// <param name="blockTableRecord">Block table record.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayers">Value indicating if locked layers should be opened.</param>
        /// <returns>The sequence of opened objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="blockTableRecord"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static IEnumerable<T> GetObjects<T>(
          this BlockTableRecord blockTableRecord,
          OpenMode mode = OpenMode.ForRead,
          bool openErased = false,
          bool forceOpenOnLockedLayers = false) where T : Entity
        {
            Throwable.ThrowIfArgumentNull(blockTableRecord, nameof(blockTableRecord));
            var tr = blockTableRecord.Database.GetTopTransaction();
            BlockTableRecord source = openErased ? blockTableRecord.IncludingErased : blockTableRecord;
            if (typeof(T) == typeof(Entity))
            {
                foreach (ObjectId id in source)
                {
                    yield return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
                }
            }
            else
            {
                var rxClass = RXObject.GetClass(typeof(T));
                foreach (ObjectId id in source)
                {
                    if (id.ObjectClass.IsDerivedFrom(rxClass))
                    {
                        yield return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
                    }
                }
            }
        }

        /// <summary>
        /// Appends the entities to the BlockTableRecord.
        /// </summary>
        /// <param name="owner">Instance to which the method applies.</param>
        /// <param name="entities">Sequence of entities.</param>
        /// <returns>The collection of added entities ObjectId.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="owner"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="entities"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active Transaction.</exception>
        public static ObjectIdCollection Add(this BlockTableRecord owner, IEnumerable<Entity> entities)
        {
            Throwable.ThrowIfArgumentNull(owner, nameof(owner));
            Throwable.ThrowIfArgumentNull(entities, nameof(entities));
            var tr = owner.Database.GetTopTransaction();
            var ids = new ObjectIdCollection();
            using (var entitySet = new DisposableSet<Entity>(entities))
            {
                foreach (Entity ent in entitySet)
                {
                    ids.Add(owner.AppendEntity(ent));
                    tr.AddNewlyCreatedDBObject(ent, true);
                }
            }
            return ids;
        }

        /// <summary>
        /// Appends the entities to the BlockTableRecord.
        /// </summary>
        /// <param name="owner">Instance to which the method applies.</param>
        /// <param name="entities">Collection of entities.</param>
        /// <returns>The collection of added entities ObjectId.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="owner"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="entities"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active Transaction.</exception>
        public static ObjectIdCollection AddRange(this BlockTableRecord owner, params Entity[] entities)
        {
            return owner.Add(entities);
        }

        /// <summary>
        /// Appends the entity to the BlockTableRecord.
        /// </summary>
        /// <param name="owner">Instance to which the method applies.</param>
        /// <param name="entity">Entity to add.</param>
        /// <returns>The ObjectId of added entity.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="owner"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="entity"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active Transaction.</exception>
        public static ObjectId Add(this BlockTableRecord owner, Entity entity)
        {
            Throwable.ThrowIfArgumentNull(owner, nameof(owner));
            Throwable.ThrowIfArgumentNull(entity, nameof(entity));
            var tr = owner.Database.GetTopTransaction();
            try
            {
                ObjectId id = owner.AppendEntity(entity);
                tr.AddNewlyCreatedDBObject(entity, true);
                return id;
            }
            catch
            {
                entity.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Inserts a block reference.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="blockName">Block name.</param>
        /// <param name="insertPoint">Insertion point.</param>
        /// <param name="xScale">X scale factor.</param>
        /// <param name="yScale">Y scale factor.</param>
        /// <param name="zScale">Z scale factor.</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="attribValues">Collection of key/value pairs (Tag/Value).</param>
        /// <returns>The newly created BlockReference.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="blockName"/> is null or empty.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active Transaction.</exception>
        public static BlockReference InsertBlockReference(
            this BlockTableRecord target,
            string blockName,
            Point3d insertPoint,
            double xScale = 1.0,
            double yScale = 1.0,
            double zScale = 1.0,
            double rotation = 0.0,
            Dictionary<string, string> attribValues = null)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));
            Throwable.ThrowIfStringNullOrWhiteSpace(blockName, nameof(blockName));

            var db = target.Database;

            BlockTable blockTable = db.BlockTableId.GetObject<BlockTable>();

            ObjectId btrId = blockTable.GetBlock(blockName);

            if (btrId == ObjectId.Null)
                return null;

            BlockReference blockReference = new BlockReference(insertPoint, btrId)
            {
                ScaleFactors = new Scale3d(xScale, yScale, zScale), Rotation = rotation
            };
            BlockTableRecord blockTableRecord = btrId.GetObject<BlockTableRecord>();
            if (blockTableRecord.Annotative == AnnotativeStates.True)
            {
                ObjectContextManager ocm = db.ObjectContextManager;
                ObjectContextCollection occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
                // TODO: GstarCAD has no interface of 'ObjectContexts'
                // Autodesk.AutoCAD.Internal.ObjectContexts.AddContext(br, occ.CurrentContext);
            }
            target.Add(blockReference);

            blockReference.AddAttributeReferences(attribValues);
            return blockReference;
        }

        /// <summary>
        /// Synchronizes the attributes of all block references.
        /// </summary>
        /// <param name="target">Instance which the method applies.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        public static void SynchronizeAttributes(this BlockTableRecord target)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));

            AttributeDefinition[] attDefs = target.GetObjects<AttributeDefinition>().ToArray();
            foreach (BlockReference blockReference in target.GetBlockReferenceIds(true, false).GetObjects<BlockReference>(OpenMode.ForWrite))
            {
                blockReference.ResetAttributes(attDefs);
            }

            if (!target.IsDynamicBlock)
                return;

            target.UpdateAnonymousBlocks();
            foreach (BlockTableRecord blockTableRecord in target.GetAnonymousBlockIds().GetObjects<BlockTableRecord>())
            {
                attDefs = blockTableRecord.GetObjects<AttributeDefinition>().ToArray();
                foreach (BlockReference br in blockTableRecord.GetBlockReferenceIds(true, false)
                             .GetObjects<BlockReference>(OpenMode.ForWrite))
                {
                    br.ResetAttributes(attDefs);
                }
            }
        }
    }
}

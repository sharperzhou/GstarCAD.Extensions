using System.Collections.Generic;
using System.Linq;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
using Exception = Gssoft.Gscad.Runtime.Exception;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using Exception = GrxCAD.Runtime.Exception;
#endif

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
        /// <param name="matchExact">Match the type exactly.</param>
        /// <returns>The sequence of opened objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="blockTableRecord"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static IEnumerable<T> GetObjects<T>(
            this BlockTableRecord blockTableRecord,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayers = false,
            bool matchExact = false) where T : Entity
        {
            Throwable.ThrowIfArgumentNull(blockTableRecord, nameof(blockTableRecord));
            BlockTableRecord source = openErased ? blockTableRecord.IncludingErased : blockTableRecord;

            return source.Cast<ObjectId>().GetObjects<T>(mode, openErased, forceOpenOnLockedLayers, matchExact);
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
            foreach (Entity ent in entities)
            {
                ids.Add(owner.AppendEntity(ent));
                tr.AddNewlyCreatedDBObject(ent, true);
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
        public static ObjectIdCollection Add(this BlockTableRecord owner, params Entity[] entities)
        {
            return Add(owner, (IEnumerable<Entity>)entities);
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
            return Add(owner, new[] { entity })[0];
        }

        /// <summary>
        /// Inserts a block reference to target BlockTableRecord (BlockDefinition) from another source BlockTableRecord.
        /// If the source is not existing in BlockTable, it try to INSERT external dwg as the source from CAD
        /// searching path by the <paramref name="blockName"/>.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="blockName">Block name.</param>
        /// <param name="insertPoint">Insertion point.</param>
        /// <param name="xScale">X scale factor.</param>
        /// <param name="yScale">Y scale factor.</param>
        /// <param name="zScale">Z scale factor.</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="attributeValues">Collection of key/value pairs (Tag/Value).</param>
        /// <returns>The newly created BlockReference.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="blockName"/> is null or empty.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active Transaction.</exception>
        /// <exception cref="Exception">eNullObjectId is thrown if there is no BlockTableRecord with name <paramref name="blockName"/> after inserting.</exception>
        public static BlockReference InsertBlockFrom(
            this BlockTableRecord target,
            string blockName,
            Point3d insertPoint,
            double xScale = 1.0,
            double yScale = 1.0,
            double zScale = 1.0,
            double rotation = 0.0,
            Dictionary<string, string> attributeValues = null)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));
            Throwable.ThrowIfStringNullOrWhiteSpace(blockName, nameof(blockName));

            var db = target.Database;

            BlockTable blockTable = db.BlockTableId.GetObject<BlockTable>();

            ObjectId blockTableRecordId = blockTable.GetBlockDefinition(blockName);
            Throwable.ThrowIfObjectIdNull(blockTableRecordId, nameof(blockTableRecordId));

            BlockReference blockReference = new BlockReference(insertPoint, blockTableRecordId)
            {
                ScaleFactors = new Scale3d(xScale, yScale, zScale), Rotation = rotation
            };
            BlockTableRecord blockTableRecord = blockTableRecordId.GetObject<BlockTableRecord>();
            if (blockTableRecord.Annotative == AnnotativeStates.True)
            {
                ObjectContextManager ocm = db.ObjectContextManager;
                ObjectContextCollection occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
                blockReference.AddContext(occ.CurrentContext);
            }

            target.Add(blockReference);

            blockReference.AddAttributeReferences(attributeValues);
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

            AttributeDefinition[] attributeDefinitions = target.GetObjects<AttributeDefinition>().ToArray();
            foreach (BlockReference blockReference in target.GetBlockReferenceIds(true, false)
                         .GetObjects<BlockReference>(OpenMode.ForWrite))
            {
                blockReference.ResetAttributes(attributeDefinitions);
            }

            if (!target.IsDynamicBlock)
                return;

            target.UpdateAnonymousBlocks();
            foreach (BlockTableRecord blockTableRecord in target.GetAnonymousBlockIds().GetObjects<BlockTableRecord>())
            {
                attributeDefinitions = blockTableRecord.GetObjects<AttributeDefinition>().ToArray();
                foreach (BlockReference br in blockTableRecord.GetBlockReferenceIds(true, false)
                             .GetObjects<BlockReference>(OpenMode.ForWrite))
                {
                    br.ResetAttributes(attributeDefinitions);
                }
            }
        }
    }
}

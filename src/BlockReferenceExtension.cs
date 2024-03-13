using System;
using System.Collections.Generic;
using System.Linq;

#if GSTARCADGREATERTHAN24
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
    /// Provides extension methods for the BlockReference type.
    /// </summary>
    public static class BlockReferenceExtension
    {
        /// <summary>
        /// Gets the effective name of the block reference (name of the DynamicBlockTableRecord for anonymous dynamic blocks).
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <returns>The effective name of the block reference.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static string GetEffectiveName(this BlockReference source)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));

            return source.DynamicBlockTableRecord.GetObject<BlockTableRecord>().Name;
        }

        /// <summary>
        /// Get all the AttributeReference/AttributeDefinition pairs
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <returns>Sequence of pairs AttributeReference/AttributeDefinition.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <remarks>
        /// Search algorithm based on AttributeReference. So, the result of *Definition* MAY BE NULL when
        /// the SPECIAL AttributeReference created without using AttributeDefinition.
        /// </remarks>
        public static IEnumerable<KeyValuePair<AttributeReference, AttributeDefinition>> GetAttributePairs(
            this BlockReference source)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            if (source.AttributeCollection.Count <= 0)
                yield break;
            var blockDefinition = source.BlockTableRecord.GetObject<BlockTableRecord>();
            var attributeDefinitions = blockDefinition.GetObjects<AttributeDefinition>().ToDictionary(d => d.Tag);
            var attributeReferences = source.AttributeCollection.GetObjects();

            foreach (AttributeReference attributeReference in attributeReferences)
            {
                yield return new KeyValuePair<AttributeReference, AttributeDefinition>(attributeReference,
                    attributeDefinitions.TryGetValue(attributeReference.Tag, out var val) ? val : null);
            }
        }

        /// <summary>
        /// Gets all the attributes by tag.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <returns>Sequence of pairs Tag/Attribute.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static IEnumerable<KeyValuePair<string, AttributeReference>> GetAttributesByTag(
            this BlockReference source)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            return source
                .AttributeCollection.GetObjects()
                .Select(att => new KeyValuePair<string, AttributeReference>(att.Tag, att));
        }

        /// <summary>
        /// Gets all the attribute values by tag.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <returns>Collection of pairs Tag/Value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static Dictionary<string, string> GetAttributeValues(this BlockReference source)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            return source.GetAttributesByTag()
                .ToDictionary(p => p.Key, p => p.Value.TextString, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets the value to the attribute.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="tag">Attribute tag.</param>
        /// <param name="value">New value.</param>
        /// <returns>The value if attribute was found, null otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name ="tag"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="value"/> is null.</exception>
        public static string SetAttributeValue(this BlockReference target, string tag, string value)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));
            Throwable.ThrowIfStringNullOrWhiteSpace(tag, nameof(tag));
            Throwable.ThrowIfArgumentNull(value, nameof(value));

            return target.AttributeCollection.GetObjects()
                .Where(attRef => string.Equals(attRef.Tag, tag, StringComparison.OrdinalIgnoreCase))
                .Select(attRef => attRef.UpgradeWrite().TextString = value)
                .FirstOrDefault();
        }

        /// <summary>
        /// Sets the values to the attributes.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="attributes">Collection of pairs Tag/Value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="attributes"/> is null.</exception>
        public static void SetAttributeValues(this BlockReference target, Dictionary<string, string> attributes)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));
            Throwable.ThrowIfArgumentNull(attributes, nameof(attributes));

            foreach (AttributeReference attRef in target.AttributeCollection.GetObjects())
            {
                if (attributes.ContainsKey(attRef.Tag))
                {
                    attRef.UpgradeWrite().TextString = attributes[attRef.Tag];
                }
            }
        }

        /// <summary>
        /// Adds the attribute references to the block reference and set their values.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="attributeValues">Collection of pairs Tag/Value.</param>
        /// <returns>A Dictionary containing the newly created attribute references by tag.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static Dictionary<string, AttributeReference> AddAttributeReferences(this BlockReference target,
            Dictionary<string, string> attributeValues)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));

            BlockTableRecord btr = target.BlockTableRecord.GetObject<BlockTableRecord>();

            var attributes = new Dictionary<string, AttributeReference>(attributeValues?.Comparer);
            foreach (AttributeDefinition attDef in btr.GetObjects<AttributeDefinition>())
            {
                if (attDef.Constant)
                    continue;

                var attRef = new AttributeReference();
                attRef.SetAttributeFromBlock(attDef, target.BlockTransform);
                if (attributeValues != null && attributeValues.ContainsKey(attDef.Tag))
                    attRef.TextString = attributeValues[attDef.Tag];

                target.AttributeCollection.AppendAttribute(attRef);
                target.Database.TransactionManager.AddNewlyCreatedDBObject(attRef, true);
                attributes[attRef.Tag] = attRef;
            }

            return attributes;
        }

        /// <summary>
        /// Resets the attribute references keeping their values.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="attributeDefinitions">Sequence of attribute definitions.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="attributeDefinitions"/> is null.</exception>
        internal static void ResetAttributes(this BlockReference target,
            IEnumerable<AttributeDefinition> attributeDefinitions)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));
            Throwable.ThrowIfArgumentNull(attributeDefinitions, nameof(attributeDefinitions));

            var attValues = new Dictionary<string, string>();
            foreach (AttributeReference attRef in target.AttributeCollection.GetObjects(OpenMode.ForWrite))
            {
                attValues.Add(attRef.Tag,
                    attRef.IsMTextAttribute ? attRef.MTextAttribute.Contents : attRef.TextString);
                attRef.Erase();
            }

            foreach (AttributeDefinition attDef in attributeDefinitions)
            {
                using (var attRef = new AttributeReference())
                {
                    attRef.SetAttributeFromBlock(attDef, target.BlockTransform);
                    if (attDef.Constant)
                    {
                        attRef.TextString = attDef.IsMTextAttributeDefinition
                            ? attDef.MTextAttributeDefinition.Contents
                            : attDef.TextString;
                    }
                    else if (attValues.ContainsKey(attDef.Tag))
                    {
                        attRef.TextString = attValues[attDef.Tag];
                    }

                    target.AttributeCollection.AppendAttribute(attRef);
                    target.Database.TransactionManager.AddNewlyCreatedDBObject(attRef, true);
                }
            }
        }

        /// <summary>
        /// Gets a dynamic property.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="propName">Dynamic property name.</param>
        /// <returns>The dynamic property or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name ="propName"/> is null or empty.</exception>
        public static DynamicBlockReferenceProperty GetDynamicProperty(this BlockReference source, string propName)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfStringNullOrWhiteSpace(propName, nameof(propName));

            return !source.IsDynamicBlock
                ? null
                : source.DynamicBlockReferencePropertyCollection.Cast<DynamicBlockReferenceProperty>()
                    .FirstOrDefault(prop => prop.PropertyName.Equals(propName));
        }

        /// <summary>
        /// Gets the value of a dynamic bloc property.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="propName">Dynamic property name.</param>
        /// <returns>The dynamic property value or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name ="propName"/> is null or empty.</exception>
        public static object GetDynamicPropertyValue(this BlockReference source, string propName)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfStringNullOrWhiteSpace(propName, nameof(propName));

            DynamicBlockReferenceProperty prop = source.GetDynamicProperty(propName);
            return prop?.Value;
        }

        /// <summary>
        /// Sets the value of a dynamic bloc property.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="propName">Dynamic property name.</param>
        /// <param name="value">New property value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name ="propName"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="value"/> is null.</exception>
        public static void SetDynamicPropertyValue(this BlockReference target, string propName, object value)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));
            Throwable.ThrowIfStringNullOrWhiteSpace(propName, nameof(propName));
            Throwable.ThrowIfArgumentNull(value, nameof(value));
            DynamicBlockReferenceProperty prop = target.GetDynamicProperty(propName);

            if (prop == null)
                return;

            try
            {
                prop.Value = value;
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Mirrors the block reference honoring the value of MIRRTEXT system variable.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="axis">Axis of the mirroring operation.</param>
        /// <param name="eraseSource">Value indicating if the source block reference have to be erased.</param>
        /// <returns>
        /// The mirrored <see cref="ObjectId"/> if <paramref name="eraseSource"/> is false,
        /// otherwise is <paramref name="source"/> object id.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="axis"/> is null.</exception>
        public static ObjectId Mirror(this BlockReference source, Line3d axis, bool eraseSource)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfArgumentNull(axis, nameof(axis));

            var db = source.Database;

            BlockReference mirrored;
            if (eraseSource)
            {
                mirrored = source;
                mirrored.UpgradeWrite();
            }
            else
            {
                var ids = new ObjectIdCollection(new[] { source.ObjectId });
                var mapping = new IdMapping();
                db.DeepCloneObjects(ids, db.CurrentSpaceId, mapping, false);
                mirrored = mapping[source.ObjectId].Value.GetObject<BlockReference>(OpenMode.ForWrite);
            }

            mirrored.TransformBy(Matrix3d.Mirroring(axis));

            if (db.Mirrtext)
                return mirrored.ObjectId;

            foreach (AttributeReference attRef in mirrored.AttributeCollection.GetObjects(OpenMode.ForWrite))
            {
                var pts = attRef.GetTextBoxCorners();
                var cen = new LineSegment3d(pts[0], pts[2]).MidPoint;
                var rotAxis = Math.Abs(axis.Direction.X) < Math.Abs(axis.Direction.Y)
                    ? pts[0].GetVectorTo(pts[3])
                    : pts[0].GetVectorTo(pts[1]);
                mirrored.TransformBy(Matrix3d.Rotation(Math.PI, rotAxis, cen));
            }

            return mirrored.ObjectId;
        }
    }
}

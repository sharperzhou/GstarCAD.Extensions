using System.Collections.Generic;
using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the AttributeCollection type.
    /// </summary>
    public static class AttributeCollectionExtension
    {
        /// <summary>
        /// Opens the attribute references in the given open mode.
        /// </summary>
        /// <param name="source">Attribute collection.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayers">Value indicating if locked layers should be opened.</param>
        /// <returns>The sequence of attribute references.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static IEnumerable<AttributeReference> GetObjects(
            this AttributeCollection source,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayers = false)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            if (source.Count <= 0)
            {
                yield break;
            }

            var tr = source[0].Database.GetTopTransaction();
            foreach (ObjectId id in source)
            {
                if (!id.IsErased || openErased)
                    yield return (AttributeReference)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
            }
        }
    }
}

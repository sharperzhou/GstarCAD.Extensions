using System.Collections.Generic;
using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the ObjectIdCollection type.
    /// </summary>
    public static class ObjectIdCollectionExtension
    {

        /// <summary>
        /// Opens the objects which type matches to the given one, and return them.
        /// </summary>
        /// <typeparam name="T">Type of objects to return.</typeparam>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayers">Value indicating if locked layers should be opened.</param>
        /// <returns>The sequence of opened objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static IEnumerable<T> GetObjects<T>(
            this ObjectIdCollection source,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayers = false)
            where T : DBObject
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            if (source.Count <= 0)
                yield break;

            var tr = source[0].Database.GetTopTransaction();

            var rxClass = RXObject.GetClass(typeof(T));
            foreach (ObjectId id in source)
            {
                if (id.ObjectClass != rxClass && !id.ObjectClass.IsDerivedFrom(rxClass))
                    continue;

                if (!id.IsErased || openErased)
                    yield return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
            }
        }
    }
}

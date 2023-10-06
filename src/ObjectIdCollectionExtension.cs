using System.Collections.Generic;
using System.Linq;

#if GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Runtime;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;
#endif

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
        /// <param name="matchExact">Match the type exactly.</param>
        /// <returns>The sequence of opened objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static IEnumerable<T> GetObjects<T>(
            this ObjectIdCollection source,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayers = false,
            bool matchExact = false)
            where T : DBObject
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            return source.Cast<ObjectId>()
                .GetObjects<T>(mode, openErased, forceOpenOnLockedLayers, matchExact);
        }
    }
}

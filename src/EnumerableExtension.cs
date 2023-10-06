using System;
using System.Collections.Generic;

#if GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Runtime;
using Exception = Gssoft.Gscad.Runtime.Exception;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;
using Exception = GrxCAD.Runtime.Exception;
#endif

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the IEnumerable(T) type.
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Opens the objects which type matches to the given one, and return them.
        /// </summary>
        /// <typeparam name="T">Type of object to return.</typeparam>
        /// <param name="source">Sequence of ObjectIds.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayers">Value indicating if locked layers should be opened.</param>
        /// <param name="matchExact">Match the type exactly.</param>
        /// <returns>The sequence of opened objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static IEnumerable<T> GetObjects<T>(
            this IEnumerable<ObjectId> source,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayers = false,
            bool matchExact = false) where T : DBObject
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));

            Transaction tr = null;
            var rxClass = RXObject.GetClass(typeof(T));
            foreach (ObjectId id in source)
            {
                tr = tr ?? id.Database.GetTopTransaction();

                if (!id.ObjectClass.IsDerivedFrom(rxClass))
                    continue;

                if (matchExact && !string.Equals(id.ObjectClass.Name, rxClass.Name,
                        StringComparison.Ordinal))
                    continue;

                if (!id.IsErased || openErased)
                    yield return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
            }
        }

        /// <summary>
        /// Upgrades the open mode of all objects in the sequence.
        /// </summary>
        /// <typeparam name="T">Type of objects.</typeparam>
        /// <param name="source">Sequence of DBObjects to upgrade.</param>
        /// <returns>The sequence of opened for write objects (objets on locked layers are discard).</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there's no active transaction.</exception>
        public static IEnumerable<T> UpgradeWrite<T>(this IEnumerable<T> source) where T : DBObject
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            foreach (T item in source)
            {
                try
                {
                    item.UpgradeWrite();
                }
                catch (Exception ex)
                {
                    if (ex.ErrorStatus != ErrorStatus.OnLockedLayer)
                        throw;
                    continue;
                }

                yield return item;
            }
        }

        /// <summary>
        /// Disposes of all items of the sequence.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="source">Sequence of disposable objects.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static void DisposeAll<T>(this IEnumerable<T> source) where T : IDisposable
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));

            System.Exception last = null;
            foreach (T item in source)
            {
                try
                {
                    item?.Dispose();
                }
                catch (System.Exception ex)
                {
                    last = last ?? ex;
                }
            }

            if (last != null)
                throw last;
        }
    }
}

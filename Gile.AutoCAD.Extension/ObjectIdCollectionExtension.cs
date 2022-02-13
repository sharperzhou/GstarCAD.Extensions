﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

using System.Collections.Generic;

namespace Gile.AutoCAD.Extension
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
        /// <exception cref="System.ArgumentNullException">Throw if <c>source</c> is null.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static IEnumerable<T> GetObjects<T>(
            this ObjectIdCollection source,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayers = false)
            where T : DBObject
        {
            Assert.IsNotNull(source, nameof(source));
            if (0 < source.Count)
            {
                var tr = source[0].Database.GetTopTransaction();

                var rxClass = RXObject.GetClass(typeof(T));
                foreach (ObjectId id in source)
                {
                    if (id.ObjectClass == rxClass || id.ObjectClass.IsDerivedFrom(rxClass))
                    {
                        if (!id.IsErased || openErased)
                            yield return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
                    }
                }
            }
        }
    }
}
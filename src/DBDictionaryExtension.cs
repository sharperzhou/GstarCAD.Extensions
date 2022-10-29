using System.Collections.Generic;
using System.Linq;
using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the DBDictionary type.
    /// </summary>
    public static class DBDictionaryExtension
    {
        /// <summary>
        /// Tries to open the object of the dictionary corresponding to the given type, in the given open mode.
        /// </summary>
        /// <typeparam name="T">Type of the returned object.</typeparam>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="key">Key of the entry in the dictionary.</param>
        /// <param name="obj">Output object.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns><c>true</c>, if the operations succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static bool TryGetObject<T>(
            this DBDictionary source,
            string key,
            out T obj,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false) where T : DBObject
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfStringNullOrWhiteSpace(key, nameof(key));

            obj = default;
            return source.Contains(key) && source.GetAt(key).TryGetObject(out obj, mode, openErased);
        }

        /// <summary>
        /// Tries to get the named dictionary.
        /// </summary>
        /// <param name="parent">Instance to which the method applies.</param>
        /// <param name="key">Name of the dictionary.</param>
        /// <param name="dict">Output dictionary.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns><c>true</c>, if the operations succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="parent"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static bool TryGetNamedDictionary(
            this DBDictionary parent,
            string key,
            out DBDictionary dict,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false)
        {
            Throwable.ThrowIfArgumentNull(parent, nameof(parent));
            Throwable.ThrowIfStringNullOrWhiteSpace(key, nameof(key));

            return parent.TryGetObject(key, out dict, mode, openErased);
        }

        /// <summary>
        /// Opens the entities which type matches to the given one, and return them.
        /// </summary>
        /// <typeparam name="T">Type of returned objects.</typeparam>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayers">Value indicating if locked layers should be opened.</param>
        /// <param name="matchExact">Match the type exactly.</param>
        /// <returns>The sequence of collected objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active Transaction.</exception>
        public static IEnumerable<T> GetObjects<T>(
            this DBDictionary source,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayers = false,
            bool matchExact = false)
            where T : DBObject
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            source = openErased ? source.IncludingErased : source;
            return source.Cast<DBDictionaryEntry>()
                .Select(x => x.Value)
                .GetObjects<T>(mode, openErased, forceOpenOnLockedLayers, matchExact);
        }

        /// <summary>
        /// Gets or creates the named dictionary.
        /// </summary>
        /// <param name="parent">Instance to which the method applies.</param>
        /// <param name="name">Name of the dictionary.</param>
        /// <returns>The found or newly created dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="parent"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="name"/> is null or empty.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active Transaction.</exception>
        public static DBDictionary GetOrCreateNamedDictionary(this DBDictionary parent, string name)
        {
            Throwable.ThrowIfArgumentNull(parent, nameof(parent));
            Throwable.ThrowIfStringNullOrWhiteSpace(name, nameof(name));
            var tr = parent.Database.GetTopTransaction();
            if (parent.Contains(name))
            {
                return parent.GetAt(name).GetObject<DBDictionary>();
            }

            parent.UpgradeWrite();
            var dict = new DBDictionary();
            parent.SetAt(name, dict);
            tr.AddNewlyCreatedDBObject(dict, true);
            return dict;
        }

        /// <summary>
        /// Gets the xrecord data.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="key">la clé du xrecord.</param>
        /// <returns>The xrecord data or null if the xrecord does not exists.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static ResultBuffer GetXrecordData(this DBDictionary source, string key)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfStringNullOrWhiteSpace(key, nameof(key));

            if (!source.Contains(key))
            {
                return null;
            }

            var id = (ObjectId)source[key];
            return id.TryGetObject(out Xrecord xrecord) ? xrecord.Data : null;
        }

        /// <summary>
        /// Sets the xrecord data.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="key">Key of the xrecord, the xrecord is created if it did not already exist.</param>
        /// <param name="values">Xrecord data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static void SetXrecordData(this DBDictionary target, string key, params TypedValue[] values)
        {
            target.SetXrecordData(key, new ResultBuffer(values));
        }

        /// <summary>
        /// Sets the xrecord data.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="key">Key of the xrecord, the xrecord is created if it did not already exist.</param>
        /// <param name="data">Xrecord data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static void SetXrecordData(this DBDictionary target, string key, ResultBuffer data)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));
            Throwable.ThrowIfStringNullOrWhiteSpace(key, nameof(key));
            Xrecord xrecord;
            if (target.Contains(key))
            {
                xrecord = ((ObjectId)target[key]).GetObject<Xrecord>(OpenMode.ForWrite);
            }
            else
            {
                target.UpgradeWrite();
                xrecord = new Xrecord();
                target.SetAt(key, xrecord);
                target.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(xrecord, true);
            }

            xrecord.Data = data;
        }
    }
}

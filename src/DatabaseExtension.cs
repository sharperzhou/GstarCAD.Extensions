using System.Collections.Generic;
using System.Linq;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Runtime;

#else
using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;
#endif

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the Database type.
    /// </summary>
    public static class DatabaseExtension
    {
        /// <summary>
        /// Gets the database top transaction. Throws an exception if none.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <returns>The active top transaction.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static Transaction GetTopTransaction(this Database db)
        {
            Throwable.ThrowIfArgumentNull(db, nameof(db));
            var tr = db.TransactionManager.TopTransaction;
            if (tr == null)
                throw new Exception(ErrorStatus.NoActiveTransactions);
            return tr;
        }

        /// <summary>
        /// Gets the named object dictionary.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The named object dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static DBDictionary GetNamedObjectsDictionary(this Database db, OpenMode mode = OpenMode.ForRead)
        {
            Throwable.ThrowIfArgumentNull(db, nameof(db));
            return db.NamedObjectsDictionaryId.GetObject<DBDictionary>(mode);
        }

        /// <summary>
        /// Gets the model space block table record.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The model space.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static BlockTableRecord GetModelSpace(this Database db, OpenMode mode = OpenMode.ForRead)
        {
            Throwable.ThrowIfArgumentNull(db, nameof(db));
            return SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject<BlockTableRecord>(mode);
        }

        /// <summary>
        /// Gets the current space block table record.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The current space.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static BlockTableRecord GetCurrentSpace(this Database db, OpenMode mode = OpenMode.ForRead)
        {
            Throwable.ThrowIfArgumentNull(db, nameof(db));
            return db.CurrentSpaceId.GetObject<BlockTableRecord>(mode);
        }

        /// <summary>
        /// Gets the block table record of each layout.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="exceptModel">Value indicating if the model space layout is left out.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The sequence of block table records.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static IEnumerable<BlockTableRecord> GetLayoutBlockTableRecords(this Database db,
            bool exceptModel = true, OpenMode mode = OpenMode.ForRead)
        {
            Throwable.ThrowIfArgumentNull(db, nameof(db));
            return db.GetLayouts(exceptModel).Select(l => l.BlockTableRecordId.GetObject<BlockTableRecord>(mode));
        }

        /// <summary>
        /// Gets the layouts.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="exceptModel">Value indicating if the model space layout is left out.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns>The sequence of layouts.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static IEnumerable<Layout> GetLayouts(this Database db, bool exceptModel = true,
            OpenMode mode = OpenMode.ForRead, bool openErased = false)
        {
            Throwable.ThrowIfArgumentNull(db, nameof(db));
            foreach (DBDictionaryEntry entry in db.LayoutDictionaryId.GetObject<DBDictionary>())
            {
                if ((entry.Key != "Model" || !exceptModel) && (!entry.Value.IsErased || openErased))
                    yield return entry.Value.GetObject<Layout>(mode, openErased);
            }
        }

        /// <summary>
        /// Gets the layouts names.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="exceptModel">Value indicating if the model space layout is left out.</param>
        /// <returns>The sequence of layout names.</returns>
        public static IEnumerable<string> GetLayoutNames(this Database db, bool exceptModel = true) =>
            db.GetLayouts(exceptModel).OrderBy(l => l.TabOrder).Select(l => l.LayoutName);

        /// <summary>
        /// Gets the value of the custom property.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="key">Custom property key.</param>
        /// <returns>The value of the custom property; or null, if it does not exist.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static string GetCustomProperty(this Database db, string key)
        {
            Throwable.ThrowIfArgumentNull(db, nameof(db));
            Throwable.ThrowIfStringNullOrWhiteSpace(key, nameof(key));

            var summaryInfoBuilder = new DatabaseSummaryInfoBuilder(db.SummaryInfo);
            var customProperties = summaryInfoBuilder.CustomPropertyTable;
            return customProperties.Contains(key) ? (string)customProperties[key] : null;
        }

        /// <summary>
        /// Gets all the custom properties.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <returns>A dictionary of custom properties.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static Dictionary<string, string> GetCustomProperties(this Database db)
        {
            Throwable.ThrowIfArgumentNull(db, nameof(db));

            var customProperties = db.SummaryInfo.CustomProperties;
            var result = new Dictionary<string, string>();
            while (customProperties.MoveNext())
            {
                var entry = customProperties.Entry;
                result.Add((string)entry.Key, (string)entry.Value);
            }

            return result;
        }

        /// <summary>
        /// Sets the value of the custom property if it exists; otherwise, add the property.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="key">Property key.</param>
        /// <param name="value">Property value.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static void SetCustomProperty(this Database db, string key, string value)
        {
            Throwable.ThrowIfArgumentNull(db, nameof(db));
            Throwable.ThrowIfStringNullOrWhiteSpace(key, nameof(key));

            var summaryInfoBuilder = new DatabaseSummaryInfoBuilder(db.SummaryInfo);
            var customProperties = summaryInfoBuilder.CustomPropertyTable;
            if (customProperties.Contains(key))
            {
                customProperties[key] = value;
            }
            else
            {
                customProperties.Add(key, value);
            }

            db.SummaryInfo = summaryInfoBuilder.ToDatabaseSummaryInfo();
        }

        /// <summary>
        /// Sets the values of the custom properties if they exist; otherwise, add them.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="values">KeyValue pairs for properties.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="values"/> is null.</exception>
        public static void SetCustomProperties(this Database db, params KeyValuePair<string, string>[] values)
        {
            Throwable.ThrowIfArgumentNull(db, nameof(db));
            Throwable.ThrowIfArgumentNull(values, nameof(values));

            var summaryInfoBuilder = new DatabaseSummaryInfoBuilder(db.SummaryInfo);
            var customProperties = summaryInfoBuilder.CustomPropertyTable;
            foreach (KeyValuePair<string, string> pair in values)
            {
                string key = pair.Key;
                if (customProperties.Contains(key))
                {
                    customProperties[key] = pair.Value;
                }
                else
                {
                    customProperties.Add(key, pair.Value);
                }
            }

            db.SummaryInfo = summaryInfoBuilder.ToDatabaseSummaryInfo();
        }
    }
}

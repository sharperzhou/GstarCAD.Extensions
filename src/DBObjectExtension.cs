using GrxCAD.DatabaseServices;
using GrxCAD.Runtime;

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the DBObject type.
    /// </summary>
    public static class DBObjectExtension
    {
        /// <summary>
        /// Tries to get the object extension dictionary
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="dict">Output dictionary.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns><c>true</c>, if the operation succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static bool TryGetExtensionDictionary(this DBObject source, out DBDictionary dict,
            OpenMode mode = OpenMode.ForRead)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));

            dict = null;
            var dictId = source.ExtensionDictionary;
            if (dictId == ObjectId.Null)
            {
                return false;
            }

            dict = dictId.GetObject<DBDictionary>(mode);
            return true;
        }

        /// <summary>
        /// Gets or creates the extension dictionary.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <returns>The extension dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static DBDictionary GetOrCreateExtensionDictionary(this DBObject source)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            if (source.ExtensionDictionary == ObjectId.Null)
            {
                source.OpenForWrite();
                source.CreateExtensionDictionary();
            }

            return source.ExtensionDictionary.GetObject<DBDictionary>();
        }

        /// <summary>
        /// Gets the xrecord data of the extension dictionary of the object.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="key">Xrecord key.</param>
        /// <returns>The xrecord data or null if the xrecord does not exists.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static ResultBuffer GetXDictionaryXrecordData(this DBObject source, string key)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfStringNullOrWhiteSpace(key, nameof(key));
            return source.TryGetExtensionDictionary(out DBDictionary dict) ? dict.GetXrecordData(key) : null;
        }

        /// <summary>
        /// Sets the xrecord data of the extension dictionary of the object.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="key">The xrecord key.</param>
        /// <param name="values">The new xrecord data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static void SetXDictionaryXrecordData(this DBObject target, string key, params TypedValue[] values)
        {
            target.SetXDictionaryXrecordData(key, new ResultBuffer(values));
        }

        /// <summary>
        /// Sets the xrecord data of the extension dictionary of the object.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="key">The xrecord key.</param>
        /// <param name="data">The new xrecord data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static void SetXDictionaryXrecordData(this DBObject target, string key, ResultBuffer data)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));
            Throwable.ThrowIfStringNullOrWhiteSpace(key, nameof(key));
            target.GetOrCreateExtensionDictionary().SetXrecordData(key, data);
        }

        /// <summary>
        /// Sets the object extended data (xdata) for the application.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="data">Extended data (the first TypedValue must be: (1001, &lt;regAppName&gt;)).</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="data"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there's no active transaction.</exception>
        /// <exception cref="Exception">eBadDxfSequence is thrown if the result buffer is not valid.</exception>
        public static void SetXDataForApplication(this DBObject target, ResultBuffer data)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));
            Throwable.ThrowIfArgumentNull(data, nameof(data));
            Database db = target.Database;
            Transaction tr = db.GetTopTransaction();
            var typedValue = data.AsArray()[0];
            if (typedValue.TypeCode != 1001)
                throw new Exception(ErrorStatus.BadDxfSequence);
            string appName = (string)typedValue.Value;
            RegAppTable regAppTable = db.RegAppTableId.GetObject<RegAppTable>();
            if (!regAppTable.Has(appName))
            {
                var regApp = new RegAppTableRecord();
                regApp.Name = appName;
                regAppTable.OpenForWrite();
                regAppTable.Add(regApp);
                tr.AddNewlyCreatedDBObject(regApp, true);
            }

            target.XData = data;
        }

        /// <summary>
        /// Opens the object for write.
        /// </summary>
        /// <param name="dbObj">Instance to which the method applies.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="dbObj"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there's no active transaction.</exception>
        public static void OpenForWrite(this DBObject dbObj)
        {
            Throwable.ThrowIfArgumentNull(dbObj, nameof(dbObj));

            if (!dbObj.IsWriteEnabled)
                dbObj.Database.GetTopTransaction().GetObject(dbObj.ObjectId, OpenMode.ForWrite);
        }
    }
}

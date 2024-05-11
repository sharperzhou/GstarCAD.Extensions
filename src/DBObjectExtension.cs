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
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The extension dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static DBDictionary GetOrCreateExtensionDictionary(this DBObject source,
            OpenMode mode = OpenMode.ForRead)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            if (source.ExtensionDictionary == ObjectId.Null)
            {
                source.UpgradeWrite().CreateExtensionDictionary();
            }

            return source.ExtensionDictionary.GetObject<DBDictionary>(mode);
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
        /// <remarks>
        /// The <paramref name="data"/> only if (1001, &lt;regAppName&gt;)
        /// will remove the specified application name of xdata.
        /// </remarks>
        public static void SetXDataForApplication(this DBObject target, ResultBuffer data)
        {
            Throwable.ThrowIfArgumentNull(target, nameof(target));
            Throwable.ThrowIfArgumentNull(data, nameof(data));
            Database db = target.Database;
            Transaction tr = db.GetTopTransaction();
            var typedValue = data.Cast<TypedValue>().FirstOrDefault();
            if (typedValue.TypeCode != 1001)
                throw new Exception(ErrorStatus.BadDxfSequence);
            string appName = (string)typedValue.Value;
            RegAppTable regAppTable = db.RegAppTableId.GetObject<RegAppTable>();
            if (!regAppTable.Has(appName))
            {
                using (var regApp = new RegAppTableRecord { Name = appName })
                {
                    regAppTable.UpgradeWrite().Add(regApp);
                    tr.AddNewlyCreatedDBObject(regApp, true);
                }
            }

            target.XData = data;
        }

        /// <summary>
        /// Sets the object extended data (xdata) for the application.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="appName">The application name.</param>
        /// <param name="typedValues">Typed values below the application name field.</param>
        /// <remarks>
        /// Setting zero length array of <paramref name="typedValues"/> will
        /// remove the specified application name of xdata.
        /// </remarks>
        public static void SetXDataForApplication(this DBObject target, string appName, params TypedValue[] typedValues)
        {
            ResultBuffer resultBuffer = new ResultBuffer(new TypedValue(1001, appName));
            if (typedValues != null)
            {
                foreach (TypedValue typedValue in typedValues)
                {
                    resultBuffer.Add(typedValue);
                }
            }

            target.SetXDataForApplication(resultBuffer);
        }

        /// <summary>
        /// Upgrade the object for write.
        /// </summary>
        /// <param name="dbObject">Instance to which the method applies.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="dbObject"/> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there's no active transaction.</exception>
        public static T UpgradeWrite<T>(this T dbObject) where T : DBObject
        {
            Throwable.ThrowIfArgumentNull(dbObject, nameof(dbObject));

            if (!dbObject.IsWriteEnabled)
                dbObject.Database.GetTopTransaction().GetObject(dbObject.ObjectId, OpenMode.ForWrite);
            return dbObject;
        }
    }
}

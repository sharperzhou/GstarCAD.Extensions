using System.Linq;

#if GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
#else
using GrxCAD.DatabaseServices;
#endif

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the DBObjectCollection type.
    /// </summary>
    public static class DBObjectCollectionExtension
    {
        /// <summary>
        /// Disposes of all objects in the collections.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static void DisposeAll(this DBObjectCollection source)
        {
            source.Cast<DBObject>().DisposeAll();
            source.Dispose();
        }
    }
}

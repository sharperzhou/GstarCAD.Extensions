using GrxCAD.DatabaseServices;

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
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            if (source.Count > 0)
            {
                System.Exception last = null;
                foreach (DBObject obj in source)
                {
                    try
                    {
                        obj?.Dispose();
                    }
                    catch (System.Exception ex)
                    {
                        last = last ?? ex;
                    }
                }
                source.Clear();
                if (last != null)
                    throw last;
            }
            source.Dispose();
        }
    }
}

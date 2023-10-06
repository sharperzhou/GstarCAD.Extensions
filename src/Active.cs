#if GSTARCADGREATERTHAN24
using Gssoft.Gscad.ApplicationServices;
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.EditorInput;
using Application = Gssoft.Gscad.ApplicationServices.Core.Application;
#else
using GrxCAD.ApplicationServices;
using GrxCAD.DatabaseServices;
using GrxCAD.EditorInput;
#endif

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides easy access to several "active" objects in GstarCAD runtime environment.
    /// </summary>
    /// <remarks>
    /// Inspired by Scott McFarlane
    /// https://www.autodesk.com/autodesk-university/class/Being-Remarkable-C-NET-AutoCAD-Developer-2015#handout
    /// </remarks>
    public static class Active
    {
        /// <summary>
        /// Gets the active Document object.
        /// </summary>
        public static Document Document
        {
            get
            {
                if (Application.DocumentManager.MdiActiveDocument != null)
                    return Application.DocumentManager.MdiActiveDocument;

                var type = typeof(DocumentCollection);
                return type.GetProperty("CurrentDocument", typeof(Document))?.GetGetMethod()
                    ?.Invoke(Application.DocumentManager, null) as Document;
            }
        }

        /// <summary>
        /// Gets the active Database object.
        /// </summary>
        public static Database Database => Document.Database;

        /// <summary>
        /// Gets the active Editor object.
        /// </summary>
        public static Editor Editor => Document.Editor;

        /// <summary>
        /// Start new transaction in active Database
        /// </summary>
        public static Transaction StartTransaction() => Database.TransactionManager.StartTransaction();
    }
}

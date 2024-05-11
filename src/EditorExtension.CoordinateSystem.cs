#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.EditorInput;
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.Runtime;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.EditorInput;
using GrxCAD.Geometry;
using GrxCAD.Runtime;
#endif

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the Editor type.
    /// </summary>
    public static partial class EditorExtension
    {
        /// <summary>
        /// Gets the transformation matrix from the current User Coordinate System (UCS) to the World Coordinate System (WCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The UCS to WCS transformation matrix.</returns>
        public static Matrix3d UcsToWcs(this Editor editor)
        {
            return editor.CurrentUserCoordinateSystem;
        }

        /// <summary>
        /// Gets the transformation matrix from the World Coordinate System (WCS) to the current User Coordinate System (UCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The WCS to UCS transformation matrix.</returns>
        public static Matrix3d WcsToUcs(this Editor editor)
        {
            return editor.CurrentUserCoordinateSystem.Inverse();
        }

        /// <summary>
        /// Gets the transformation matrix from the current viewport Display Coordinate System (DCS) to the World Coordinate System (WCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The DCS to WCS transformation matrix.</returns>
        public static Matrix3d DcsToWcs(this Editor editor)
        {
            Matrix3d retVal;
            bool tileMode = editor.Document.Database.TileMode;
            if (!tileMode)
                editor.SwitchToModelSpace();
            using (ViewTableRecord currentView = editor.GetCurrentView())
            {
                retVal = currentView.DcsToWcs();
            }

            if (!tileMode)
                editor.SwitchToPaperSpace();
            return retVal;
        }

        /// <summary>
        /// Gets the transformation matrix from the World Coordinate System (WCS) to the current viewport Display Coordinate System (DCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The WCS to DCS transformation matrix.</returns>
        public static Matrix3d WcsToDcs(this Editor editor)
        {
            return editor.DcsToWcs().Inverse();
        }

        /// <summary>
        ///  Gets the transformation matrix from the paper space active viewport Display Coordinate System (DCS) to the Paper space Display Coordinate System (PSDCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The DCS to PSDCS transformation matrix.</returns>
        /// <exception cref="Exception">
        /// eNotInPaperSpace is thrown if this method is called form Model Space.</exception>
        /// <exception cref="Exception">
        /// eCannotChangeActiveViewport is thrown if there is none floating viewport in the current layout.
        /// </exception>
        public static Matrix3d DcsToPsdcs(this Editor editor)
        {
            Database db = editor.Document.Database;
            if (db.TileMode)
                throw new Exception(ErrorStatus.NotInPaperspace);
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Viewport viewport =
                    (Viewport)tr.GetObject(editor.CurrentViewportObjectId, OpenMode.ForRead);
                if (viewport.Number != 1) return viewport.DcsToPsdcs();
                try
                {
                    editor.SwitchToModelSpace();
                    viewport = (Viewport)tr.GetObject(editor.CurrentViewportObjectId, OpenMode.ForRead);
                    editor.SwitchToPaperSpace();
                }
                catch
                {
                    throw new Exception(ErrorStatus.CannotChangeActiveViewport);
                }

                return viewport.DcsToPsdcs();
            }
        }

        /// <summary>
        ///  Gets the transformation matrix from the Paper space Display Coordinate System (PSDCS) to the paper space active viewport Display Coordinate System (DCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The PSDCS to DCS transformation matrix.</returns>
        /// <exception cref="Exception">
        /// eNotInPaperSpace is thrown if this method is called form Model Space.</exception>
        /// <exception cref="Exception">
        /// eCannotChangeActiveViewport is thrown if there is none floating viewport in the current layout.
        /// </exception>
        public static Matrix3d PsdcsToDcs(this Editor editor)
        {
            return editor.DcsToPsdcs().Inverse();
        }
    }
}

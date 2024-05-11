using System;
using System.Collections.Generic;
using System.Linq;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.ApplicationServices.Core;
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.EditorInput;
using Gssoft.Gscad.Geometry;
#else
using GrxCAD.ApplicationServices;
using GrxCAD.DatabaseServices;
using GrxCAD.EditorInput;
using GrxCAD.Geometry;
#endif

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the Editor type.
    /// </summary>
    public static partial class EditorExtension
    {
        #region Zoom

        /// <summary>
        /// Zooms to given extents in the current viewport.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="ext">Extents of the zoom.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="ext"/> is an invalid extents.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
        public static void ZoomExtents(this Editor ed, Extents3d ext)
        {
            Throwable.ThrowIfArgumentNull(ed, nameof(ed));
            if (ext.MinPoint.X > ext.MaxPoint.X || ext.MinPoint.Y > ext.MaxPoint.Y || ext.MinPoint.Z > ext.MaxPoint.Z)
                throw new ArgumentException("Invalid extents", nameof(ext));

            using (ViewTableRecord view = ed.GetCurrentView())
            {
                ext.TransformBy(view.WcsToDcs());
                view.Width = ext.MaxPoint.X - ext.MinPoint.X;
                view.Height = ext.MaxPoint.Y - ext.MinPoint.Y;
                view.CenterPoint = new Point2d(
                    (ext.MaxPoint.X + ext.MinPoint.X) / 2.0,
                    (ext.MaxPoint.Y + ext.MinPoint.Y) / 2.0);
                ed.SetCurrentView(view);
            }
        }

        /// <summary>
        /// Zooms to the extents of the current viewport.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
        public static void ZoomExtents(this Editor ed)
        {
            Database db = ed.Document.Database;
            db.UpdateExt(false);
            Extents3d ext = (short)Application.GetSystemVariable("cvport") == 1
                ? new Extents3d(db.Pextmin, db.Pextmax)
                : new Extents3d(db.Extmin, db.Extmax);
            ed.ZoomExtents(ext);
        }

        /// <summary>
        /// Zooms to the given window.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="p1">First window corner.</param>
        /// <param name="p2">Opposite window corner.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
        public static void ZoomExtents(this Editor ed, Point3d p1, Point3d p2)
        {
            Extents3d ex = new Extents3d();
            ex.AddPoint(p1);
            ex.AddPoint(p2);
            ed.ZoomExtents(ex);
        }

        /// <summary>
        /// Zooms to the specified entity collection.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="ids">Collection of the entities ObjectId on which to zoom.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
        public static void ZoomExtents(this Editor ed, IEnumerable<ObjectId> ids)
        {
            Throwable.ThrowIfArgumentNull(ed, nameof(ed));
            Throwable.ThrowIfArgumentNull(ids, nameof(ids));
            using (Transaction tr = ed.Document.TransactionManager.StartTransaction())
            {
                Extents3d ext = ids
                    .GetObjects<Entity>()
                    .Select(ent => ent.Bounds)
                    .Where(b => b.HasValue)
                    .Select(b => b.Value)
                    .Aggregate((e1, e2) =>
                    {
                        e1.AddExtents(e2);
                        return e1;
                    });
                ed.ZoomExtents(ext);
                tr.Commit();
            }
        }

        /// <summary>
        /// Zooms in current viewport to the specified scale.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="scale">Scale.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
        public static void ZoomScale(this Editor ed, double scale)
        {
            Throwable.ThrowIfArgumentNull(ed, nameof(ed));
            using (ViewTableRecord view = ed.GetCurrentView())
            {
                view.Width /= scale;
                view.Height /= scale;
                ed.SetCurrentView(view);
            }
        }

        /// <summary>
        /// Zooms in current viewport to the specified scale and center.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="center">Viewport center.</param>
        /// <param name="scale">Scale (default = 1).</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
        public static void ZoomCenter(this Editor ed, Point3d center, double scale = 1.0)
        {
            Throwable.ThrowIfArgumentNull(ed, nameof(ed));
            using (ViewTableRecord view = ed.GetCurrentView())
            {
                center = center.TransformBy(view.WcsToDcs());
                view.Height /= scale;
                view.Width /= scale;
                view.CenterPoint = new Point2d(center.X, center.Y);
                ed.SetCurrentView(view);
            }
        }

        #endregion

        #region Selection

        /// <summary>
        /// Gets a selection set using the supplied prompt selection options, the supplied filter and the supplied predicate.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="options">Selection options.</param>
        /// <param name="filter">Selection filter</param>
        /// <param name="predicate">Selection predicate.</param>
        /// <returns>The selection result.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="ed"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
        public static PromptSelectionResult GetSelection(this Editor ed, PromptSelectionOptions options,
            SelectionFilter filter, Predicate<ObjectId> predicate) =>
            ed.GetPredicatedSelection(predicate, options, filter);

        /// <summary>
        /// Gets a selection set using the supplied prompt selection options and the supplied predicate.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="options">Selection options.</param>
        /// <param name="predicate">Selection predicate.</param>
        /// <returns>The selection result.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="ed"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
        public static PromptSelectionResult GetSelection(this Editor ed, PromptSelectionOptions options,
            Predicate<ObjectId> predicate) =>
            ed.GetPredicatedSelection(predicate, options);

        /// <summary>
        /// Gets a selection set using the supplied filter and the supplied predicate.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="filter">Selection filter</param>
        /// <param name="predicate">Selection predicate.</param>
        /// <returns>The selection result.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="ed"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
        public static PromptSelectionResult GetSelection(this Editor ed, SelectionFilter filter,
            Predicate<ObjectId> predicate) =>
            ed.GetPredicatedSelection(predicate, null, filter);

        /// <summary>
        /// Gets a selection set using the supplied predicate.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="predicate">Selection predicate.</param>
        /// <returns>The selection result.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="ed"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
        public static PromptSelectionResult GetSelection(this Editor ed, Predicate<ObjectId> predicate) =>
            ed.GetPredicatedSelection(predicate);

        private static PromptSelectionResult GetPredicatedSelection(
            this Editor ed,
            Predicate<ObjectId> predicate,
            PromptSelectionOptions options = null,
            SelectionFilter filter = null)
        {
            Throwable.ThrowIfArgumentNull(ed, nameof(ed));
            Throwable.ThrowIfArgumentNull(predicate, nameof(predicate));

            void OnSelectionAdded(object sender, SelectionAddedEventArgs e)
            {
                var ids = e.AddedObjects.GetObjectIds();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (!predicate(ids[i]))
                        e.Remove(i);
                }
            }

            PromptSelectionResult result;
            ed.SelectionAdded += OnSelectionAdded;
            if (options == null)
            {
                result = filter == null ? ed.GetSelection() : ed.GetSelection(filter);
            }
            else
            {
                result = filter == null ? ed.GetSelection(options) : ed.GetSelection(options, filter);
            }

            ed.SelectionAdded -= OnSelectionAdded;
            return result;
        }

        #endregion
    }
}

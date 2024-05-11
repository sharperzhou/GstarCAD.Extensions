using System;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
#endif

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension for the AbstractViewTableRecord type.
    /// </summary>
    public static class AbstractViewTableRecordExtension
    {
        /// <summary>
        /// Gets the transformation matrix from the view Display Coordinate System (DCS) to World Coordinate System (WCS).
        /// </summary>
        /// <param name="view">Instance to which the method applies.</param>
        /// <returns>The DCS to WCS transformation matrix.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="view"/> is null.</exception>
        public static Matrix3d DcsToWcs(this AbstractViewTableRecord view)
        {
            Throwable.ThrowIfArgumentNull(view, nameof(view));
            return
                Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
                Matrix3d.Displacement(view.Target.GetAsVector()) *
                Matrix3d.PlaneToWorld(view.ViewDirection);
        }


        /// <summary>
        /// Gets the transformation matrix from the view Display Coordinate System (DCS) to World Coordinate System (WCS).
        /// </summary>
        /// <param name="view">Instance to which the method applies.</param>
        /// <returns>The DCS to WCS transformation matrix.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="view"/> is null.</exception>
        [Obsolete("Use the DcsToWcs method instead")]
        public static Matrix3d EyeToWorld(this AbstractViewTableRecord view)
        {
            Throwable.ThrowIfArgumentNull(view, nameof(view));
            return
                Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
                Matrix3d.Displacement(view.Target.GetAsVector()) *
                Matrix3d.PlaneToWorld(view.ViewDirection);
        }

        /// <summary>
        /// Gets the transformation matrix from World Coordinate System (WCS) to the view Display Coordinate System (DCS).
        /// </summary>
        /// <param name="view">Instance to which the method applies.</param>
        /// <returns>The WCS to DCS transformation matrix.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="view"/> is null.</exception>
        public static Matrix3d WcsToDcs(this AbstractViewTableRecord view)
        {
            Throwable.ThrowIfArgumentNull(view, nameof(view));
            return
                Matrix3d.WorldToPlane(view.ViewDirection) *
                Matrix3d.Displacement(view.Target.GetAsVector().Negate()) *
                Matrix3d.Rotation(view.ViewTwist, view.ViewDirection, view.Target);
        }


        /// <summary>
        /// Gets the transformation matrix from World Coordinate System (WCS) to the view Display Coordinate System (DCS).
        /// </summary>
        /// <param name="view">Instance to which the method applies.</param>
        /// <returns>The WCS to DCS transformation matrix.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="view"/> is null.</exception>
        [Obsolete("Use the WcsToDcs method instead")]
        public static Matrix3d WorldToEye(this AbstractViewTableRecord view)
        {
            Throwable.ThrowIfArgumentNull(view, nameof(view));
            return
                Matrix3d.WorldToPlane(view.ViewDirection) *
                Matrix3d.Displacement(view.Target.GetAsVector().Negate()) *
                Matrix3d.Rotation(view.ViewTwist, view.ViewDirection, view.Target);
        }
    }
}

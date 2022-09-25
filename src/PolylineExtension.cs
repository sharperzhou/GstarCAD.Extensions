using System;
using System.Collections.Generic;
using System.Linq;
using GrxCAD.DatabaseServices;

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Enumeration of offset side options
    /// </summary>
    public enum OffsetSide
    {
        /// <summary>
        /// Inside.
        /// </summary>
        In,

        /// <summary>
        /// Outside.
        /// </summary>
        Out,

        /// <summary>
        /// Left side.
        /// </summary>
        Left,

        /// <summary>
        /// Right side.
        /// </summary>
        Right,

        /// <summary>
        /// Both sides.
        /// </summary>
        Both
    }

    /// <summary>
    /// Provides the Offset() extension method for the Polyline type
    /// </summary>
    /// <remarks>
    /// credits to Tony 'TheMaster' Tanzillo
    /// http://www.theswamp.org/index.php?topic=31862.msg494503#msg494503
    /// </remarks>
    public static class PolylineExtension
    {
        /// <summary>
        /// Offset the source polyline to specified side(s).
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="offsetDist">Offset distance.</param>
        /// <param name="side">Offset side(s).</param>
        /// <returns>A polyline sequence resulting from the offset of the source polyline.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static IEnumerable<Polyline> Offset(this Polyline source, double offsetDist, OffsetSide side)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));

            offsetDist = Math.Abs(offsetDist);
            using (var polylineSet = new DisposableSet<Polyline>())
            {
                IEnumerable<Polyline> offsetRight = source.GetOffsetCurves(offsetDist).Cast<Polyline>();
                polylineSet.AddRange(offsetRight);
                IEnumerable<Polyline> offsetLeft = source.GetOffsetCurves(-offsetDist).Cast<Polyline>();
                polylineSet.AddRange(offsetLeft);
                double areaRight = offsetRight.Select(polyline => polyline.Area).Sum();
                double areaLeft = offsetLeft.Select(polyline => polyline.Area).Sum();
                switch (side)
                {
                    case OffsetSide.In:
                        return polylineSet.RemoveRange(
                            areaRight < areaLeft ? offsetRight : offsetLeft);
                    case OffsetSide.Out:
                        return polylineSet.RemoveRange(
                            areaRight < areaLeft ? offsetLeft : offsetRight);
                    case OffsetSide.Left:
                        return polylineSet.RemoveRange(offsetLeft);
                    case OffsetSide.Right:
                        return polylineSet.RemoveRange(offsetRight);
                    case OffsetSide.Both:
                        polylineSet.Clear();
                        return offsetRight.Concat(offsetLeft);
                    default:
                        return null;
                }
            }
        }
    }
}

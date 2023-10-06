using System;
using System.Collections.Generic;
using System.Linq;

#if GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
#else
using GrxCAD.DatabaseServices;
#endif

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
    public static class CurveExtension
    {
        /// <summary>
        /// Offset the source curve to specified side(s).
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="offsetDist">Offset distance.</param>
        /// <param name="side">Offset side(s).</param>
        /// <returns>A curve sequence resulting from the offset of the source curve.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static IEnumerable<Curve> Offset(this Curve source, double offsetDist, OffsetSide side)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));

            offsetDist = Math.Abs(offsetDist);
            IList<Curve> offsetRight = source.GetOffsetCurves(offsetDist).Cast<Curve>().ToList();
            IList<Curve> offsetLeft = source.GetOffsetCurves(-offsetDist).Cast<Curve>().ToList();
            double areaRight = offsetRight.Select(curve => curve.Area).Sum();
            double areaLeft = offsetLeft.Select(curve => curve.Area).Sum();

            IEnumerable<Curve> disposed;
            IEnumerable<Curve> result;
            switch (side)
            {
                case OffsetSide.In:
                    result = areaRight < areaLeft ? offsetRight : offsetLeft;
                    disposed = !(areaRight < areaLeft) ? offsetRight : offsetLeft;
                    break;
                case OffsetSide.Out:
                    result = areaRight < areaLeft ? offsetLeft : offsetRight;
                    disposed = !(areaRight < areaLeft) ? offsetLeft : offsetRight;
                    break;
                case OffsetSide.Left:
                    result = offsetLeft;
                    disposed = offsetRight;
                    break;
                case OffsetSide.Right:
                    result = offsetRight;
                    disposed = offsetLeft;
                    break;
                case OffsetSide.Both:
                    result = offsetLeft.Concat(offsetRight);
                    disposed = null;
                    break;
                default:
                    throw new InvalidOperationException("Invalid offset side");
            }

            disposed?.DisposeAll();
            return result;
        }
    }
}

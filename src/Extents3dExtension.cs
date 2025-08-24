#if !GSTARCADGREATERTHAN24
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using GrxCAD.Runtime;
#else
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.Runtime;
#endif

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Extents3d"/> type.
    /// </summary>
    public static class Extents3dExtension
    {
        /// <summary>
        /// Check if the Extents object contains the specified point.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <param name="point">The specified point</param>
        /// <param name="tolerance">Geometric tolerance</param>
        /// <returns><see langword="true"/> if the Extents object contains the specified point, otherwise <see langword="flase"/></returns>
        public static bool Contains(this Extents3d self, Point3d point, Tolerance tolerance)
        {
            return point.X + tolerance.EqualPoint >= self.MinPoint.X
                   && point.Y + tolerance.EqualPoint >= self.MinPoint.Y
                   && point.Z + tolerance.EqualPoint >= self.MinPoint.Z
                   && point.X - tolerance.EqualPoint <= self.MaxPoint.X
                   && point.Y - tolerance.EqualPoint <= self.MaxPoint.Y
                   && point.Z - tolerance.EqualPoint <= self.MaxPoint.Z;
        }

        /// <summary>
        /// Check if the Extents object contains the specified point with the <see cref="Tolerance.Global"/> tolerance.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <param name="point">The specified point</param>
        /// <returns><see langword="true"/> if the Extents object contains the specified point, otherwise <see langword="flase"/></returns>
        public static bool Contains(this Extents3d self, Point3d point)
            => self.Contains(point, Tolerance.Global);

        /// <summary>
        /// Check if the Extents object contains the specified Extents.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <param name="other">The specified Extents</param>
        /// <param name="tolerance">Geometric tolerance</param>
        /// <returns><see langword="true"/> if the Extents object contains the specified Extents, otherwise <see langword="flase"/></returns>
        /// <remarks>Shrink <paramref name="other"/> Extents and then compare</remarks>
        public static bool Contains(this Extents3d self, Extents3d other, Tolerance tolerance)
        {
            return other.MinPoint.X + tolerance.EqualPoint >= self.MinPoint.X
                   && other.MinPoint.Y + tolerance.EqualPoint >= self.MinPoint.Y
                   && other.MinPoint.Z + tolerance.EqualPoint >= self.MinPoint.Z
                   && self.MaxPoint.X >= other.MaxPoint.X - tolerance.EqualPoint
                   && self.MaxPoint.Y >= other.MaxPoint.Y - tolerance.EqualPoint
                   && self.MaxPoint.Z >= other.MaxPoint.Z - tolerance.EqualPoint;
        }

        /// <summary>
        /// Check if the Extents object contains the specified Extents with the <see cref="Tolerance.Global"/> tolerance.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <param name="other">The specified Extents</param>
        /// <returns><see langword="true"/> if the Extents object contains the specified Extents, otherwise <see langword="flase"/></returns>
        /// <remarks>Shrink <paramref name="other"/> Extents and then compare</remarks>
        public static bool Contains(this Extents3d self, Extents3d other)
            => self.Contains(other, Tolerance.Global);

        /// <summary>
        /// Check if the Extents object intersects the other Extents.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <param name="other">The other object</param>
        /// <param name="tolerance">Geometric tolerance</param>
        /// <returns>True if the Extents intersects the other one, otherwise false</returns>
        /// <remarks>
        /// <list type="number">
        /// <item>The opposite operation is <see cref="IsDisjoint(Extents3d,Extents3d,Tolerance)"/></item>
        /// <item>Expand <paramref name="other"/> Extents and then compare</item>
        /// </list>
        /// </remarks>
        public static bool Intersects(this Extents3d self, Extents3d other, Tolerance tolerance)
        {
            return other.MaxPoint.X + tolerance.EqualPoint >= self.MinPoint.X
                   && other.MaxPoint.Y + tolerance.EqualPoint >= self.MinPoint.Y
                   && other.MaxPoint.Z + tolerance.EqualPoint >= self.MinPoint.Z
                   && self.MaxPoint.X >= other.MinPoint.X - tolerance.EqualPoint
                   && self.MaxPoint.Y >= other.MinPoint.Y - tolerance.EqualPoint
                   && self.MaxPoint.Z >= other.MinPoint.Z - tolerance.EqualPoint;
        }

        /// <summary>
        /// Check if the Extents object intersects the other Extents with the <see cref="Tolerance.Global"/> tolerance.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <param name="other">The other object</param>
        /// <returns>True if the Extents intersects the other one, otherwise false</returns>
        /// <remarks>
        /// <list type="number">
        /// <item>The opposite operation is <see cref="IsDisjoint(Extents3d,Extents3d)"/></item>
        /// <item>Expand <paramref name="other"/> Extents and then compare</item>
        /// </list>
        /// </remarks>
        public static bool Intersects(this Extents3d self, Extents3d other)
            => self.Intersects(other, Tolerance.Global);

        /// <summary>
        /// Check if the Extents object does not intersect the other Extents.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <param name="other">The other object</param>
        /// <param name="tolerance">Geometric tolerance</param>
        /// <returns>True if the Extents does not intersect the other one, otherwise false</returns>
        /// <remarks>
        /// <list type="number">
        /// <item>The opposite operation is <see cref="Intersects(Extents3d,Extents3d,Tolerance)"/></item>
        /// <item>Expand <paramref name="other"/> Extents and then compare</item>
        /// </list>
        /// </remarks>
        public static bool IsDisjoint(this Extents3d self, Extents3d other, Tolerance tolerance)
        {
            return other.MinPoint.X - tolerance.EqualPoint > self.MaxPoint.X
                   || other.MinPoint.Y - tolerance.EqualPoint > self.MaxPoint.Y
                   || other.MinPoint.Z - tolerance.EqualPoint > self.MaxPoint.Z
                   || self.MinPoint.X > other.MaxPoint.X + tolerance.EqualPoint
                   || self.MinPoint.Y > other.MaxPoint.Y + tolerance.EqualPoint
                   || self.MinPoint.Z > other.MaxPoint.Z + tolerance.EqualPoint;
        }

        /// <summary>
        /// Check if the Extents object does not intersect the other Extents with the <see cref="Tolerance.Global"/> tolerance.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <param name="other">The other object</param>
        /// <returns>True if the Extents does not intersect the other one, otherwise false</returns>
        /// <remarks>
        /// <list type="number">
        /// <item>The opposite operation is <see cref="Intersects(Extents3d,Extents3d)"/></item>
        /// <item>Expand <paramref name="other"/> Extents and then compare</item>
        /// </list>
        /// </remarks>
        public static bool IsDisjoint(this Extents3d self, Extents3d other)
            => self.IsDisjoint(other, Tolerance.Global);

        /// <summary>
        /// Check if the Extents object is valid.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <returns>True if extents is valid, otherwise is false</returns>
        public static bool IsValid(this Extents3d self)
            => self.MaxPoint.X >= self.MinPoint.X
               && self.MaxPoint.Y >= self.MinPoint.Y
               && self.MaxPoint.Z >= self.MinPoint.Z;

        /// <summary>
        /// Returns a center point of Extents object.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <returns>Center point</returns>
        /// <exception cref="Exception">The extents is invalid</exception>
        public static Point3d GetCenter(this Extents3d self)
            => self.IsValid()
                ? self.MinPoint + (self.MaxPoint - self.MinPoint) * 0.5
                : throw new Exception(ErrorStatus.InvalidExtents, "The extents is invalid");

        /// <summary>
        /// Get the width of Extents object in X-Axis.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <returns>The width of Extents</returns>
        /// <exception cref="Exception">The extents is invalid</exception>
        public static double GetWidth(this Extents3d self)
            => self.IsValid()
                ? self.MaxPoint.X - self.MinPoint.X
                : throw new Exception(ErrorStatus.InvalidExtents, "The extents is invalid");

        /// <summary>
        /// Get the height of Extents object in Y-Axis.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <returns>The height of Extents</returns>
        /// <exception cref="Exception">The extents is invalid</exception>
        public static double GetHeight(this Extents3d self)
            => self.IsValid()
                ? self.MaxPoint.Y - self.MinPoint.Y
                : throw new Exception(ErrorStatus.InvalidExtents, "The extents is invalid");

        /// <summary>
        /// Get the thickness of Extents object in Z-Axis.
        /// </summary>
        /// <param name="self">The self object</param>
        /// <returns>The thickness of Extents</returns>
        /// <exception cref="Exception">The extents is invalid</exception>
        public static double GetThickness(this Extents3d self)
            => self.IsValid()
                ? self.MaxPoint.Z - self.MinPoint.Z
                : throw new Exception(ErrorStatus.InvalidExtents, "The extents is invalid");
    }
}

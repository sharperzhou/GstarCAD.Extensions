using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.ApplicationServices.Core;
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
#else
using GrxCAD.ApplicationServices;
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
#endif

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the DBText type.
    /// </summary>
    public static class DBTextExtension
    {
        /// <summary>
        /// Gets the center of the text bounding box.
        /// </summary>
        /// <param name="dbText">Instance to which the method applies.</param>
        /// <returns>The center point of the text.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="dbText"/> is null.</exception>
        public static Point3d GetTextBoxCenter(this DBText dbText)
        {
            Throwable.ThrowIfArgumentNull(dbText, nameof(dbText));

            GetTextBox(dbText, out double[] point1, out double[] point2);

            var transform =
                Matrix3d.Displacement(dbText.Position.GetAsVector()) *
                Matrix3d.Rotation(dbText.Rotation, dbText.Normal, Point3d.Origin) *
                Matrix3d.PlaneToWorld(new Plane(Point3d.Origin, dbText.Normal));

            return new Point3d(
                    (point1[0] + point2[0]) / 2.0,
                    (point1[1] + point2[1]) / 2.0,
                    (point1[2] + point2[2]) / 2.0)
                .TransformBy(transform);
        }

        /// <summary>
        /// Gets the points at corners of the text bounding box.
        /// </summary>
        /// <param name="dbText">Instance to which the method applies.</param>
        /// <returns>The points(counter-clockwise from lower left).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="dbText"/> is null.</exception>
        public static Point3d[] GetTextBoxCorners(this DBText dbText)
        {
            Throwable.ThrowIfArgumentNull(dbText, nameof(dbText));

            GetTextBox(dbText, out double[] point1, out double[] point2);

            var transform =
                Matrix3d.Displacement(dbText.Position.GetAsVector()) *
                Matrix3d.Rotation(dbText.Rotation, dbText.Normal, Point3d.Origin) *
                Matrix3d.PlaneToWorld(new Plane(Point3d.Origin, dbText.Normal));

            return new[]
            {
                new Point3d(point1).TransformBy(transform),
                new Point3d(point2[0], point1[1], 0.0).TransformBy(transform),
                new Point3d(point2).TransformBy(transform),
                new Point3d(point1[0], point2[1], 0.0).TransformBy(transform)
            };
        }

        /// <summary>
        /// Mirrors the text honoring the value of MIRRTEXT system variable.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="axis">Axis of the mirroring operation.</param>
        /// <param name="eraseSource">Value indicating if the source block reference have to be erased.</param>
        /// <returns>
        /// The mirrored <see cref="DBText"/> if <paramref name="eraseSource"/> is false,
        /// otherwise is <paramref name="source"/> self.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="axis"/> is null.</exception>
        public static DBText Mirror(this DBText source, Line3d axis, bool eraseSource)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfArgumentNull(axis, nameof(axis));
            Throwable.ThrowIfObjectIdNull(source.ObjectId, nameof(source));

            DBText mirrored = eraseSource ? source.UpgradeWrite() : (DBText)source.Clone();

            mirrored.TransformBy(Matrix3d.Mirroring(axis));

            if (source.Database.Mirrtext)
                return mirrored;

            var pts = mirrored.GetTextBoxCorners();
            var cen = new LineSegment3d(pts[0], pts[2]).MidPoint;
            var rotAxis = Math.Abs(axis.Direction.X) < Math.Abs(axis.Direction.Y)
                ? pts[0].GetVectorTo(pts[3])
                : pts[0].GetVectorTo(pts[1]);
            mirrored.TransformBy(Matrix3d.Rotation(Math.PI, rotAxis, cen));

            return mirrored;
        }

        /// <summary>
        /// Get bounding box(different between DBText.GeometricExtents) of text.
        /// </summary>
        /// <param name="dbText">Text object.</param>
        /// <param name="point1">Minimum (X,Y) coordinates of the box.</param>
        /// <param name="point2">Maximum (X,Y) coordinates of the box</param>
        /// <exception cref="InvalidOperationException">Thrown if result of calling <see cref="acedTextBox"/> is RTERROR.</exception>
        /// <remarks>Text box of <see cref="DBText"/> may not be parallel to X/Y axis</remarks>
        private static void GetTextBox(DBText dbText, out double[] point1, out double[] point2)
        {
            // TODO: No test for AttributeDefinition/AttributeReference
            int mirrored = dbText.IsMirroredInX ? 2 : 0;
            mirrored |= dbText.IsMirroredInY ? 4 : 0;
            var rb = new ResultBuffer(
                new TypedValue(1, dbText.TextString),
                new TypedValue(40, dbText.Height),
                new TypedValue(41, dbText.WidthFactor),
                new TypedValue(51, dbText.Oblique),
                new TypedValue(7, dbText.TextStyleName),
                new TypedValue(71, mirrored),
                new TypedValue(72, (int)dbText.HorizontalMode),
                new TypedValue(73, (int)dbText.VerticalMode));

            point1 = new double[3];
            point2 = new double[3];

            if (5100 != acedTextBox(
#if !GSTARCADGREATERTHAN24
                    rb.ResbufObject
#else
                    rb.UnmanagedObject
#endif
                    , point1, point2))
                throw new InvalidOperationException(
                    $"Get text box errors, ERRNO: {Application.GetSystemVariable("ERRNO")}");
        }

        /// <summary>
        /// P/Invokes the unmanaged acedTextBox method.
        /// </summary>
        /// <param name="rb">ResultBuffer containing the DBText DXF definition.</param>
        /// <param name="point1">Minimum point of the bounding box.</param>
        /// <param name="point2">Maximum point of the bounding box.</param>
        /// <returns>RTNORM, if succeeds; RTERROR, otherwise.</returns>
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("gced.dll", CharSet = CharSet.Unicode,
            CallingConvention = CallingConvention.Cdecl, EntryPoint = "gcedTextBox")]
        private static extern int gcedTextBox(IntPtr rb, double[] point1, double[] point2);

        /// <summary>
        /// P/Invokes the unmanaged acedTextBox method.
        /// </summary>
        /// <param name="rb">ResultBuffer containing the DBText DXF definition.</param>
        /// <param name="point1">Minimum point of the bounding box.</param>
        /// <param name="point2">Maximum point of the bounding box.</param>
        /// <returns>RTNORM, if succeeds; RTERROR, otherwise.</returns>
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("gccore.dll", CharSet = CharSet.Unicode,
            CallingConvention = CallingConvention.Cdecl, EntryPoint = "?gcedTextBox@@YAHPEBUresbuf@@QEAN1@Z")]
        private static extern int gcedTextBoxNew(IntPtr rb, double[] point1, double[] point2);

        /// <summary>
        /// P/Invokes the unmanaged acedTextBox method.
        /// </summary>
        /// <param name="rb">ResultBuffer containing the DBText DXF definition.</param>
        /// <param name="point1">Minimum point of the bounding box.</param>
        /// <param name="point2">Maximum point of the bounding box.</param>
        /// <returns>RTNORM, if succeeds; RTERROR, otherwise.</returns>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static int acedTextBox(IntPtr rb, double[] point1, double[] point2) =>
            Application.Version < new Version(23, 0)
                ? gcedTextBox(rb, point1, point2)
                : gcedTextBoxNew(rb, point1, point2);
    }
}

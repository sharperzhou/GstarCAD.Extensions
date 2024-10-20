#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.Runtime;
using Exception = Gssoft.Gscad.Runtime.Exception;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using GrxCAD.Runtime;
using Exception = GrxCAD.Runtime.Exception;
#endif
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestExtents3dExtension
    {
        private static readonly Extents3d s_regular = new Extents3d(Point3d.Origin, new Point3d(10, 10, 0));

        [Test]
        public void TestContainsExtents()
        {
            Assert.IsTrue(new Extents3d().Contains(new Extents3d()), "Invalid Extents contains invalid Extents");
            Assert.IsTrue(
                new Extents3d(Point3d.Origin, Point3d.Origin).Contains(new Extents3d(Point3d.Origin, Point3d.Origin)),
                "Same zero Extents");
            Assert.IsTrue(s_regular.Contains(new Extents3d(Point3d.Origin, Point3d.Origin)),
                "Regular Extents contains zero Extents");
            Assert.IsTrue(s_regular.Contains(s_regular), "Same regular Extents");

            // Typical case
            Assert.IsTrue(s_regular.Contains(new Extents3d(new Point3d(2, 2, 0), new Point3d(8, 8, 0))));
            // Intersection case
            Assert.IsFalse(s_regular.Contains(new Extents3d(new Point3d(5, 5, 0), new Point3d(12, 12, 0))));

            // Use tolerance
            Assert.IsFalse(s_regular.Contains(new Extents3d(Point3d.Origin, new Point3d(11, 11, 0))));
            Assert.IsTrue(
                s_regular.Contains(new Extents3d(Point3d.Origin, new Point3d(11, 11, 0)), new Tolerance(1, 1)));
        }

        [Test]
        public void TestContainsPoint()
        {
            Assert.IsTrue(s_regular.Contains(Point3d.Origin));

            Assert.IsFalse(s_regular.Contains(new Point3d(-1, 0, 0)));
            Assert.IsTrue(s_regular.Contains(new Point3d(-1, 0, 0), new Tolerance(1, 1)));

            Assert.IsFalse(s_regular.Contains(new Point3d(5, 5, -1)));
        }

        [Test]
        public void TestIntersection()
        {
            Assert.IsTrue(s_regular.Intersects(s_regular), "Self intersection");
            Assert.IsTrue(
                new Extents3d(Point3d.Origin, Point3d.Origin).Intersects(new Extents3d(Point3d.Origin, Point3d.Origin)),
                "Zero Extents are intersection");

            // Typical intersection case
            Assert.IsTrue(s_regular.Intersects(new Extents3d(new Point3d(5, 5, 0), new Point3d(12, 12, 0))));
            Assert.IsTrue(s_regular.Intersects(new Extents3d(new Point3d(10, 0, 0), new Point3d(20, 10, 0))));
            Assert.IsTrue(s_regular.Intersects(new Extents3d(new Point3d(-10, -10, 0), Point3d.Origin)));

            // Use tolerance
            var top = new Extents3d(new Point3d(0, 12, 0), new Point3d(10, 20, 0));
            Assert.IsFalse(s_regular.Intersects(top));
            Assert.IsTrue(s_regular.Intersects(top, new Tolerance(2, 2)));
        }

        [Test]
        public void TestDisjoint()
        {
            // Regular case
            Assert.IsTrue(s_regular.IsDisjoint(new Extents3d(new Point3d(20, 20, 0), new Point3d(30, 50, 0))));

            // Neighbor case
            Assert.IsFalse(s_regular.IsDisjoint(new Extents3d(new Point3d(10, 0, 0), new Point3d(20, 10, 0))),
                "Neighbor Extents are not disjoint");

            // Use tolerance
            var top = new Extents3d(new Point3d(0, 11, 0), new Point3d(10, 20, 0));
            Assert.IsTrue(s_regular.IsDisjoint(top));
            Assert.IsFalse(s_regular.IsDisjoint(top, new Tolerance(1, 1)));
        }

        [Test]
        public void TestIsValid()
        {
            Assert.IsTrue(s_regular.IsValid(), "Regular extents is valid");
            Assert.IsFalse(new Extents3d(new Point3d(10, 10, 0), Point3d.Origin).IsValid(),
                "(10,10,0)-(0,0,0) is invalid");
        }

        [Test]
        public void TestGetCenter()
        {
            Assert.AreEqual(new LineSegment3d(s_regular.MinPoint, s_regular.MaxPoint).MidPoint, s_regular.GetCenter());
            var ex = Assert.Throws<Exception>(() => new Extents3d(new Point3d(10, 10, 0), Point3d.Origin).GetCenter());
            Assert.NotNull(ex);
            Assert.AreEqual(ex.ErrorStatus, ErrorStatus.InvalidExtents);
        }

        [Test]
        public void TestWidthHeightAndThickness()
        {
            double actualWidth = s_regular.MaxPoint.X - s_regular.MinPoint.X;
            double actualHeight = s_regular.MaxPoint.Y - s_regular.MinPoint.Y;
            double actualThickness = s_regular.MaxPoint.Z - s_regular.MinPoint.Z;

            Assert.AreEqual(actualWidth, s_regular.GetWidth());
            Assert.AreEqual(actualHeight, s_regular.GetHeight());
            Assert.AreEqual(actualThickness, s_regular.GetThickness());

            var invalid = new Extents3d(new Point3d(10, 10, 0), Point3d.Origin);
            Assert.AreEqual(ErrorStatus.InvalidExtents,
                Assert.Throws<Exception>(() => invalid.GetWidth())?.ErrorStatus);
            Assert.AreEqual(ErrorStatus.InvalidExtents,
                Assert.Throws<Exception>(() => invalid.GetHeight())?.ErrorStatus);
            Assert.AreEqual(ErrorStatus.InvalidExtents,
                Assert.Throws<Exception>(() => invalid.GetThickness())?.ErrorStatus);
        }
    }
}

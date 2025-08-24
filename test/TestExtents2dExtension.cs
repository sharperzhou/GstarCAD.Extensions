#if !GSTARCADGREATERTHAN24
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using GrxCAD.Runtime;
using Exception = GrxCAD.Runtime.Exception;
#else
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.Runtime;
using Exception = Gssoft.Gscad.Runtime.Exception;
#endif
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestExtents2dExtension
    {
        private static readonly Extents2d s_regular = new Extents2d(Point2d.Origin, new Point2d(10, 10));
        private static readonly Extents2d s_invalid = new Extents2d(Point2d.Origin, new Point2d(-100, 100));

        [Test]
        public void TestContainsPoint()
        {
            Assert.IsTrue(s_regular.Contains(Point2d.Origin));
            Assert.IsTrue(s_regular.Contains(new Point2d(5, 5)));

            Assert.IsFalse(s_regular.Contains(new Point2d(-1, 0)));
            Assert.IsTrue(s_regular.Contains(new Point2d(-1, 0), new Tolerance(1, 1)));

            Assert.IsFalse(s_regular.Contains(new Point2d(20, 20)));
        }

        [Test]
        public void TestContainsExtents()
        {
            var zero = new Extents2d(Point2d.Origin, Point2d.Origin);
            Assert.IsTrue(s_regular.Contains(s_regular), "s_regular.Contains(s_regular)");
            Assert.IsTrue(s_regular.Contains(zero),
                "s_regular.Contains(new Extents2d(Point2d.Origin, Point2d.Origin))");
            Assert.IsTrue(zero.Contains(zero), "Zero Extents contains zero Extents");

            Assert.IsFalse(s_regular.Contains(new Extents2d(new Point2d(-100, -100), new Point2d(100, -10))),
                "Disjoint case is not containing other");
            Assert.IsFalse(s_regular.Contains(new Extents2d(new Point2d(8, 8), new Point2d(16, 16))),
                "Intersection case is not containing other");

            var outer = new Extents2d(new Point2d(-1, -1), new Point2d(11, 11));
            Assert.IsFalse(s_regular.Contains(outer), "Inner extents can not contains outer extents");
            Assert.IsTrue(s_regular.Contains(outer, new Tolerance(1, 1)),
                "Inner extents can contains outer extents with large enough tolerance");
        }

        [Test]
        public void TestIntersection()
        {
            Assert.IsTrue(s_regular.Intersects(s_regular), "Self case is intersection");
            Assert.IsTrue(
                new Extents2d(Point2d.Origin, Point2d.Origin).Intersects(new Extents2d(Point2d.Origin, Point2d.Origin)),
                "Zero Extents are intersection");

            Assert.IsTrue(s_regular.Intersects(new Extents2d(new Point2d(2, 2), new Point2d(8, 8))),
                "Contains case is intersection");
            Assert.IsTrue(s_regular.Intersects(new Extents2d(new Point2d(-6, -6), new Point2d(6, 6))),
                "Diagonal intersection case");
            Assert.IsTrue(s_regular.Intersects(new Extents2d(new Point2d(-6, 2), new Point2d(20, 4))),
                "Cross intersection case");
            Assert.IsTrue(s_regular.Intersects(new Extents2d(new Point2d(10, 0), new Point2d(20, 10))),
                "Neighbor intersection case");

            var top = new Extents2d(new Point2d(0, 11), new Point2d(10, 20));
            Assert.IsFalse(s_regular.Intersects(top), "Top/bottom with gap=1 is not intersection");
            Assert.IsTrue(s_regular.Intersects(top, new Tolerance(1, 1)),
                "Top/bottom with gap=1, but tolerance=1, is intersection");
        }

        [Test]
        public void TestDisjoint()
        {
            Assert.IsTrue(s_regular.IsDisjoint(new Extents2d(new Point2d(100, -50), new Point2d(200, 100))),
                "Regular case is disjoint");
            Assert.IsFalse(s_regular.IsDisjoint(new Extents2d(new Point2d(2, 2), new Point2d(8, 8))),
                "Contains case is not disjoint");
            Assert.IsFalse(s_regular.IsDisjoint(new Extents2d(new Point2d(8, 8), new Point2d(16, 16))),
                "Intersection case is not disjoint");
            Assert.IsFalse(s_regular.IsDisjoint(new Extents2d(new Point2d(10, 0), new Point2d(20, 10))),
                "Neighbor case is not disjoint");

            var top = new Extents2d(new Point2d(0, 11), new Point2d(10, 20));
            Assert.IsTrue(s_regular.IsDisjoint(top), "Top/bottom with gap=1 is disjoint");
            Assert.IsFalse(s_regular.IsDisjoint(top, new Tolerance(1, 1)),
                "Top/bottom with gap=1, but tolerance=1, is not disjoint");
        }

        [Test]
        public void TestIsValid()
        {
            Assert.IsTrue(s_regular.IsValid(), "Regular extents is valid");
            Assert.IsFalse(s_invalid.IsValid(), $"{s_invalid} is invalid");
        }

        [Test]
        public void TestGetCenter()
        {
            Assert.AreEqual(new LineSegment2d(s_regular.MinPoint, s_regular.MaxPoint).MidPoint, s_regular.GetCenter());
            Assert.AreEqual(ErrorStatus.InvalidExtents,
                Assert.Throws<Exception>(() => s_invalid.GetCenter())?.ErrorStatus);
        }

        [Test]
        public void TestGetWidthAndHeight()
        {
            double actualWidth = s_regular.MaxPoint.X - s_regular.MinPoint.X;
            double actualHeight = s_regular.MaxPoint.Y - s_regular.MinPoint.Y;

            Assert.AreEqual(actualWidth, s_regular.GetWidth());
            Assert.AreEqual(actualHeight, s_regular.GetHeight());

            Assert.AreEqual(ErrorStatus.InvalidExtents,
                Assert.Throws<Exception>(() => s_invalid.GetWidth())?.ErrorStatus);
            Assert.AreEqual(ErrorStatus.InvalidExtents,
                Assert.Throws<Exception>(() => s_invalid.GetHeight())?.ErrorStatus);
        }
    }
}

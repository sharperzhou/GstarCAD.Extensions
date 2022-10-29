using System.Linq;
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using GrxCAD.Runtime;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestCurveExtension
    {
        [Test]
        public void TestOffsetCircle()
        {
            using (var circle = new Circle(default, Vector3d.ZAxis, 100))
            {
                var newCurve = circle.Offset(10, OffsetSide.In).ToList();
                Assert.AreEqual(newCurve.Count, 1);
                Assert.IsTrue(newCurve[0].GetRXClass().IsDerivedFrom(RXObject.GetClass(typeof(Circle))));
                Assert.AreEqual(((Circle)newCurve[0]).Radius, 90, 1e-6);

                newCurve = circle.Offset(10, OffsetSide.Both).ToList();
                Assert.AreEqual(newCurve.Count, 2);
                Assert.IsTrue(newCurve[1].GetRXClass().IsDerivedFrom(RXObject.GetClass(typeof(Circle))));
                Assert.AreEqual(((Circle)newCurve[1]).Radius, 110, 1e-6);
            }
        }

        [Test]
        public void TestOffsetLine()
        {
            using (var line = new Line(default, new Point3d(100, 100, 0)))
            {
                var newCurve1 = line.Offset(10, OffsetSide.Left).ToList();
                var newCurve2 = line.Offset(10, OffsetSide.Right).ToList();

                Assert.AreEqual(newCurve1.Count, newCurve2.Count);

                LineSegment2d l1 = new LineSegment2d(new Point2d(newCurve1[0].StartPoint.ToArray()),
                    new Point2d(newCurve1[0].EndPoint.ToArray()));
                LineSegment2d l2 = new LineSegment2d(new Point2d(line.StartPoint.ToArray()),
                    new Point2d(line.EndPoint.ToArray()));
                LineSegment2d l3 = new LineSegment2d(new Point2d(newCurve2[0].StartPoint.ToArray()),
                    new Point2d(newCurve2[0].EndPoint.ToArray()));

                Assert.AreEqual(l1.GetDistanceTo(l2), l2.GetDistanceTo(l3), 1e-6);
            }
        }
    }
}

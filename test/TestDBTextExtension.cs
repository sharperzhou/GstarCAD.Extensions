using System;
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestDBTextExtension
    {
        [Test]
        public void TestGetTextBoxCenter()
        {
            using (var text = new DBText()
                   {
                       TextString = "hello",
                       HorizontalMode = TextHorizontalMode.TextLeft,
                       VerticalMode = TextVerticalMode.TextBottom,
                       Justify = AttachmentPoint.TopLeft,
                       AlignmentPoint = default,
                       Position = default,
                       Height = 5,
                       Rotation = Math.PI / 3
                   })
            {
                var center = text.GetTextBoxCenter();
                Assert.AreEqual(center,
                    new LineSegment3d(text.GeometricExtents.MinPoint,
                        text.GeometricExtents.MaxPoint).MidPoint);
            }
        }

        [Test]
        public void TestGetTextBoxCorners()
        {
            using (var text = new DBText())
            {
                var bounds = text.GetTextBoxCorners();
                Assert.AreEqual(bounds.Length, 4);
                Assert.AreEqual(bounds[0], bounds[2]);
                Assert.AreEqual(bounds[1], bounds[3]);

                text.TextString = "0123456789abc";
                bounds = text.GetTextBoxCorners();
                Assert.AreEqual(bounds[0], text.GeometricExtents.MinPoint);
                Assert.AreEqual(bounds[2], text.GeometricExtents.MaxPoint);
            }

            using (var text = new DBText { TextString = "Tag_pkj_123^测试", Height = 5, Rotation = Math.PI / 3 })
            {
                var bounds = text.GetTextBoxCorners();
                var vector = bounds[1] - bounds[0];

                // Angle to x-axis
                double angle = vector.GetAngleTo(Vector3d.XAxis);

                Assert.AreEqual(angle, text.Rotation, 1e-6);

                var extents = new Extents3d();
                foreach (Point3d point in bounds)
                {
                    extents.AddPoint(point);
                }

                Assert.AreEqual(extents, text.GeometricExtents);
            }
        }

        [Test]
        public void TestMirror()
        {
            using (var trans = Active.StartTransaction())
            using (var text = new DBText { TextString = "Tag_pkj_123^测试", Height = 5, Rotation = Math.PI / 3 })
            {
                var modelSpace = Active.Database.GetModelSpace(OpenMode.ForWrite);
                modelSpace.AppendEntity(text);
                trans.AddNewlyCreatedDBObject(text, true);

                var axis = new Line3d(new Point3d(-5, 0, 0), Vector3d.YAxis);
                var newText = text.Mirror(axis, false);
                modelSpace.AppendEntity(newText);
                trans.AddNewlyCreatedDBObject(newText, true);

                Assert.AreNotEqual(newText, text);

                var newText2 = newText.Mirror(axis, true);

                Assert.AreEqual(newText2, newText);
                Assert.AreEqual(newText2.Position, text.Position);
            }
        }
    }
}

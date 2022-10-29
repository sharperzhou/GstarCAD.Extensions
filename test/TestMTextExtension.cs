using System;
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestMTextExtension
    {
        private const string Content =
            "\\A1;abc{\\H0.7x;\\Sde^fs;}gasTestKey\\Pa{\\H0.7x;\\Ss^;}gsd{\\H0.7x;\\S^gg;}s\\P122345666677889907676";

        [Test]
        public void TestGetTextBoxCorners()
        {
            using (var text = new MText { Contents = Content, Attachment = AttachmentPoint.BottomCenter })
            {
                var corners = text.GetTextBoxCorners();

                Assert.AreEqual(new Extents3d(corners[0], corners[2]), text.GeometricExtents);
            }

            using (var text = new MText
                   {
                       Contents = Content, Attachment = AttachmentPoint.BottomRight, Rotation = Math.PI / 6
                   })
            {
                var corners = text.GetTextBoxCorners();
                var vector = corners[1] - corners[0];

                // Angle to x-axis
                double angle = vector.GetAngleTo(Vector3d.XAxis);

                Assert.AreEqual(angle, text.Rotation, 1e-6);
            }
        }

        [Test]
        public void TestMirror()
        {
            using (var trans = Active.StartTransaction())
            using (var text = new MText() { Contents = Content, Height = 5, Rotation = Math.PI / 3 })
            {
                var modelSpace = Active.Database.GetModelSpace(OpenMode.ForWrite);
                modelSpace.AppendEntity(text);
                trans.AddNewlyCreatedDBObject(text, true);

                var axis = new Line3d(new Point3d(-50, 0, 0), Vector3d.YAxis);
                var newText = text.Mirror(axis, false);
                modelSpace.AppendEntity(newText);
                trans.AddNewlyCreatedDBObject(newText, true);

                Assert.AreNotEqual(newText, text);

                var newText2 = newText.Mirror(axis, true);

                Assert.AreEqual(newText2, newText);
                Assert.AreEqual(newText2.Location, text.Location);
            }
        }
    }
}

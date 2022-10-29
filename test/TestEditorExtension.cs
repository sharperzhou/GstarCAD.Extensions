using System;
using System.Collections.Generic;
using System.Linq;
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestEditorExtension
    {
        [Test]
        public void TestZoomExtents()
        {
            var editor = Active.Editor;

            using (Active.StartTransaction())
            {
                if (Active.Database.GetModelSpace().GetObjects<Entity>().Any())
                    Assert.NotNull(Assert.Catch<ArgumentException>(editor.ZoomExtents));
            }

            editor.ZoomExtents(new Extents3d(new Point3d(1000, 1000, 0), new Point3d(2000, 2000, 0)));
            using (var view = editor.GetCurrentView())
            {
                Assert.AreEqual(view.CenterPoint,
                    new LineSegment2d(new Point2d(1000, 1000), new Point2d(2000, 2000)).MidPoint);
            }

            editor.ZoomExtents(default, new Point3d(100, 100, 0));
            using (var view = editor.GetCurrentView())
            {
                Assert.AreEqual(view.CenterPoint, new LineSegment2d(default, new Point2d(100, 100)).MidPoint);
            }

            var list = new List<ObjectId>();
            using (var trans = Active.StartTransaction())
            {
                var modelSpace = Active.Database.GetModelSpace(OpenMode.ForWrite);

                for (int i = 0; i < 10; i++)
                {
                    var line = new Line(default, new Point3d(1000 + i * 10, 1000 - i * 10, 0));
                    list.Add(modelSpace.AppendEntity(line));
                    trans.AddNewlyCreatedDBObject(line, true);
                }

                trans.Commit();
            }

            editor.ZoomExtents(list);
            using (var view = editor.GetCurrentView())
            {
                Assert.AreNotEqual(view.CenterPoint, new LineSegment2d(default, new Point2d(100, 100)).MidPoint);
            }

            using (var trans = Active.StartTransaction())
            {
                foreach (Entity entity in list.GetObjects<Entity>(OpenMode.ForWrite))
                {
                    entity.Erase(true);
                }

                trans.Commit();
            }
        }

        [Test]
        public void TestZoomScale()
        {
            var editor = Active.Editor;

            double width;
            double height;
            Point2d center;
            using (var view = editor.GetCurrentView())
            {
                width = view.Width;
                height = view.Height;
                center = view.CenterPoint;
            }

            editor.ZoomScale(3);

            using (var view = editor.GetCurrentView())
            {
                Assert.AreEqual(width / 3, view.Width, 1e-6);
                Assert.AreEqual(height / 3, view.Height, 1e-6);
                Assert.AreEqual(center, view.CenterPoint);
            }
        }

        [Test]
        public void TestZoomCenter()
        {
            var editor = Active.Editor;
            var newCenter = new Point3d(100, 50, 0);
            editor.ZoomCenter(newCenter);

            using (var view = editor.GetCurrentView())
            {
                Assert.AreEqual(view.CenterPoint, new Point2d(newCenter.ToArray()));
            }
        }
    }
}

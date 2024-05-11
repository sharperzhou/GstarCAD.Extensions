#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
#else
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
#endif

using System.Linq;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestViewportExtension
    {
        [Test]
        public void TestDcsAndWcs()
        {
            using (var viewport = new Viewport()
                   {
                       ViewCenter = new Point2d(100, 100),
                       ViewDirection = new Vector3d(0.7, 0.7, 0.5),
                       ViewTarget = new Point3d(1000, 0, 100),
                       TwistAngle = 1.23
                   })
            {
                var matrix1 = viewport.DcsToWcs();
                var matrix2 = viewport.WcsToDcs();

                Assert.IsTrue(matrix1.IsInverse(matrix2));
                Assert.IsTrue(matrix2.IsInverse(matrix1));
            }
        }

        [Test]
        public void TestDcsAndPsdcs()
        {
            using (Active.StartTransaction())
            using (var viewport = new Viewport()
                   {
                       ViewCenter = new Point2d(-200, 100),
                       ViewDirection = new Vector3d(1.7, 0.7, 1.5),
                       ViewTarget = new Point3d(1000, 0, -100),
                       CenterPoint = new Point3d(100, 120,-30),
                       Height = 3000,
                       Width = 3500,
                       TwistAngle = -1.34,
                       CustomScale = 2.31
                   })
            {
                var layoutBlockTableRecord = Active.Database.GetLayoutBlockTableRecords(false).First();
                layoutBlockTableRecord.UpgradeWrite().AppendEntity(viewport);

                var matrix1 = viewport.DcsToPsdcs();
                var matrix2 = viewport.PsdcsToDcs();

                Assert.IsTrue(matrix1.IsInverse(matrix2));
                Assert.IsTrue(matrix2.IsInverse(matrix1));
            }
        }
    }
}

using System;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.Geometry;
using AcRx = Gssoft.Gscad.Runtime;
#else
using GrxCAD.Geometry;
using AcRx = GrxCAD.Runtime;
#endif

using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public partial class TestEditorExtension
    {
        [Test]
        public void TestUcsAndWcs()
        {
            var editor = Active.Editor;
            var old = editor.CurrentUserCoordinateSystem;
            try
            {
                editor.CurrentUserCoordinateSystem = Matrix3d.Displacement(new Vector3d(100, 100, 0)) *
                                                     Matrix3d.Rotation(Math.PI / 6, Vector3d.XAxis, Point3d.Origin);

                Assert.AreEqual(editor.CurrentUserCoordinateSystem, editor.UcsToWcs());
                Assert.IsTrue(editor.WcsToUcs().IsInverse(editor.UcsToWcs()));
            }
            finally
            {
                editor.CurrentUserCoordinateSystem = old;
            }
        }

        [Test]
        public void TestDcsAndWcs()
        {
            var matrix1 = Active.Editor.DcsToWcs();
            var matrix2 = Active.Editor.WcsToDcs();
            Assert.IsTrue(matrix1.IsInverse(matrix2));
            Assert.IsTrue(matrix2.IsInverse(matrix1));
        }

        [Test]
        public void TestDcsAndPsdcs()
        {
            var editor = Active.Editor;
            var db = Active.Database;
            bool isInModelSpace = Active.Database.TileMode;
            try
            {
                if (isInModelSpace)
                {
                    var ex = Assert.Catch<AcRx::Exception>(() => editor.DcsToPsdcs());
                    Assert.AreEqual(ex?.ErrorStatus, AcRx::ErrorStatus.NotInPaperspace);
                }
                else
                {
                    var matrix1 = editor.DcsToPsdcs();
                    var matrix2 = editor.PsdcsToDcs();
                    Assert.IsTrue(matrix1.IsInverse(matrix2));
                    Assert.IsTrue(matrix2.IsInverse(matrix1));
                }
            }
            finally
            {
                if (isInModelSpace)
                {
                    editor.SwitchToModelSpace();
                }
                else
                {
                    editor.SwitchToPaperSpace();
                }
            }
        }
    }
}

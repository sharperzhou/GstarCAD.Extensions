using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestAbstractViewTableRecordExtension
    {
        [Test]
        public void TestEyeAndWorldMatrix()
        {
            var editor = Active.Document.Editor;
            var matrix1 = editor.GetCurrentView().DcsToWcs();
            var matrix2 = editor.GetCurrentView().WcsToDcs();

            Assert.IsTrue(matrix1.IsInverse(matrix2));
            Assert.IsTrue(matrix2.IsInverse(matrix1));
        }
    }
}

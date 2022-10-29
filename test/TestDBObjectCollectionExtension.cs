using GrxCAD.DatabaseServices;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestDBObjectCollectionExtension
    {
        [Test]
        public void TestDisposeAll()
        {
            var circle = new Circle();
            var line = new Line();
            var record = new BlockTableRecord();
            var dict = new DBDictionary();
            var xrecord = new Xrecord();
            var view = new Viewport();

            using (var collection = new DBObjectCollection
                   {
                       circle,
                       line,
                       record,
                       dict,
                       xrecord,
                       view
                   })
            {
                collection.DisposeAll();
                Assert.IsTrue(collection.IsDisposed);
                Assert.IsTrue(circle.IsDisposed);
                Assert.IsTrue(view.IsDisposed);
            }
        }
    }
}

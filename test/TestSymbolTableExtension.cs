using System;
using System.Linq;
using GrxCAD.DatabaseServices;
using NUnit.Framework;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    public class TestSymbolTableExtension
    {
        [Test]
        public void TestGetObjects()
        {
            using (Active.StartTransaction())
            {
                var blockTable = Active.Database.BlockTableId.GetObject<BlockTable>();
                var layerTable = Active.Database.LayerTableId.GetObject<LayerTable>();
                var dimensionStyleTable = Active.Database.DimStyleTableId.GetObject<DimStyleTable>();
                var lineTypeTable = Active.Database.LinetypeTableId.GetObject<LinetypeTable>();
                var regAppTable = Active.Database.RegAppTableId.GetObject<RegAppTable>();
                var textStyleTable = Active.Database.TextStyleTableId.GetObject<TextStyleTable>();
                var ucsTable = Active.Database.UcsTableId.GetObject<UcsTable>();
                var viewTable = Active.Database.ViewTableId.GetObject<ViewTable>();
                var viewportTable = Active.Database.ViewportTableId.GetObject<ViewportTable>();

                Assert.GreaterOrEqual(blockTable.GetObjects<BlockTableRecord>().Count(), 2);
                Assert.IsTrue(layerTable.GetObjects<LayerTableRecord>().Any(x => string.Equals(x.Name, "0")));
                Assert.IsTrue(dimensionStyleTable.GetObjects<DimStyleTableRecord>().Any(x =>
                    string.Equals(x.Name, "standard", StringComparison.OrdinalIgnoreCase)));
                Assert.IsTrue(lineTypeTable.GetObjects<LinetypeTableRecord>().Any(x =>
                    string.Equals(x.Name, "ByLayer", StringComparison.OrdinalIgnoreCase)));
                Assert.IsTrue(regAppTable.GetObjects<RegAppTableRecord>().Any(x =>
                    string.Equals(x.Name, "ACAD", StringComparison.OrdinalIgnoreCase)));
                Assert.IsTrue(textStyleTable.GetObjects<TextStyleTableRecord>().Any(x =>
                    string.Equals(x.Name, "Annotative", StringComparison.OrdinalIgnoreCase)));
                Assert.GreaterOrEqual(ucsTable.GetObjects<UcsTableRecord>().Count(), 0);
                Assert.GreaterOrEqual(viewTable.GetObjects<ViewTableRecord>().Count(), 0);
                Assert.IsTrue(viewportTable.GetObjects<ViewportTableRecord>().Any(x =>
                    string.Equals(x.Name, "*Active", StringComparison.OrdinalIgnoreCase)));
            }
        }

        [Test]
        public void TestPurge()
        {
            using (Active.StartTransaction())
            {
                var blockTable = Active.Database.BlockTableId.GetObject<BlockTable>();
                var layerTable = Active.Database.LayerTableId.GetObject<LayerTable>();
                var dimensionStyleTable = Active.Database.DimStyleTableId.GetObject<DimStyleTable>();
                var lineTypeTable = Active.Database.LinetypeTableId.GetObject<LinetypeTable>();
                var regAppTable = Active.Database.RegAppTableId.GetObject<RegAppTable>();
                var textStyleTable = Active.Database.TextStyleTableId.GetObject<TextStyleTable>();
                var ucsTable = Active.Database.UcsTableId.GetObject<UcsTable>();
                var viewTable = Active.Database.ViewTableId.GetObject<ViewTable>();
                var viewportTable = Active.Database.ViewportTableId.GetObject<ViewportTable>();

                Assert.GreaterOrEqual(blockTable.Purge(), 0);
                Assert.GreaterOrEqual(layerTable.Purge(), 0);
                Assert.GreaterOrEqual(dimensionStyleTable.Purge(), 0);
                Assert.GreaterOrEqual(lineTypeTable.Purge(), 0);
                Assert.GreaterOrEqual(regAppTable.Purge(), 0);
                Assert.GreaterOrEqual(textStyleTable.Purge(), 0);
                Assert.GreaterOrEqual(ucsTable.Purge(), 0);
                Assert.GreaterOrEqual(viewTable.Purge(), 0);
                Assert.GreaterOrEqual(viewportTable.Purge(), 0);
            }
        }
    }
}

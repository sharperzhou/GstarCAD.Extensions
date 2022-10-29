using System;
using System.IO;
using GrxCAD.DatabaseServices;
using GrxCAD.EditorInput;
using GrxCAD.Runtime;
using NUnit.Common;
using NUnitLite;
using Sharper.GstarCAD.Extensions;

namespace GstarCAD.Extensions.Test
{
    /// <summary>
    /// Command entry
    /// </summary>
    public class Command
    {
        [CommandMethod(nameof(TestExtensions))]
        public void TestExtensions()
        {
            new TextRunner(GetType().Assembly)
                .Execute(new ExtendedTextWrapper(new CADWriter()), TextReader.Null,
                    new[] { "--noresult", "--workers=4" });
        }

        [CommandMethod(nameof(TestSelection), CommandFlags.Session)]
        public void TestSelection()
        {
            var editor = Active.Editor;

            var res1 = editor.GetSelection(x =>
                string.Equals(x.ObjectClass.DxfName, "line", StringComparison.OrdinalIgnoreCase));

            using (Active.StartTransaction())
            {
                var res2 = editor.GetSelection(
                    new PromptSelectionOptions
                    {
                        MessageForAdding = "\nSelect entity of color index is 3: ", MessageForRemoval = "message removal"
                    }, x => x.GetObject<Entity>().ColorIndex == 3);
            }

            using (Active.StartTransaction())
            {
                var res3 = editor.GetSelection(
                    new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, "LINE") }),
                    x => x.GetObject<Line>().ColorIndex == 3);
            }
        }
    }
}

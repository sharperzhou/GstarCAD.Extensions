using System.IO;
using System.Text;
using GrxCAD.ApplicationServices;
using GrxCAD.EditorInput;

namespace GstarCAD.Extensions.Test
{
    /// <summary>
    /// CAD commandline writer
    /// </summary>
    internal class CADWriter : TextWriter
    {
        private readonly Editor _editor = Application.DocumentManager.MdiActiveDocument.Editor;

        public override Encoding Encoding => Encoding.Default;

        public override void Write(char value) => _editor.WriteMessage(value.ToString());

        public override void Write(string value) => _editor.WriteMessage(value);

        public override void WriteLine(string value) => _editor.WriteMessage($"\n{value}");
    }
}

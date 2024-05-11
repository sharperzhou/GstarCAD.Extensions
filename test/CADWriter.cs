using System.IO;
using System.Text;

#if NET48_OR_GREATER && GSTARCADGREATERTHAN24
using Gssoft.Gscad.ApplicationServices.Core;
using Gssoft.Gscad.EditorInput;
#else
using GrxCAD.ApplicationServices;
using GrxCAD.EditorInput;
#endif

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

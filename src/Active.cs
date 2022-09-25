﻿using GrxCAD.ApplicationServices;
using GrxCAD.DatabaseServices;
using GrxCAD.EditorInput;

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides easy access to several "active" objects in AutoCAD runtime environment.
    /// </summary>
    /// <remarks>
    /// Inspired by Scott McFarlane
    /// https://www.autodesk.com/autodesk-university/class/Being-Remarkable-C-NET-AutoCAD-Developer-2015#handout
    /// </remarks>
    public static class Active
    {
        /// <summary>
        /// Gets the active Document object.
        /// </summary>
        public static Document Document => Application.DocumentManager.MdiActiveDocument;

        /// <summary>
        /// Gets the active Database object.
        /// </summary>
        public static Database Database => Document.Database;

        /// <summary>
        /// Gets the active Editor object.
        /// </summary>
        public static Editor Editor => Document.Editor;
    }
}

﻿using System;
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;

namespace Sharper.GstarCAD.Extensions
{
    /// <summary>
    /// Provides extension methods for the MText Type.
    /// </summary>
    public static class MTextExtension
    {
        /// <summary>
        /// Gets the points at corners of the MText bounding box.
        /// </summary>
        /// <param name="text">Instance to which the method applies.</param>
        /// <returns>The points (counter-clockwise from lower left).</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="text"/> is null.</exception>
        public static Point3d[] GetMTextBoxCorners(this MText text)
        {
            Throwable.ThrowIfArgumentNull(text, nameof(text));

            double width = text.ActualWidth;
            double height = text.ActualHeight;
            Point3d point1, point2;
            switch (text.Attachment)
            {
                case AttachmentPoint.TopLeft:
                default:
                    point1 = new Point3d(0.0, -height, 0.0);
                    point2 = new Point3d(width, 0.0, 0.0);
                    break;
                case AttachmentPoint.TopCenter:
                    point1 = new Point3d(-width * 0.5, -height, 0.0);
                    point2 = new Point3d(width * 0.5, 0.0, 0.0);
                    break;
                case AttachmentPoint.TopRight:
                    point1 = new Point3d(-width, -height, 0.0);
                    point2 = new Point3d(0.0, 0.0, 0.0);
                    break;
                case AttachmentPoint.MiddleLeft:
                    point1 = new Point3d(0.0, -height * 0.5, 0.0);
                    point2 = new Point3d(width, height * 0.5, 0.0);
                    break;
                case AttachmentPoint.MiddleCenter:
                    point1 = new Point3d(-width * 0.5, -height * 0.5, 0.0);
                    point2 = new Point3d(width * 0.5, height * 0.5, 0.0);
                    break;
                case AttachmentPoint.MiddleRight:
                    point1 = new Point3d(-width, -height * 0.5, 0.0);
                    point2 = new Point3d(0.0, height * 0.5, 0.0);
                    break;
                case AttachmentPoint.BottomLeft:
                    point1 = new Point3d(0.0, 0.0, 0.0);
                    point2 = new Point3d(width, height, 0.0);
                    break;
                case AttachmentPoint.BottomCenter:
                    point1 = new Point3d(-width * 0.5, 0.0, 0.0);
                    point2 = new Point3d(width * 0.5, height, 0.0);
                    break;
                case AttachmentPoint.BottomRight:
                    point1 = new Point3d(-width, 0.0, 0.0);
                    point2 = new Point3d(0.0, height, 0.0);
                    break;
            }

            var transform =
                Matrix3d.Displacement(text.Location.GetAsVector()) *
                Matrix3d.Rotation(text.Rotation, text.Normal, Point3d.Origin) *
                Matrix3d.PlaneToWorld(new Plane(Point3d.Origin, text.Normal));

            return new[]
            {
                point1.TransformBy(transform),
                new Point3d(point2.X, point1.Y, 0.0).TransformBy(transform),
                point2.TransformBy(transform),
                new Point3d(point1.X, point2.Y, 0.0).TransformBy(transform)
            };
        }

        /// <summary>
        /// Mirrors the MText honoring the value of MIRRTEXT system variable.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="axis">Axis of the mirroring operation.</param>
        /// <param name="eraseSource">Value indicating if the source block reference have to be erased.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="axis"/> is null.</exception>
        public static void Mirror(this MText source, Line3d axis, bool eraseSource)
        {
            Throwable.ThrowIfArgumentNull(source, nameof(source));
            Throwable.ThrowIfArgumentNull(axis, nameof(axis));

            var db = source.Database;
            var tr = db.GetTopTransaction();

            MText mirrored;
            if (eraseSource)
            {
                mirrored = source;
                if (!mirrored.IsWriteEnabled)
                {
                    tr.GetObject(mirrored.ObjectId, OpenMode.ForWrite);
                }
            }
            else
            {
                var ids = new ObjectIdCollection(new[] { source.ObjectId });
                var mapping = new IdMapping();
                db.DeepCloneObjects(ids, db.CurrentSpaceId, mapping, false);
                mirrored = (MText)tr.GetObject(mapping[source.ObjectId].Value, OpenMode.ForWrite);
            }
            mirrored.TransformBy(Matrix3d.Mirroring(axis));

            if (db.Mirrtext)
                return;

            var pts = mirrored.GetMTextBoxCorners();
            var cen = new LineSegment3d(pts[0], pts[2]).MidPoint;
            var rotAxis = Math.Abs(axis.Direction.X) < Math.Abs(axis.Direction.Y) ?
                pts[0].GetVectorTo(pts[3]) :
                pts[0].GetVectorTo(pts[1]);
            mirrored.TransformBy(Matrix3d.Rotation(Math.PI, rotAxis, cen));
        }
    }
}

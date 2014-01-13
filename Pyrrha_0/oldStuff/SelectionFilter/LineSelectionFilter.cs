using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Pyrrha.SelectionFilter
{
    /// <summary>
    ///     A filter for defining line properties, as to retrieve lines from the modelspace
    ///     fit the specified criteria
    /// </summary>
    public sealed class LineSelectionFilter : EntitySelectionFilter
    {
        public double? Angle { get; set; }
        public PointQuery? EndX { get; set; }
        public PointQuery? EndY { get; set; }
        public PointQuery? EndZ { get; set; }
        public PointQuery? StartX { get; set; }
        public PointQuery? StartY { get; set; }
        public PointQuery? StartZ { get; set; }
        public double? Thickness { get; set; }
        
        public LineSelectionFilter(double? angle = null,
            PointQuery? endX = null,
            PointQuery? endY = null,
            PointQuery? endZ = null,
            PointQuery? startX = null,
            PointQuery? startY = null,
            PointQuery? startZ = null,
            double? thickness = null)
            : base(type: "LINE")
        {
            Angle = angle;
            EndX = endX;
            EndY = endY;
            EndZ = endZ;
            StartX = startX;
            StartY = startY;
            StartZ = startZ;
            Thickness = thickness;
        }

        internal override IList<TypedValue> GetSelectionFilter()
        {
            var rtnList = base.GetSelectionFilter(); // Get the entity filter content

            if (Angle != null)
                rtnList.Add(new TypedValue(50 , Angle.Value));

            // Startpoint
            var opString = StartX == null ? "*" : StartX.Value.Operator;
            opString += StartY == null ? ",*" : "," + StartY.Value.Operator;
            opString += StartZ == null ? ",*" : "," + StartZ.Value.Operator;
            if (!opString.Equals("*,*,*"))
            {
                rtnList.Add(new TypedValue(-4, opString));
                var sPoint = new Point3d(StartX == null ? 0 : StartX.Value.PointValue,
                                         StartY == null ? 0 : StartY.Value.PointValue,
                                         StartZ == null ? 0 : StartZ.Value.PointValue);
                rtnList.Add(new TypedValue(10, sPoint));
            }

            // Endpoint
            opString = EndX == null ? "*" : EndX.Value.Operator;
            opString += EndY == null ? ",*" : "," + EndY.Value.Operator;
            opString += EndZ == null ? ",*" : "," + EndZ.Value.Operator;
            if (!opString.Equals("*,*,*"))
            {
                rtnList.Add(new TypedValue(-4, opString));
                var sPoint = new Point3d(EndX == null ? 0 : EndX.Value.PointValue,
                                         EndY == null ? 0 : EndY.Value.PointValue,
                                         EndZ == null ? 0 : EndZ.Value.PointValue);
                rtnList.Add(new TypedValue(11, sPoint));
            }

            if (Thickness != null)
                rtnList.Add(new TypedValue(39 , Thickness.Value));

            return rtnList;
        }
    }
}

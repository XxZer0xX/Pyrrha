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
        public double? Length { get; set; }
        public Int32? LineWeight { get; set; }
        public Int32? Thickness { get; set; }
        public Point3d? StartPoint { get; set; }
        public Point3d? EndPoint { get; set; }
        public string Linetype { get; set; }

        public LineSelectionFilter(double? angle = null ,
            Point3d? startPoint = null ,
            Point3d? endPoint = null ,
            Int32? lineWeight = null ,
            Int32? thickness = null)
            : base(type: "Line")
        {
            Angle = angle;
            LineWeight = lineWeight;
            StartPoint = startPoint;
            EndPoint = endPoint;
            Thickness = thickness;
        }

        internal override List<TypedValue> GetFilterContent()
        {
            var rtnList = new List<TypedValue>();

            if (Angle != null)
                rtnList.Add(new TypedValue(50 , Angle.Value));

            if (LineWeight != null)
                rtnList.Add(new TypedValue(370 , LineWeight.Value));

            if (StartPoint != null)
            {
                rtnList.Add(new TypedValue(10 , StartPoint.Value.X));
                rtnList.Add(new TypedValue(20 , StartPoint.Value.Y));
                rtnList.Add(new TypedValue(30 , StartPoint.Value.Z));
            }

            if (EndPoint != null)
            {
                rtnList.Add(new TypedValue(11 , EndPoint.Value.X));
                rtnList.Add(new TypedValue(21 , EndPoint.Value.Y));
                rtnList.Add(new TypedValue(31 , EndPoint.Value.Z));
            }

            if (Thickness != null)
                rtnList.Add(new TypedValue(39 , Thickness.Value));

            rtnList.AddRange(base.GetFilterContent()); // Get the base filter content
            return rtnList;
        }
    }
}

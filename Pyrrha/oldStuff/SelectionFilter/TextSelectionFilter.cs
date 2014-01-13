using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Pyrrha.SelectionFilter
{
    public class TextSelectionFilter : EntitySelectionFilter
    {
        public PointQuery? AlignmentX { get; set; }
        public PointQuery? AlignmentY { get; set; }
        public PointQuery? AlignmentZ { get; set; }
        public double? Height { get; set; }
        public AttachmentPoint? Justify { get; set; }
        public double? Oblique { get; set; }
        public PointQuery? PositionX { get; set; }
        public PointQuery? PositionY { get; set; }
        public PointQuery? PositionZ { get; set; }
        public double? Rotation { get; set; }
        public string TextString { get; set; }
        public string TextStyle { get; set; }
        public TextVerticalMode? VerticalMode { get; set; }
        public double? Width { get; set; }

        public TextSelectionFilter( double? height = null,
            PointQuery? alignmentX = null,
            PointQuery? alignmentY = null,
            PointQuery? alignmentZ = null,
            AttachmentPoint? justify = null,
            double? oblique = null,
            PointQuery? positionX = null,
            PointQuery? positionY = null,
            PointQuery? positionZ = null,
            double? rotation = null,
            string textString = null,
            string textStyle = null,
            TextVerticalMode? verticalMode = null,
            double? width = null)
            : base(type: "TEXT")
        {
            AlignmentX = alignmentX;
            AlignmentY = alignmentY;
            AlignmentZ = alignmentZ;
            Height = height;
            Justify = justify;
            Oblique = oblique;
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;
            Rotation = rotation;
            TextString = textString;
            TextStyle = textStyle;
            VerticalMode = verticalMode;
            Width = width;
        }

        internal override IList<TypedValue> GetSelectionFilter()
        {
            var rtnList = base.GetSelectionFilter(); // Get the entity filter content
            var text = new DBText();

            // Alignment
            var opString = AlignmentX == null ? "*" : AlignmentX.Value.Operator;
            opString += AlignmentY == null ? ",*" : "," + AlignmentY.Value.Operator;
            opString += AlignmentZ == null ? ",*" : "," + AlignmentZ.Value.Operator;
            if (!opString.Equals("*,*,*"))
            {
                rtnList.Add(new TypedValue(-4, opString));
                var sPoint = new Point3d(AlignmentX == null ? 0 : AlignmentX.Value.PointValue,
                                         AlignmentY == null ? 0 : AlignmentY.Value.PointValue,
                                         AlignmentZ == null ? 0 : AlignmentZ.Value.PointValue);
                rtnList.Add(new TypedValue(10, sPoint));
            }

            if (Height != null)
                rtnList.Add(new TypedValue(40, Height.Value));

            if (Justify != null)
                rtnList.Add(new TypedValue(72, Justify.Value));

            if (Oblique != null)
                rtnList.Add(new TypedValue(51, Oblique.Value));

            // Position
            opString = PositionX == null ? "*" : PositionX.Value.Operator;
            opString += PositionY == null ? ",*" : "," + PositionY.Value.Operator;
            opString += PositionZ == null ? ",*" : "," + PositionZ.Value.Operator;
            if (!opString.Equals("*,*,*"))
            {
                rtnList.Add(new TypedValue(-4, opString));
                var sPoint = new Point3d(PositionX == null ? 0 : PositionX.Value.PointValue,
                                         PositionY == null ? 0 : PositionY.Value.PointValue,
                                         PositionZ == null ? 0 : PositionZ.Value.PointValue);
                rtnList.Add(new TypedValue(11, sPoint));
            }

            if (Rotation != null)
                rtnList.Add(new TypedValue(50, Rotation.Value));

            if (TextString != null)
                rtnList.Add(new TypedValue(1, TextString));

            if (TextStyle != null)
                rtnList.Add(new TypedValue(7, TextStyle));

            if (VerticalMode != null)
                rtnList.Add(new TypedValue(73, VerticalMode.Value));

            if (Width != null)
                rtnList.Add(new TypedValue(41, Width.Value));

            return rtnList;
        }
    }
}

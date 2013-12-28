using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;

namespace Pyrrha.SelectionFilter
{
    public class EntitySelectionFilter
    {
        //public AnnotativeStates? Annotative { get; set; }
        public int? ColorIndex { get; set; }
        public string Layer { get; set; }
        public string Linetype { get; set; }
        public double? LinetypeScale { get; set; }
        public LineWeight? LineWeight { get; set; }
        //public string Material { get; set; }
        public string PlotStyle { get; set; }
        public Transparency? Transparency { get; set; }
        public string Type { get; set; }
        public bool? Visible { get; set; }

        public Autodesk.AutoCAD.EditorInput.SelectionFilter Selection
        {
            get
            {
                return new Autodesk.AutoCAD.EditorInput.SelectionFilter(
                        _closeFilter(GetSelectionFilter()).ToArray());
            }
        }

        public EntitySelectionFilter(string type = null,
            //AnnotativeStates? annotative = null,
            int? colorindex = null,
            string layer = null,
            string linetype = null,
            double? linetypescale = null,
            LineWeight? lineweight = null,
            //string material = null,
            string plotstyle= null,
            Transparency? transparency = null,
            bool? visible = null)
        {
            //Annotative = annotative;
            ColorIndex = colorindex;
            Layer = layer;
            Linetype = linetype;
            LinetypeScale = linetypescale;
            LineWeight = lineweight;
            //Material = material;
            PlotStyle = plotstyle;
            Transparency = transparency;
            Type = type;
            Visible = visible;
        }

        internal virtual List<TypedValue> GetSelectionFilter()
        {
            var rtnList = new List<TypedValue>();
            if (Type != null)
                rtnList.Add(new TypedValue(0, Type));
            if (ColorIndex != null)
                rtnList.Add(new TypedValue(62, ColorIndex.Value));
            if (Layer != null)
                rtnList.Add(new TypedValue(8 , Layer));
            if (Linetype != null)
                rtnList.Add(new TypedValue(6, Linetype));
            if (LinetypeScale != null)
                rtnList.Add(new TypedValue(48, LinetypeScale.Value));
            if (LineWeight != null)
                rtnList.Add(new TypedValue(370, LineWeight.Value));
            if (PlotStyle != null)
                rtnList.Add(new TypedValue(380, PlotStyle));
            if (Transparency != null)
                rtnList.Add(new TypedValue(440, Transparency.Value.Alpha)); // Maybe?
            if (Visible != null)
                rtnList.Add(new TypedValue(60, Visible.Value));
            
            return rtnList.Count > 0 ? rtnList : null;
        }
        private List<TypedValue> _closeFilter(IEnumerable<TypedValue> filterContent)
        {
            var rtnList = filterContent.ToList();
            if (rtnList.Count > 1)
            {
                rtnList.Insert(0 , new TypedValue(-4 , "<and"));
                rtnList.Add(new TypedValue(-4 , "and>"));
            }
            return rtnList;
        }
    }
}

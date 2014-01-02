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

        public EntitySelectionFilter(string type = null ,
            //AnnotativeStates? annotative = null,
            int? colorindex = null ,
            string layer = null ,
            string linetype = null ,
            double? linetypescale = null ,
            LineWeight? lineweight = null ,
            //string material = null,
            string plotstyle = null ,
            Transparency? transparency = null ,
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

        internal virtual IList<TypedValue> GetSelectionFilter()
        {
            var rtnList = new List<TypedValue>();
            if (Type != null)
                rtnList.Add(new TypedValue(0 , Type));
            if (ColorIndex != null)
                rtnList.Add(new TypedValue(62 , ColorIndex.Value));
            if (Layer != null)
                rtnList.Add(new TypedValue(8 , Layer));
            if (Linetype != null)
                rtnList.Add(new TypedValue(6 , Linetype));
            if (LinetypeScale != null)
                rtnList.Add(new TypedValue(48 , LinetypeScale.Value));
            if (LineWeight != null)
                rtnList.Add(new TypedValue(370 , LineWeight.Value));
            if (PlotStyle != null)
                rtnList.Add(new TypedValue(380 , PlotStyle));
            if (Transparency != null)
                rtnList.Add(new TypedValue(440 , Transparency.Value.Alpha)); // Maybe?
            if (Visible != null)
                rtnList.Add(new TypedValue(60 , Visible.Value));

            return rtnList.Count > 0 ? rtnList : null;
        }

        private IEnumerable<TypedValue> _closeFilter(IEnumerable<TypedValue> filterContent)
        {
            var rtnList = filterContent != null 
                ? filterContent.ToList() 
                : new List<TypedValue>();

            if (rtnList.Count() == 1)
                return rtnList;

            if (!rtnList.Any())
            {
                rtnList.Insert( 0, new TypedValue( -4, "not>" ) );
                rtnList.Insert( 0, new TypedValue( 0, "VIEWPORT" ) );
                rtnList.Insert( 0, new TypedValue( -4, "<not" ) );
                return rtnList;
            }

            //rtnList.Insert( 0, new TypedValue( -4, "<and" ) );
            //rtnList.Add( new TypedValue( -4, "and>" ) );
            return rtnList;


            

            // document.RunQuery("update ('type'='Line' OR 'type'='PolyLine') Set Color='Green'

            // selectall(new typeselectionfilter("polyline OR lwpolyline OR line OR circle WHERE color = 'blue'"))
            //selectall(new typeselectionfilter(type: "line", color: <blue>, Startx: <0,0,0> )
            // document.RunQuery("select Line Where color = 'blue' AND Startx = '0,0,0')

            rtnList.Insert(0 , new TypedValue(-4 , "<and"));
            rtnList.Insert(0 , new TypedValue(0 , "VIEWPORT"));
            rtnList.Insert(0 , new TypedValue(-4 , "<not"));
            rtnList.Insert(0 , new TypedValue(-4 , "not>"));
            rtnList.Insert(0 , new TypedValue(0 , "VIEWPORT"));
            rtnList.Insert(0 , new TypedValue(-4 , "<not"));
            rtnList.Insert(0 , new TypedValue(-4 , "not>"));
            rtnList.Insert(0 , new TypedValue(0 , "VIEWPORT"));
            rtnList.Insert(0 , new TypedValue(-4 , "and>"));
        }
    }
}

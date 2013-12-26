using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace Pyrrha.SelectionFilter
{
    public class EntitySelectionFilter
    {
        public string Layer { get; set; }
        public int Color { get; set; }
        public string Type { get; set; }

        public Autodesk.AutoCAD.EditorInput.SelectionFilter Selection
        {
            get
            {
                return new Autodesk.AutoCAD.EditorInput.SelectionFilter(
                        GetSelectionFilter(GetFilterContent()).ToArray());
            }
        }

        public EntitySelectionFilter(string type = null , string layerName = null , int color = -1)
        {
            Type = type;
            Layer = layerName;
            Color = color;
        }

        internal virtual List<TypedValue> GetFilterContent()
        {
            var rtnList = new List<TypedValue>();
            if (Layer != null)
                rtnList.Add(new TypedValue(8 , Layer));
            if (Color != -1)
                rtnList.Add(new TypedValue(62 , Color));
            if (Type != null)
                rtnList.Add(new TypedValue(0 , Type));

            return rtnList.Count > 0 ? rtnList : null;
        }

        internal List<TypedValue> GetSelectionFilter(IEnumerable<TypedValue> filterContent)
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

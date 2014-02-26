
using Autodesk.AutoCAD.DatabaseServices;
using Utils = Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices;

namespace Pyrrha.Collections
{
    public class TextStyleCollection : RecordCollection<TextStyleTable, TextStyleTableRecord>
    {
        public TextStyleTableRecord Standard
        {
            get
            {
                return _standard ??
                    (_standard = GetRecord(Utils.GetTextStyleStandardId(ObjectManager.Database)));
            }
        }
        private TextStyleTableRecord _standard;

        public TextStyleCollection(PyrrhaDocument document, OpenMode openMode = OpenMode.ForRead)
            : base(document, document.Database.TextStyleTableId, openMode)
        {
        }
    }
}

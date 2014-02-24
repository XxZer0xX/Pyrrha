
using Autodesk.AutoCAD.DatabaseServices;

namespace Pyrrha.Collections
{
    public class TextStyleCollection : RecordCollection<TextStyleTableRecord>
    {
        public TextStyleCollection(PyrrhaDocument document, OpenMode openMode = OpenMode.ForRead)
            : base(document, document.Database.TextStyleTableId, openMode)
        {
        }
    }
}

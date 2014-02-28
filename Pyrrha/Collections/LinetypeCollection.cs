
using Autodesk.AutoCAD.DatabaseServices;
using Utils = Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices;
using Pyrrha.Runtime;

namespace Pyrrha.Collections
{
    public class LinetypeCollection : RecordCollection<LinetypeTable, LinetypeTableRecord>
    {
        public LinetypeCollection(PyrrhaDocument document, OpenMode openMode = OpenMode.ForRead) 
            : base(document, document.Database.LinetypeTableId, openMode)
        {
        }

        public void Load(string linetype, string filename = "acad.lin")
        {
            if (RecordTable.Has(linetype))
                throw new PyrrhaException("{0} is already loaded.", linetype);

            ObjectManager.Database.LoadLineTypeFile(linetype, filename);
        }

    }
}

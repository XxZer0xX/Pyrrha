
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;

namespace Pyrrha.Collections
{
    public class LinetypeCollection : RecordCollection<LinetypeTableRecord>
    {
        public LinetypeCollection(PyrrhaDocument document, OpenMode openMode = OpenMode.ForRead) 
            : base(document, document.Database.LinetypeTableId, openMode)
        {
        }

        public void Load(string linetype, string filename = "acad.lin")
        {
            if (Table.Has(linetype))
                throw new PyrrhaException("{0} is already loaded.", linetype);

            Manager.Database.LoadLineTypeFile(linetype, filename);
        }
    }
}

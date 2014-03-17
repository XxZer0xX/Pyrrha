#region Referencing

using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;
using Utils = Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices;

#endregion

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
            if (this.RecordTable.Has(linetype))
                throw new PyrrhaException("{0} is already loaded.", linetype);

            this.ObjectManager.Database.LoadLineTypeFile(linetype, filename);
        }

    }
}

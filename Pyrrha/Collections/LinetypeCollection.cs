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

        public ObjectId Load(string linetypeName)
        {
            if (!this.RecordTable.Has(linetypeName))
            {
                Transaction.Commit();
                ObjectManager.Database.LoadLineTypeFile(linetypeName, "acad.lin");
            }
            return this[linetypeName].ObjectId;
        }

        public ObjectId CreateLinetype(string name, string description)
        {
            var newRecord = new LinetypeTableRecord() { AsciiDescription = description, Name = name };
            RecordTable.Add(newRecord);
            Transaction.AddNewlyCreatedDBObject(newRecord,true);
            return newRecord.ObjectId;
        }
    }
}

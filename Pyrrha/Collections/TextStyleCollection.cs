#region Referencing

using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Utils = Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices;

#endregion

namespace Pyrrha.Collections
{
    public class TextStyleCollection : RecordCollection<TextStyleTable, TextStyleTableRecord>
    {
        public TextStyleTableRecord Standard
        {
            get
            {
                return this._standard ??
                    (this._standard = this.GetRecord(Utils.GetTextStyleStandardId(this.ObjectManager.Database)));
            }
        }
        private TextStyleTableRecord _standard;

        public TextStyleCollection(PyrrhaDocument document, OpenMode openMode = OpenMode.ForRead)
            : base(document, document.Database.TextStyleTableId, openMode)
        {
        }

        public ObjectId CreateTextStyle(string name, string fileName)
        {
            var newRecord = new TextStyleTableRecord() 
            {
                Name = name,
                FileName = fileName
            };
            RecordTable.Add(newRecord);
            Transaction.AddNewlyCreatedDBObject(newRecord, true);
            return newRecord.ObjectId;
        }
    }
}

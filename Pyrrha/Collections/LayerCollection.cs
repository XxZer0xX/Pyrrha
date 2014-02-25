
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;

namespace Pyrrha.Collections
{
    public class LayerCollection : RecordCollection<LayerTable, LayerTableRecord>
    {
        public LayerCollection(PyrrhaDocument document, OpenMode openmode = OpenMode.ForRead)
            : base(document, document.Database.LayerTableId, openmode)
        {
            //if (includeHidden)
            //    RecordTable = RecordTable.IncludingHidden;
        }

        // Some method specific to layers
        public void Export()
        {
            
        }

        public override bool Remove(LayerTableRecord item)
        {
            if (item.Name == "0")
                return false;

            if (!Contains(item))
                throw new PyrrhaException("The Layer {0} does not exist in the collection", item.Name);

            //if (item.IsUsed)
                //throw new PyrrhaException("The Layer {0} is in use", item.Name);

            item.Erase();

            return true;
        }

        public void GenerateUsageData()
        {
            RecordTable.GenerateUsageData();
        }
    }
}

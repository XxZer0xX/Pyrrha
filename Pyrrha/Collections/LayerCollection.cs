
using Autodesk.AutoCAD.DatabaseServices;

namespace Pyrrha.Collections
{
    public class LayerCollection : RecordCollection<LayerTableRecord>
    {
        // Some property specific to layers
        private bool HasUnreconciledLayers
        {
            get { throw new System.NotImplementedException(); }
        }

        public LayerCollection(PyrrhaDocument document, OpenMode openmode = OpenMode.ForRead)
            : base(document, document.Database.LayerTableId, openmode)
        {
        }

        // Some method specific to layers
        public void Export()
        {
            
        }
    }
}

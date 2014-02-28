
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;

namespace Pyrrha.Collections
{
    public class LayerCollection : RecordCollection<LayerTable, LayerTableRecord>
    {
        #region Properties

        public bool HasUnReconciledLayers
        {
            get { return RecordTable.HasUnreconciledLayers; }
        }

        public bool IncludeErased
        {
            get { return _includeErased; }
            set
            {
                RecordTable = value ? (LayerTable)RecordTable.IncludingErased : null;
                _includeErased = value;
            }
        }
        private bool _includeErased;

        public bool IncludeHidden
        {
            get { return _includeHidden; }
            set
            {
                RecordTable = value ? RecordTable.IncludingHidden : null;
                _includeHidden = value;
            }
        }
        private bool _includeHidden;

        #endregion

        #region Constructor

        public LayerCollection(PyrrhaDocument document, OpenMode openmode = OpenMode.ForRead)
            : base(document, document.Database.LayerTableId, openmode)
        {

        }

        #endregion

        #region Methods

        public void GenerateUsageData()
        {
            RecordTable.GenerateUsageData();
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

        #endregion
    }
}

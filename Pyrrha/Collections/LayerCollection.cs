
#region Referencing

using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;
using Autodesk.AutoCAD.Colors;

#endregion

namespace Pyrrha.Collections
{
    public class LayerCollection : RecordCollection<LayerTable, LayerTableRecord>
    {
        #region Properties

        public bool HasUnReconciledLayers
        {
            get { return this.RecordTable.HasUnreconciledLayers; }
        }

        public bool IncludeErased
        {
            get { return this._includeErased; }
            set
            {
                this.RecordTable = value ? (LayerTable)this.RecordTable.IncludingErased : null;
                this._includeErased = value;
            }
        }
        private bool _includeErased;

        public bool IncludeHidden
        {
            get { return this._includeHidden; }
            set
            {
                this.RecordTable = value ? this.RecordTable.IncludingHidden : null;
                this._includeHidden = value;
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
            this.RecordTable.GenerateUsageData();
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

        /// <summary>
        ///     Returns true if layer exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains( string name )
        {
            return base.RecordTable.Has( name );
        }

        public LayerTableRecord CreateLayer(string name, Color color, string linetypeName )
        {
            if (!RecordTable.Has(name))
            {
                var newRecord = new LayerTableRecord()
                {
                    Name = name,
                    Color = color,
                    LinetypeObjectId = ObjectManager.Document.Linetypes.Load(linetypeName)
                };

                RecordTable.Add(newRecord);
                Transaction.AddNewlyCreatedDBObject(newRecord, true);
            }
            return this[name];
        }

        #endregion
    }
}

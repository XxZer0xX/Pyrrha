#region Referenceing

using System.Linq;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Util;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

#endregion



namespace Pyrrha.Managers
{
    public class LayerManager
    {
        #region Properties

        private readonly Database _database;

        #endregion

        #region Constructor

        public LayerManager(Database database)
        {
            this._database = database;
        }

        #endregion

        #region CreateNewLayer Methods

        public Layer CreateNewLayer(string layerName)
        {
            return this._createNewLayer(layerName);
        }

        public Layer CreateNewLayer(string layerName, short color)
        {
            return this._createNewLayer(layerName, StaticExtenstions.GenerateAutoCadColor(color));
        }

        public Layer CreateNewLayer(string layerName, Color color)
        {
            return this._createNewLayer(layerName, color);
        }

        public Layer CreateNewLayer(string layerName, short color, string linetype)
        {
            return this._createNewLayer(layerName, StaticExtenstions.GenerateAutoCadColor(color), linetype);
        }

        public Layer CreateNewLayer(string layerName, Color color, string linetype)
        {
            return this._createNewLayer(layerName, color, linetype);
        }

        public Layer CreateNewLayer(string layerName, short color, string linetype, LineWeight lineWeight)
        {
            return this._createNewLayer(layerName, StaticExtenstions.GenerateAutoCadColor(color), linetype, lineWeight);
        }

        public Layer CreateNewLayer(string layerName, Color color, string linetype, LineWeight lineWeight)
        {
            return this._createNewLayer(layerName, color, linetype, lineWeight);
        }

        public Layer CreateNewLayer(
            string layerName,
            short color,
            string linetype,
            LineWeight lineWeight,
            ResultBuffer XData)
        {
            return this._createNewLayer(layerName, StaticExtenstions.GenerateAutoCadColor(color), linetype, lineWeight,
                                         XData);
        }

        public Layer CreateNewLayer(
            string layerName,
            Color color,
            string linetype,
            LineWeight lineWeight,
            ResultBuffer XData)
        {
            return this._createNewLayer(layerName, color, linetype, lineWeight, XData);
        }

        private Layer _createNewLayer(
            string layerName,
            Color color = null,
            string linetype = null,
            LineWeight? lineWeight = null,
            ResultBuffer XData = null)
        {
            Layer rtnLayer = null;
            using (OpenCloseTransaction trans = this._database.TransactionManager.StartOpenCloseTransaction())
            {
                var layerTable = (LayerTable)trans.GetObject(this._database.LayerTableId, OpenMode.ForWrite);
                if (layerTable.Has(layerName))
                {
                    StaticExtenstions.WriteToActiveDocument(
                        string.Format("\nlayer: {0} exists.", layerName)
                        );
                    layerTable.Dispose();
                    trans.Commit();
                    trans.Dispose();
                    return null;
                }

                var newLayerRecord = new LayerTableRecord
                {
                    Name = layerName,
                    Color = color ?? StaticExtenstions.GenerateAutoCadColor(7),
                    LineWeight = lineWeight ?? LineWeight.ByLineWeightDefault,
                    XData = XData,
                };

                if (!string.IsNullOrEmpty(linetype))
                    using (var linetypeTable = (LinetypeTable)trans.GetObject(
                                                this._database.LinetypeTableId, OpenMode.ForRead))
                    {
                        if (!linetypeTable.Has(linetype))
                            this._database.LoadLinetype(linetype);
                        newLayerRecord.LinetypeObjectId = linetypeTable[linetype];
                    }

                rtnLayer = new Layer { OriginalRecord = newLayerRecord };

                layerTable.Add(rtnLayer.OriginalRecord);
                trans.AddNewlyCreatedDBObject(rtnLayer.OriginalRecord, true);
                layerTable.Dispose();
                trans.Commit();
            }
            return rtnLayer;
        }

        #endregion

        #region Methods General

        public void PurgeLayerTable()
        {
            using (OpenCloseTransaction trans = this._database.TransactionManager.StartOpenCloseTransaction())
            {
                var layerTable = (LayerTable)trans.GetObject(this._database.LayerTableId, OpenMode.ForRead);

                var objIdCollection = new ObjectIdCollection(layerTable.Cast<ObjectId>().ToArray());

                this._database.Purge(objIdCollection);

                foreach (ObjectId objectId in objIdCollection)

                    using (DBObject layerRecord = objectId.Open(OpenMode.ForWrite)) layerRecord.Erase();

                trans.Commit();
            }
        }

        #endregion
    }
}
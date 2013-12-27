using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Pyrrha.Managers
{
    public class ModelSpaceManager
    {
        private readonly BlockTableRecord _modelSpace;
        private readonly Autodesk.AutoCAD.ApplicationServices.Document acDoc;
        #region Properties

        public Database Database
        {
            get { return _modelSpace.Database; }
        }

        /// <summary>
        ///     Current documents modelspace Object Id.
        /// </summary>
        public ObjectId ModelSpaceObjectId
        {
            get { return _modelSpace.ObjectId; }
        }


        #endregion

        #region Constructor

        public ModelSpaceManager(Autodesk.AutoCAD.ApplicationServices.Document docParam)
        {
            acDoc = docParam;

            using (var trans = docParam.Database.TransactionManager.StartOpenCloseTransaction())
            {
                _modelSpace =
                (BlockTableRecord)
                    trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(docParam.Database) , OpenMode.ForRead);
                trans.Commit();
            }

        }

        #endregion

        #region Methods

        public BlockReference CreateNewBlock(
           string definitionName ,
           Scale3d scale = default (Scale3d) ,
           Point3d pos = default(Point3d) ,
           string layerName = "0")
        {
            return acDoc.CreateNewBlock(definitionName , scale , pos , layerName);
        }

        public void AddEntity(IList<Entity> entityList)
        {
            using (var modelSpace = (BlockTableRecord)_modelSpace.ObjectId.Open(OpenMode.ForWrite))
            {
                using (var trans = modelSpace.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    foreach (var entity in entityList)
                    {
                        trans.AddNewlyCreatedDBObject(entity , true);
                        modelSpace.AppendEntity(entity);
                    }
                    trans.Commit();
                }
            }
        }

        public void CommitChanges(IList<Entity> entityList)
        {
            using (var trans = Database.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (Entity entity in entityList)
                {
                    var longHandle = long.Parse((string)entity
                        .GetXDataForApplication("PYRRHA")
                        .AsArray()[1].Value 
                            , NumberStyles.AllowHexSpecifier);

                    var handle = new Handle(longHandle);
                    var trueEntity = (Entity)trans.GetObject(Database.GetObjectId(false , handle , 0) 
                        , OpenMode.ForWrite);

                    trueEntity.CopyFrom(entity);
                    trueEntity.Dispose();
                    entity.Dispose();
                }
                trans.Commit();
            }
        }

        #endregion
    }
}

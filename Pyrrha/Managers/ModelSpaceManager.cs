using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Pyrrha.Util;

namespace Pyrrha.Managers
{
    public class ModelSpaceManager
    {
        private readonly BlockTableRecord _modelSpace;
        private readonly Autodesk.AutoCAD.ApplicationServices.Document _acDoc;

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
            _acDoc = docParam;

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
            return _acDoc.CreateNewBlock(definitionName , scale , pos , layerName);
        }

        public void AddEntity<T>(IList<T> entityList) where T : Entity
        {
            using (var doclock = _acDoc.LockDocument())
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

        public void CommitChanges<T>( IList<T> entityList ) where T :Entity
        {
            using (var trans = Database.TransactionManager.StartOpenCloseTransaction())
            {
                foreach (Entity entity in entityList)
                {
                    var trueEntity = trans.GetObject(
                     Database.GetObjectId(false , entity.GetHandle() , 0)
                     , OpenMode.ForWrite);

                    trueEntity.CopyFrom(entity);
                    trueEntity.Dispose();
                    entity.Dispose();
                }
                trans.Commit();
            }
        }

        public void EraseEntity(Entity ent)
        {
            using (var trans = Database.TransactionManager.StartOpenCloseTransaction())
            {
                var trueEntity = trans.GetObject(
                    Database.GetObjectId(false , ent.GetHandle() , 0)
                    , OpenMode.ForWrite);

                if (trueEntity == null) 
                    return;
                
                trueEntity.Erase(true);
                trueEntity.Dispose();

                trans.Commit();
            }
        }

        #endregion
    }
}

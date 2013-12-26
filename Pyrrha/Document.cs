#region Referenceing

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Pyrrha.Managers;
using Pyrrha.SelectionFilter;
using DbTransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;

#endregion

namespace Pyrrha
{
    #region Event Delegates

    public delegate void ModifiedEventHandler(object sender , EventArgs eventArgs);

    #endregion

    public class Document
    {
        private LayerManager _layerManager;
        private ModelSpaceManager _modelSpaceManager;

        #region Properties

        /// <summary>
        ///     The base document.
        /// </summary>
        public Autodesk.AutoCAD.ApplicationServices.Document OriginalDocument { get; set; }

        /// <summary>
        ///     Document Database
        /// </summary>
        public Database Database
        {
            get { return OriginalDocument.Database; }
        }

        /// <summary>
        ///     Document Editor.
        /// </summary>
        public Editor Editor
        {
            get { return OriginalDocument.Editor; }
        }

        /// <summary>
        ///     Document Editor.
        /// </summary>
        public DbTransactionManager TransactionManager
        {
            get { return OriginalDocument.TransactionManager; }
        }

        /// <summary>
        ///     Returns true if this drawing is active
        /// </summary>
        public bool IsActive
        {
            get { return OriginalDocument.IsActive; }
        }

        /// <summary>
        ///     Returns true if this documents is read only
        /// </summary>
        public bool IsReadOnly
        {
            get { return OriginalDocument.IsReadOnly; }
        }

        /// <summary>
        ///     Current documents LayerManager.
        /// </summary>
        public ModelSpaceManager ModelSpaceManager
        {
            get
            {
                return _modelSpaceManager ?? (_modelSpaceManager
                    = new ModelSpaceManager(OriginalDocument));
            }
            set { _modelSpaceManager = value; }
        }

        /// <summary>
        ///     Current documents LayerManager.
        /// </summary>
        public LayerManager LayerManager
        {
            get
            {
                return _layerManager ?? (_layerManager
                    = new LayerManager(Database));
            }
            set { _layerManager = value; }
        }

        /// <summary>
        ///     Current documents layers.
        /// </summary>
        public IList<Layer> Layers
        {
            get
            {
                using (OpenCloseTransaction trans = TransactionManager.StartOpenCloseTransaction())
                    return ((LayerTable)trans.GetObject(OriginalDocument.Database.LayerTableId , OpenMode.ForRead))
                        .Cast<ObjectId>()
                        .Select(objId =>
                        {
                            var newLayer = new Layer(((LayerTableRecord)trans.GetObject(objId , OpenMode.ForRead)));
                            newLayer.WillBeErased += (sender , args) => Layers.Remove(((Layer)sender));
                            return newLayer;
                        }).ToList();

            }
        }

        /// <summary>
        ///     Current documents text styles.
        /// </summary>
        public IList<TextStyle> TextStyles
        {
            get
            {
                using (OpenCloseTransaction trans = TransactionManager.StartOpenCloseTransaction())
                    return ((TextStyleTable)trans.GetObject(OriginalDocument.Database.LayerTableId , OpenMode.ForRead))
                        .Cast<ObjectId>()
                        .Select(objId => new TextStyle(((TextStyleTableRecord)trans.GetObject(objId , OpenMode.ForRead))))
                        .ToList();
            }
        }

        /// <summary>
        ///     All blocks in the modelspace
        /// </summary>
        public IList<BlockReference> Blocks
        {
            get
            {
                return _getEntities(new EntitySelectionFilter("INSERT"))
                    .Cast<BlockReference>()
                    .ToList();
            }
        }

        /// <summary>
        ///     All text in the modelspace
        /// </summary>
        public IList<Entity> DBText
        {
            get { return _getEntities( new EntitySelectionFilter("TEXT") ); }
        }


        /// <summary>
        ///     All text in the modelspace
        /// </summary>
        public IList<Entity> MText
        {
            get { return _getEntities(new EntitySelectionFilter("MTEXT")); }
        }

        /// <summary>
        ///     All text in the modelspace
        /// </summary>
        public IList<Entity> AllText
        {
            get { return DBText.Union(MText).ToList(); }
        }

        /// <summary>
        ///     All Entities in the modelspace
        /// </summary>
        public IList<Entity> ModelSpaceEntities
        {
            get { return _getEntities(); }
        }

        /// <summary>
        ///     Name of the current drawing.
        /// </summary>
        public string Name
        {
            get { return Path.GetFileName(OriginalDocument.Name); }
        }

        /// <summary>
        ///     Path to the current drawing.
        /// </summary>
        public string DrawingPath
        {
            get { return OriginalDocument.Name; }
        }

        #endregion

        #region Constructor

        public Document(Autodesk.AutoCAD.ApplicationServices.Document documentParameter)
        {
            OriginalDocument = documentParameter;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Regenerate the model.
        /// </summary>
        public void RegenModel()
        {
            OriginalDocument.Editor.Regen();
        }

        /// <summary>
        ///     Write a message to the editor
        /// </summary>
        /// <param name="message"></param>
        public void WriteMessage(string message)
        {
            OriginalDocument.Editor.WriteMessage(message);
        }

        /// <summary>
        ///     Execute command or lisp.
        /// </summary>
        public void SendStringToExecute(string command)
        {
            OriginalDocument.SendStringToExecute(command , true , false , true);
        }

        public void SendStringToExecute(string command , bool activate)
        {
            OriginalDocument.SendStringToExecute(command , activate , false , true);
        }

        public void SendStringToExecute(string command , bool activate , bool wrapUpInactiveDoc)
        {
            OriginalDocument.SendStringToExecute(command , activate , wrapUpInactiveDoc , true);
        }

        public void SendStringToExecute(string command , bool activate , bool wrapUpInactiveDoc , bool echoCommand)
        {
            OriginalDocument.SendStringToExecute(command , activate , wrapUpInactiveDoc , echoCommand);
        }

        /// <summary>
        ///     returns a lock for the document.
        /// </summary>
        /// <returns></returns>
        public DocumentLock LockDocument()
        {
            return OriginalDocument.LockDocument();
        }

        public DocumentLock LockDocument(DocumentLockMode lockMode , string globalCommandName , string localCommandName ,
            bool promptIfFails)
        {
            return OriginalDocument.LockDocument(lockMode , globalCommandName , localCommandName , promptIfFails);
        }

        /// <summary>
        ///     Send a command to execute synchronously.
        /// </summary>
        /// <param name="command"></param>
        public void SendCommandSynchronously(string command)
        {
            OriginalDocument.SendCommandSynchronously(command);


        }

        private IList<Entity> _getEntities(IEnumerable<EntitySelectionFilter> filterList = null)
        {
            var objIdList = new List<ObjectId>();
            foreach (var filter in filterList)
            {
                var objectIds = filter == null
                    ? Editor.SelectAll().Value.GetObjectIds()
                    : Editor.SelectAll(filter.Selection).Value.GetObjectIds();

                if (objectIds.Count() > 0)
                    objIdList.AddRange(objectIds);
            }

            var rtnList = new List<Entity>();
            using (var trans = TransactionManager.StartOpenCloseTransaction())
            {
                var regAppTable = (RegAppTable)trans.GetObject(Database.RegAppTableId , OpenMode.ForRead);
                if (!regAppTable.Has("PYRRHA"))
                {
                    using (var innerTrans = TransactionManager.StartOpenCloseTransaction())
                    {
                        regAppTable.UpgradeOpen();
                        var newAppRcd = new RegAppTableRecord { Name = "PYRRHA" };
                        regAppTable.Add(newAppRcd);
                        innerTrans.AddNewlyCreatedDBObject(newAppRcd , true);
                        innerTrans.Commit();
                    }
                }

                foreach (var objId in objIdList)
                {
                    using (var actualEntity = trans.GetObject(objId , OpenMode.ForRead))
                    {
                        var moddedEntity = (Entity)actualEntity.Clone();
                        var resBuffer = new ResultBuffer(
                            new TypedValue(1001 , "PYRRHA") , new TypedValue(1005 , actualEntity.Handle));
                        using (resBuffer)
                            moddedEntity.XData = resBuffer;
                        rtnList.Add(moddedEntity);
                    }
                }
                trans.Commit();
            }
            return rtnList.Count > 0 ? rtnList : null;
        }

        private IList<Entity> _getEntities(EntitySelectionFilter filter)
        {
            return _getEntities(new List<EntitySelectionFilter> {filter});
        }

        #endregion

        #region Event Setters

        // TODO 

        #endregion
    }

}
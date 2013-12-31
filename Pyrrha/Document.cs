#region Referenceing

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Pyrrha.Managers;
using Pyrrha.SelectionFilter;
using Pyrrha.Util;
using DbTransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

#endregion

namespace Pyrrha
{

    #region Event Delegates

    public delegate void ModifiedEventHandler( object sender, EventArgs eventArgs );

    #endregion

    public class Document
    {
        private LayerManager _layerManager;
        private ModelSpaceManager _modelSpaceManager;
        internal static Boolean InvokedFromScripting;

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
                return _modelSpaceManager ?? ( _modelSpaceManager
                    = new ModelSpaceManager( OriginalDocument ) );
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
                return _layerManager ?? ( _layerManager
                    = new LayerManager( Database ) );
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
                using ( OpenCloseTransaction trans = TransactionManager.StartOpenCloseTransaction() )
                    return ( (LayerTable) trans.GetObject( OriginalDocument.Database.LayerTableId, OpenMode.ForRead ) )
                        .Cast<ObjectId>()
                        .Select( objId =>
                        {
                            var newLayer = new Layer( ( (LayerTableRecord) trans.GetObject( objId, OpenMode.ForRead ) ) );
                            newLayer.WillBeErased += ( sender, args ) => Layers.Remove( ( (Layer) sender ) );
                            return newLayer;
                        } ).ToList();
            }
        }

        /// <summary>
        ///     Current documents text styles.
        /// </summary>
        public IList<TextStyle> TextStyles
        {
            get
            {
                using ( OpenCloseTransaction trans = TransactionManager.StartOpenCloseTransaction() )
                    return
                        ( (TextStyleTable) trans.GetObject( OriginalDocument.Database.LayerTableId, OpenMode.ForRead ) )
                            .Cast<ObjectId>()
                            .Select(
                                objId =>
                                    new TextStyle( ( (TextStyleTableRecord) trans.GetObject( objId, OpenMode.ForRead ) ) ) )
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
                return GetEntities( new EntitySelectionFilter( "INSERT" ) )
                    .Cast<BlockReference>()
                    .ToList();
            }
        }

        /// <summary>
        ///     All text in the modelspace
        /// </summary>
        public IList<Entity> DBText
        {
            get { return GetEntities( new EntitySelectionFilter( "TEXT" ) ); }
        }


        /// <summary>
        ///     All text in the modelspace
        /// </summary>
        public IList<Entity> MText
        {
            get { return GetEntities( new EntitySelectionFilter( "MTEXT" ) ); }
        }

        /// <summary>
        ///     All text in the modelspace
        /// </summary>
        public IList<Entity> AllText
        {
            get { return GetEntities( new EntitySelectionFilter( "*TEXT" ) ); }
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
            get { return Path.GetFileName( OriginalDocument.Name ); }
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

        public Document()
            : this( AcApp.DocumentManager.MdiActiveDocument ) {}

        public Document( string path ) : this( AcApp.DocumentManager.Open( path, false ) ) {}

        private Document( Autodesk.AutoCAD.ApplicationServices.Document documentParameter )
        {
            var exCon = Thread.CurrentThread.ExecutionContext;
            InvokedFromScripting = Thread.CurrentThread.IsScriptSource();
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
        public void WriteMessage( string message )
        {
            OriginalDocument.Editor.WriteMessage( message );
        }

        /// <summary>
        ///     Execute command or lisp.
        /// </summary>
        public void SendStringToExecute( string command )
        {
            OriginalDocument.SendStringToExecute( command, true, false, true );
        }

        public void SendStringToExecute( string command, bool activate )
        {
            OriginalDocument.SendStringToExecute( command, activate, false, true );
        }

        public void SendStringToExecute( string command, bool activate, bool wrapUpInactiveDoc )
        {
            OriginalDocument.SendStringToExecute( command, activate, wrapUpInactiveDoc, true );
        }

        public void SendStringToExecute( string command, bool activate, bool wrapUpInactiveDoc, bool echoCommand )
        {
            OriginalDocument.SendStringToExecute( command, activate, wrapUpInactiveDoc, echoCommand );
        }

        /// <summary>
        ///     returns a lock for the document.
        /// </summary>
        /// <returns></returns>
        public DocumentLock LockDocument()
        {
            return OriginalDocument.LockDocument();
        }

        public DocumentLock LockDocument( DocumentLockMode lockMode, string globalCommandName, string localCommandName,
            bool promptIfFails )
        {
            return OriginalDocument.LockDocument( lockMode, globalCommandName, localCommandName, promptIfFails );
        }

        /// <summary>
        ///     Send a command to execute synchronously.
        /// </summary>
        /// <param name="command"></param>
        public void SendCommandSynchronously( string command )
        {
            OriginalDocument.SendCommandSynchronously( command );
        }

        public void SaveAndCLose()
        {
            OriginalDocument.CloseAndSave( @"C\debug\text.dwg" );
        }

        public IList<Entity> GetEntities( EntitySelectionFilter filter )
        {
            return _getEntities( new List<EntitySelectionFilter> {filter} );
        }

        private IList<Entity> _getEntities( IEnumerable<EntitySelectionFilter> filterList = null )
        {
            var objIdList = new List<ObjectId>();
            if ( filterList == null )
            {
                PromptSelectionResult selection = Editor.SelectAll();
                if ( selection.Status == PromptStatus.Error )
                    return null;
                objIdList = selection.Value.GetObjectIds().ToList();
            }

            else
                foreach (var filter in filterList)
                {
                    // TODO throwing error here
                    var PRS = Editor.SelectAll(filter.Selection);
                    var ss = PRS.Value;
                    var objectIds = ss.GetObjectIds();
                    if (objectIds.Count() > 0)
                        objIdList.AddRange(objectIds);
                }

            var rtnList = new List<Entity>();
            using ( var regAppTable = (RegAppTable) Database.RegAppTableId.Open( OpenMode.ForRead ) )
                if ( !regAppTable.Has( "PYRRHA" ) )
                    using ( OpenCloseTransaction innerTrans = TransactionManager.StartOpenCloseTransaction() )
                    {
                        regAppTable.UpgradeOpen();
                        var newAppRcd = new RegAppTableRecord {Name = "PYRRHA"};
                        regAppTable.Add( newAppRcd );
                        innerTrans.AddNewlyCreatedDBObject( newAppRcd, true );
                        innerTrans.Commit();
                    }
            return StaticExtenstions.GetEntityClones( objIdList ) ?? null;
        }

        #endregion

        #region Event Setters

        // TODO 

        #endregion
    }
}
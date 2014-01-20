#region Referenceing

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsSystem;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Windows.Data;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using TransactionManager = Pyrrha.OverriddenClasses.TransactionManager;
using Transaction = Pyrrha.OverriddenClasses.Transaction;

#endregion
#pragma warning disable 612,618

namespace Pyrrha
{
    public class PyrrhaDocument : MarshalByRefObject , IDisposable
    {
        #region Properties

        private readonly Document _document;

        private readonly OpenCloseTransaction _innerTransaction;

        public BlockTableRecord ModelSpace
        {
            get
            {
                return (BlockTableRecord)SymbolUtilityServices.GetBlockModelSpaceId( 
                    Database ).Open(OpenMode.ForWrite) ;

            }
        }

        public BlockTableRecord PaperSpace
        {
            get
            {
                return (BlockTableRecord)SymbolUtilityServices.GetBlockPaperSpaceId(
                    Database).Open(OpenMode.ForWrite);
            }
        }

        public LayerTable LayerTable
        {
            get { return (LayerTable) Database.LayerTableId.Open(OpenMode.ForRead); }
        }

        public TextStyleTable TextStyleTable
        {
            get { return (TextStyleTable)Database.TextStyleTableId.Open(OpenMode.ForRead); }
        }

        public LinetypeTable LinetypeTable
        {
            get { return (LinetypeTable)Database.LinetypeTableId.Open(OpenMode.ForRead); }
        }

        public IEnumerable<LayerTableRecord> Layers
        {
            get
            {
                return
                    LayerTable.Cast<ObjectId>()
                        .Select( objId => _innerTransaction.GetOpenObject<LayerTableRecord>( objId ) );
            }
        }

        private TransactionManager TransactionManager
        {
            get { return _document.TransactionManager; }
        }

        #endregion

        #region Construcotrs

        public PyrrhaDocument()
            : this( AcApp.DocumentManager.MdiActiveDocument ) {}

        public PyrrhaDocument( string path )
            : this( () =>
                AcApp.DocumentManager.Cast<Document>()
                    .FirstOrDefault( doc => doc.Name.Equals( Path.GetFileName( path ) ) )
                ?? AcApp.DocumentManager.Open( path, false, null ) ) {}

        public PyrrhaDocument( Func<Document> func )
            : this( func() ) {}

        private PyrrhaDocument( Document doc )
        {
            if ( doc == null )
                throw new NullReferenceException( "Document is null." );
            _document = doc;
        }

        #endregion

        #region Autocad Document Implementation

        #region Properties

        public object AcadDocument
        {
            get { return _document.AcadDocument; }
        }

        public string CommandInProgress
        {
            get { return _document.CommandInProgress; }
        }

        public Database Database
        {
            get { return _document.Database; }
        }

        public Editor Editor
        {
            get { return _document.Editor; }
        }

        public Manager GraphicsManager
        {
            get { return _document.GraphicsManager; }
        }

        public bool IsActive
        {
            get { return _document.IsActive; }
        }

        public bool IsReadOnly
        {
            get { return _document.IsReadOnly; }
        }

        public string Name
        {
            get { return _document.Name; }
        }

        public StatusBar StatusBar
        {
            get { return _document.StatusBar; }
        }

        public Hashtable UserData
        {
            get { return _document.UserData; }
        }

        public Window Window
        {
            get { return _document.Window; }
        }

        #endregion

        #region Methods

        public Bitmap CapturePreviewImage( uint width, uint height )
        {
            return _document.CapturePreviewImage( width, height );
        }

        public void CloseAndDiscard()
        {
            _document.CloseAndDiscard();
        }

        public void CloseAndSave( string fileName )
        {
            _document.CloseAndSave( fileName );
        }

        public static Document Create( IntPtr unmanagedPointer )
        {
            return Document.Create( unmanagedPointer );
        }

        public void DowngradeDocOpen( bool bPromptForSave )
        {
            _document.DowngradeDocOpen( bPromptForSave );
        }

        public static Document FromAcadDocument( object acadDocument )
        {
            return Document.FromAcadDocument( acadDocument );
        }

        public DocumentLock LockDocument()
        {
            return _document.LockDocument();
        }

        public DocumentLock LockDocument( DocumentLockMode lockMode, string globalCommandName, string localCommandName,
            bool promptIfFails )
        {
            return _document.LockDocument( lockMode, globalCommandName, localCommandName, promptIfFails );
        }

        public DocumentLockMode LockMode()
        {
            return _document.LockMode();
        }

        public DocumentLockMode LockMode( bool bIncludeMyLocks )
        {
            return _document.LockMode( bIncludeMyLocks );
        }

        public void PopDbmod()
        {
            _document.PopDbmod();
        }

        public void PushDbmod()
        {
            _document.PushDbmod();
        }

        public void SendStringToExecute( string command, bool activate, bool wrapUpInactiveDoc, bool echoCommand )
        {
            _document.SendStringToExecute( command, activate, wrapUpInactiveDoc, echoCommand );
        }

        public Database TryGetDatabase()
        {
            return _document.TryGetDatabase();
        }

        public void UpgradeDocOpen()
        {
            _document.UpgradeDocOpen();
        }

        #endregion

        #region Events

        private event DisposingEventHandler _beginDocumentDispose;
        
        public event DisposingEventHandler BeginDocumentDispose
        {
            add { _beginDocumentDispose += value; }
            remove { _beginDocumentDispose -= value; }
        }

        public event DocumentBeginCloseEventHandler BeginDocumentClose
        {
            add { _document.BeginDocumentClose += value; }
            remove { _document.BeginDocumentClose -= value; }
        }

        public event DrawingOpenEventHandler BeginDwgOpen
        {
            add { _document.BeginDwgOpen += value; }
            remove { _document.BeginDwgOpen -= value; }
        }

        public event EventHandler CloseAborted
        {
            add { _document.CloseAborted += value; }
            remove { _document.CloseAborted -= value; }
        }

        public event EventHandler CloseWillStart
        {
            add { _document.CloseWillStart += value; }
            remove { _document.CloseWillStart -= value; }
        }

        public event CommandEventHandler CommandCancelled
        {
            add { _document.CommandCancelled += value; }
            remove { _document.CommandCancelled -= value; }
        }

        public event CommandEventHandler CommandEnded
        {
            add { _document.CommandEnded += value; }
            remove { _document.CommandEnded -= value; }
        }

        public event CommandEventHandler CommandFailed
        {
            add { _document.CommandFailed += value; }
            remove { _document.CommandFailed -= value; }
        }

        public event CommandEventHandler CommandWillStart
        {
            add { _document.CommandWillStart += value; }
            remove { _document.CommandWillStart -= value; }
        }

        public event DrawingOpenEventHandler EndDwgOpen
        {
            add { _document.EndDwgOpen += value; }
            remove { _document.EndDwgOpen -= value; }
        }

        public event EventHandler ImpliedSelectionChanged
        {
            add { _document.ImpliedSelectionChanged += value; }
            remove { _document.ImpliedSelectionChanged -= value; }
        }

        public event EventHandler LispCancelled
        {
            add { _document.LispCancelled += value; }
            remove { _document.LispCancelled -= value; }
        }

        public event EventHandler LispEnded
        {
            add { _document.LispEnded += value; }
            remove { _document.LispEnded -= value; }
        }

        public event LispWillStartEventHandler LispWillStart
        {
            add { _document.LispWillStart += value; }
            remove { _document.LispWillStart -= value; }
        }

        public event UnknownCommandEventHandler UnknownCommand
        {
            add { _document.UnknownCommand += value; }
            remove { _document.UnknownCommand -= value; }
        }

        public event EventHandler ViewChanged
        {
            add { _document.ViewChanged += value; }
            remove { _document.ViewChanged -= value; }
        }

        #endregion

        #region DisposableWrapper implementation

        public void Dispose()
        {
            if (_beginDocumentDispose != null)
                _beginDocumentDispose(this , new EventArgs());
            _document.Dispose();
        }

        #endregion

        #region IEqualityComparer implementation

        public override bool Equals(object obj)
        {
            return ((Document)obj).Name.Equals(Name , StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (_document != null ? _document.GetHashCode() : 0);
            }
        }

        #endregion

        #endregion
    }
}
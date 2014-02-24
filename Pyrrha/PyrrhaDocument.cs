#region Referencing

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsSystem;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Windows.Data;
using Pyrrha.Runtime;
using Pyrrha.Runtime.Exception;
using Pyrrha.Util;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

#endregion
#pragma warning disable 612,618

namespace Pyrrha
{
    public sealed class PyrrhaDocument : MarshalByRefObject , IDisposable
    {
        #region Properties

        private readonly Document _document;
        private OpenObjectManager _objectManager;
        public DocumentManager DocumentManager;

        public OpenObjectManager ObjectManager
        {
            get
            {
                return this._objectManager ??
                       (this._objectManager = new OpenObjectManager());
            }
        } 

        public BlockTableRecord ModelSpace
        {
            get
            {
                return (BlockTableRecord) this.ObjectManager.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(this.Database));
            }
        }

        public BlockTableRecord PaperSpace
        {
            get
            {
                return (BlockTableRecord) this.ObjectManager.GetObject(
                    SymbolUtilityServices.GetBlockPaperSpaceId(this.Database));     
            }
        }

        public LayerTable LayerTable
        {
            get { return (LayerTable) this.ObjectManager.GetObject(this.Database.LayerTableId); }
        }

        public TextStyleTable TextStyleTable
        {
            get { return (TextStyleTable) this.ObjectManager.GetObject(this.Database.TextStyleTableId); }
        }

        public LinetypeTable LinetypeTable
        {
            get { return (LinetypeTable) this.ObjectManager.GetObject(this.Database.LinetypeTableId); }
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
            PyrrhaException.IsScriptSource = Thread.CurrentThread.IsScriptSource();
            this._document = doc;
            DocumentManager.AddDocument( this );
        }

        #endregion

        #region Methods

        public void ConfirmAllChanges()
        {
            this.ObjectManager.ConfirmAllChanges();
        }

        #endregion

        #region Autocad Document Implementation

        #region Properties

        public object AcadDocument
        {
            get { return this._document.AcadDocument; }
        }

        public string CommandInProgress
        {
            get { return this._document.CommandInProgress; }
        }

        public Database Database
        {
            get { return this._document.Database; }
        }

        public Editor Editor
        {
            get { return this._document.Editor; }
        }

        public Manager GraphicsManager
        {
            get { return this._document.GraphicsManager; }
        }

        public bool IsActive
        {
            get { return this._document.IsActive; }
        }

        public bool IsReadOnly
        {
            get { return this._document.IsReadOnly; }
        }

        public string Name
        {
            get { return this._document.Name; }
        }

        public StatusBar StatusBar
        {
            get { return this._document.StatusBar; }
        }

        public Hashtable UserData
        {
            get { return this._document.UserData; }
        }

        public Window Window
        {
            get { return this._document.Window; }
        }

        #endregion

        #region Methods

        public Bitmap CapturePreviewImage( uint width, uint height )
        {
            return this._document.CapturePreviewImage( width, height );
        }

        public void CloseAndDiscard()
        {
            this._document.CloseAndDiscard();
        }

        public void CloseAndSave( string fileName )
        {
            this._document.CloseAndSave( fileName );
        }

        public static Document Create( IntPtr unmanagedPointer )
        {
            return Document.Create( unmanagedPointer );
        }

        public void DowngradeDocOpen( bool bPromptForSave )
        {
            this._document.DowngradeDocOpen( bPromptForSave );
        }

        public static Document FromAcadDocument( object acadDocument )
        {
            return Document.FromAcadDocument( acadDocument );
        }

        public DocumentLock LockDocument()
        {
            return this._document.LockDocument();
        }

        public DocumentLock LockDocument( DocumentLockMode lockMode, string globalCommandName, string localCommandName,
            bool promptIfFails )
        {
            return this._document.LockDocument( lockMode, globalCommandName, localCommandName, promptIfFails );
        }

        public DocumentLockMode LockMode()
        {
            return this._document.LockMode();
        }

        public DocumentLockMode LockMode( bool bIncludeMyLocks )
        {
            return this._document.LockMode( bIncludeMyLocks );
        }

        public void PopDbmod()
        {
            this._document.PopDbmod();
        }

        public void PushDbmod()
        {
            this._document.PushDbmod();
        }

        public void SendStringToExecute( string command, bool activate, bool wrapUpInactiveDoc, bool echoCommand )
        {
            this._document.SendStringToExecute( command, activate, wrapUpInactiveDoc, echoCommand );
        }

        public Database TryGetDatabase()
        {
            return this._document.TryGetDatabase();
        }

        public void UpgradeDocOpen()
        {
            this._document.UpgradeDocOpen();
        }

        #endregion

        #region Events

        private event DisposingEventHandler _beginDocumentDispose;
        
        public event DisposingEventHandler BeginDocumentDispose
        {
            add { this._beginDocumentDispose += value; }
            remove { this._beginDocumentDispose -= value; }
        }

        public event DocumentBeginCloseEventHandler BeginDocumentClose
        {
            add { this._document.BeginDocumentClose += value; }
            remove { this._document.BeginDocumentClose -= value; }
        }

        public event DrawingOpenEventHandler BeginDwgOpen
        {
            add { this._document.BeginDwgOpen += value; }
            remove { this._document.BeginDwgOpen -= value; }
        }

        public event EventHandler CloseAborted
        {
            add { this._document.CloseAborted += value; }
            remove { this._document.CloseAborted -= value; }
        }

        public event EventHandler CloseWillStart
        {
            add { this._document.CloseWillStart += value; }
            remove { this._document.CloseWillStart -= value; }
        }

        public event CommandEventHandler CommandCancelled
        {
            add { this._document.CommandCancelled += value; }
            remove { this._document.CommandCancelled -= value; }
        }

        public event CommandEventHandler CommandEnded
        {
            add { this._document.CommandEnded += value; }
            remove { this._document.CommandEnded -= value; }
        }

        public event CommandEventHandler CommandFailed
        {
            add { this._document.CommandFailed += value; }
            remove { this._document.CommandFailed -= value; }
        }

        public event CommandEventHandler CommandWillStart
        {
            add { this._document.CommandWillStart += value; }
            remove { this._document.CommandWillStart -= value; }
        }

        public event DrawingOpenEventHandler EndDwgOpen
        {
            add { this._document.EndDwgOpen += value; }
            remove { this._document.EndDwgOpen -= value; }
        }

        public event EventHandler ImpliedSelectionChanged
        {
            add { this._document.ImpliedSelectionChanged += value; }
            remove { this._document.ImpliedSelectionChanged -= value; }
        }

        public event EventHandler LispCancelled
        {
            add { this._document.LispCancelled += value; }
            remove { this._document.LispCancelled -= value; }
        }

        public event EventHandler LispEnded
        {
            add { this._document.LispEnded += value; }
            remove { this._document.LispEnded -= value; }
        }

        public event LispWillStartEventHandler LispWillStart
        {
            add { this._document.LispWillStart += value; }
            remove { this._document.LispWillStart -= value; }
        }

        public event UnknownCommandEventHandler UnknownCommand
        {
            add { this._document.UnknownCommand += value; }
            remove { this._document.UnknownCommand -= value; }
        }

        public event EventHandler ViewChanged
        {
            add { this._document.ViewChanged += value; }
            remove { this._document.ViewChanged -= value; }
        }

        #endregion

        #region DisposableWrapper implementation

        public void Dispose()
        {
            if (this._beginDocumentDispose != null)
                this._beginDocumentDispose(this , new EventArgs());
            this._document.Dispose();
        }

        #endregion

        #region IEqualityComparer implementation

        public override bool Equals(object obj)
        {
            if ( ReferenceEquals( null , obj ) )
                return false;
            if ( ReferenceEquals( this , obj ) )
                return true;
            return obj.GetType() == this.GetType() && this.Equals( (PyrrhaDocument) obj );
        }

        public bool Equals(PyrrhaDocument other)
        {
            return Equals(this._document, other._document) 
                && Equals(this._objectManager, other._objectManager);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this._document != null ? this._document.GetHashCode() : 0) * 397) ^ (this._objectManager != null ? this._objectManager.GetHashCode() : 0);
            }
        }

        #endregion

        #endregion
    }
}
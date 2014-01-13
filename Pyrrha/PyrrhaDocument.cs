#region Referenceing

using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsSystem;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using TransactionManager = Pyrrha.OverriddenClasses.TransactionManager;

#endregion

namespace Pyrrha
{
    public sealed class PyrrhaDocument : DisposableWrapper
    {
        #region Properties

        private readonly Document _document;

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

        public TransactionManager TransactionManager
        {
            get { return new TransactionManager( _document.TransactionManager ); }
        }

        public Hashtable UserData
        {
            get { return _document.UserData; }
        }

        public Window Window
        {
            get { return _document.Window; }
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

        #region DisposableWrapper implementation

        protected override void DeleteUnmanagedObject() {}

        #endregion
    }
}
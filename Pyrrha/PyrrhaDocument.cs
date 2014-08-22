using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;

#region Referencing

using ErrorStatus = Autodesk.AutoCAD.Runtime.ErrorStatus;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using Color = Autodesk.AutoCAD.Colors.Color;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsSystem;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Windows.Data;
using Pyrrha.Attributes;
using Pyrrha.Collections;
using System;
using System.Collections;
using System.IO;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System.Linq;
using System.Drawing;
using Autodesk.AutoCAD.Colors;

#endregion
//#pragma warning disable 612,618

namespace Pyrrha
{
    public sealed class PyrrhaDocument : MarshalByRefObject, IDisposable
    {
        #region Properties

        internal readonly Document BaseDocument;

        public DocumentManager DocumentManager
        {
            get { return this._documentManager ?? (this._documentManager = new DocumentManager()); }
        }
        private DocumentManager _documentManager;

        [ScriptingProperty]
        public OpenObjectManager ObjectManager
        {
            get
            {
                return this._objectManager ??
                       (this._objectManager = new OpenObjectManager(this));
            }
        }
        private OpenObjectManager _objectManager;

        public BlockTableRecord ModelSpace
        {
            get
            {
                return null; //(BlockTableRecord) ObjectManager.GetObject(
                //SymbolUtilityServices.GetBlockModelSpaceId(Database));
            }
        }
        public BlockTableRecord PaperSpace
        {
            get
            {
                return null;// (BlockTableRecord)ObjectManager.GetObject(
                //SymbolUtilityServices.GetBlockPaperSpaceId(Database));     
            }
        }

        [ScriptingProperty]
        public LayerCollection Layers
        {
            get { return this._layers ?? (this._layers = new LayerCollection(this, OpenMode.ForWrite)); }
            set { this._layers = value; }
        }
        private LayerCollection _layers;

        [ScriptingProperty]
        public TextStyleCollection TextStyles
        {
            get { return this._textstyles ?? (this._textstyles = new TextStyleCollection(this, OpenMode.ForWrite)); }
            set { this._textstyles = value; }
        }
        private TextStyleCollection _textstyles;

        [ScriptingProperty]
        public LinetypeCollection Linetypes
        {
            get { return this._linetypes ?? (this._linetypes = new LinetypeCollection(this, OpenMode.ForWrite)); }
            set { this._linetypes = value; }
        }
        private LinetypeCollection _linetypes;

        #endregion

        #region Constructors

        public PyrrhaDocument()
            : this(AcApp.DocumentManager.MdiActiveDocument) { }

        public PyrrhaDocument(string path)
            : this(FindDocument(path)) { }

        public PyrrhaDocument(Func<Document> func)
            : this(func()) { }

        private PyrrhaDocument(Document doc)
        {
            if (doc == null)
                throw new NullReferenceException("Document is null.");
            //_loadAllLinetypes(doc);
            //PyrrhaException.IsScriptSource = Thread.CurrentThread.IsScriptSource();
            this.BaseDocument = doc;
            DocumentManager.AddDocument(this);
            
        }

        // This is safer because it actually checks full drawing paths.
        // AutoCAD allows ambiguous drawing names in a single session,
        // so this checks for the fully qualified drawing, and opens it
        // if not available.
        public static Document FindDocument(string path)
        {
            var name = Path.GetFileName(path);
            var host = HostApplicationServices.Current;

            foreach (Document doc in AcApp.DocumentManager)
            {
                try
                {
                    var docName = host.FindFile(name, doc.Database, FindFileHint.Default);
                    if (docName.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                        return doc;
                }
                catch (Exception ex)
                {
                    if (ex.ErrorStatus == ErrorStatus.FilerError)
                        continue;
                    throw;
                }
            }

            return AcApp.DocumentManager.Open(path, false);
        }

        #endregion

        #region Methods

        [ScriptingMethod]
        public void ConfirmAllChanges()
        {
            this.ObjectManager.CommitAll();
        }

        public IList<Entity> ExecuteQuery(string query)
        {
            throw new NotImplementedException();
        }

        public IList<Entity> Select(string selectQuery)
        {
            throw new NotImplementedException();
        }

        public bool Update(string updateQuery)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string deleteQuery)
        {
            throw new NotImplementedException();
        }



        #endregion

        #region Scripting Methods

        #region Editor

        [ScriptingMethod]
        public void Write(object message)
        {
            this.Editor.WriteMessage(string.Format("\n{0}\n", message));
        }

        [ScriptingMethod]
        public double? GetDistance(string message) 
        {
            var result = this.Editor.GetDistance(message);
            return result.Status.Equals(PromptStatus.OK) ? new double?(result.Value) : null;
        }

        [ScriptingMethod]
        public double? GetDouble(string message) 
        {
            var result = this.Editor.GetDouble(message);
            return result.Status.Equals(PromptStatus.OK) ? new double?(result.Value) : null;
        }

        [ScriptingMethod]
        public Entity GetEntity(string message)
        {
            throw new NotImplementedException();
        }

        [ScriptingMethod]
        public string GetFileNameForOpen(string message) 
        {
            return this.Editor.GetFileNameForOpen(message).StringResult;
        }

        [ScriptingMethod]
        public string GetFileNameForSave(string message)
        {
            return this.Editor.GetFileNameForSave(message).StringResult;
        }

        [ScriptingMethod]
        public int? GetInteger(string message)
        {
            var result = this.Editor.GetInteger(message);
            return result.Status.Equals(PromptStatus.OK) ? new int?(result.Value) : null;
        }

        //public PromptNestedEntityResult GetNestedEntity(PromptNestedEntityOptions options) { return null; }
        //public PromptNestedEntityResult GetNestedEntity(string message) { return null; }
        
        [ScriptingMethod]
        public Point3d? GetPoint(string message) 
        {
            var result = this.Editor.GetPoint(message);
            return result.Status.Equals(PromptStatus.OK) ? new Point3d?(result.Value) : null;
        }

        [ScriptingMethod]
        public string GetString(string message,bool spacesAllowed)
        {
            return this.Editor.GetString(new PromptStringOptions(message) { AllowSpaces = spacesAllowed }).StringResult;
        }

        [ScriptingMethod]
        public void Regen()
        {
            Editor.Regen();
        }

        #endregion

        #region Layers

        [ScriptingMethod]
        public LayerTableRecord CreateLayer(string name, int colorIndex, string linetype)
        {
            var color = Color.FromColorIndex(ColorMethod.ByAci, (short)colorIndex);
            return Layers.CreateLayer(name, color, linetype);
        }

        
        [ScriptingMethod]
        public Color FromColorIndex(int colorIndex)
        {
            return Color.FromColorIndex(ColorMethod.ByAci, (short)colorIndex);
        }

        #endregion

        #region Application

        [ScriptingMethod]
        public void SetVar(string varName, object value)
        {
            Application.SetSystemVariable( varName , value );
        }

        [ScriptingMethod]
        public object GetVar(string varName)
        {
            return Application.GetSystemVariable( varName );
        }

        [ScriptingMethod]
        public void Alert( string message )
        {
            Application.ShowAlertDialog( message );
        }

        [ScriptingMethod]
        public void Update()
        {
            Application.UpdateScreen();
        }

        #endregion

        #endregion

        #region Private Supporting Methods

        

        #endregion

        #region Autocad Document Implementation

        #region Properties

        public object AcadDocument
        {
            get { return this.BaseDocument.AcadDocument; }
        }

        public string CommandInProgress
        {
            get { return this.BaseDocument.CommandInProgress; }
        }

        [ScriptingProperty]
        public Database Database
        {
            get { return this.BaseDocument.Database; }
        }

        [ScriptingProperty]
        public Editor Editor
        {
            get { return this.BaseDocument.Editor; }
        }

        [ScriptingProperty]
        public Manager GraphicsManager
        {
            get { return this.BaseDocument.GraphicsManager; }
        }

        public bool IsActive
        {
            get { return this.BaseDocument.IsActive; }
        }

        [ScriptingProperty]
        public bool IsReadOnly
        {
            get { return this.BaseDocument.IsReadOnly; }
        }

        [ScriptingProperty]
        public string Name
        {
            get { return this.BaseDocument.Name; }
        }

        [ScriptingProperty]
        public StatusBar StatusBar
        {
            get { return this.BaseDocument.StatusBar; }
        }

        public Hashtable UserData
        {
            get { return this.BaseDocument.UserData; }
        }

        public Window Window
        {
            get { return this.BaseDocument.Window; }
        }

        #endregion

        #region Methods

        public Bitmap CapturePreviewImage(int width, int height)
        {
            return this.BaseDocument.CapturePreviewImage((uint)width, (uint)height);
        }

        [ScriptingMethod]
        public void CloseAndDiscard()
        {
            this.BaseDocument.CloseAndDiscard();
        }

        [ScriptingMethod]
        public void CloseAndSave()
        {
            this.BaseDocument.CloseAndSave(this.Name);
        }

        public static Document Create(IntPtr unmanagedPointer)
        {
            return Document.Create(unmanagedPointer);
        }

        public void DowngradeDocOpen(bool bPromptForSave)
        {
            this.BaseDocument.DowngradeDocOpen(bPromptForSave);
        }

        public static Document FromAcadDocument(object acadDocument)
        {
            return Document.FromAcadDocument(acadDocument);
        }

        [ScriptingMethod]
        public DocumentLock LockDocument()
        {
            return this.BaseDocument.LockDocument();
        }

        public DocumentLock LockDocument(DocumentLockMode lockMode, string globalCommandName, string localCommandName,
            bool promptIfFails)
        {
            return this.BaseDocument.LockDocument(lockMode, globalCommandName, localCommandName, promptIfFails);
        }

        public DocumentLockMode LockMode()
        {
            return this.BaseDocument.LockMode();
        }

        public DocumentLockMode LockMode(bool bIncludeMyLocks)
        {
            return this.BaseDocument.LockMode(bIncludeMyLocks);
        }

        public void PopDbmod()
        {
            this.BaseDocument.PopDbmod();
        }

        public void PushDbmod()
        {
            this.BaseDocument.PushDbmod();
        }

        public void SendStringToExecute(string command, bool activate, bool wrapUpInactiveDoc, bool echoCommand)
        {
            this.BaseDocument.SendStringToExecute(command, activate, wrapUpInactiveDoc, echoCommand);
        }

        public Database TryGetDatabase()
        {
            return this.BaseDocument.TryGetDatabase();
        }

        public void UpgradeDocOpen()
        {
            this.BaseDocument.UpgradeDocOpen();
        }

        #endregion

        #region Events

        public event DisposingEventHandler BeginDocumentDispose
        {
            add { this._beginDocumentDispose += value; }
            remove { this._beginDocumentDispose -= value; }
        }
        private event DisposingEventHandler _beginDocumentDispose;

        public event DocumentBeginCloseEventHandler BeginDocumentClose
        {
            add { this.BaseDocument.BeginDocumentClose += value; }
            remove { this.BaseDocument.BeginDocumentClose -= value; }
        }

        public event DrawingOpenEventHandler BeginDwgOpen
        {
            add { this.BaseDocument.BeginDwgOpen += value; }
            remove { this.BaseDocument.BeginDwgOpen -= value; }
        }

        public event EventHandler CloseAborted
        {
            add { this.BaseDocument.CloseAborted += value; }
            remove { this.BaseDocument.CloseAborted -= value; }
        }

        public event EventHandler CloseWillStart
        {
            add { this.BaseDocument.CloseWillStart += value; }
            remove { this.BaseDocument.CloseWillStart -= value; }
        }

        public event CommandEventHandler CommandCancelled
        {
            add { this.BaseDocument.CommandCancelled += value; }
            remove { this.BaseDocument.CommandCancelled -= value; }
        }

        public event CommandEventHandler CommandEnded
        {
            add { this.BaseDocument.CommandEnded += value; }
            remove { this.BaseDocument.CommandEnded -= value; }
        }

        public event CommandEventHandler CommandFailed
        {
            add { this.BaseDocument.CommandFailed += value; }
            remove { this.BaseDocument.CommandFailed -= value; }
        }

        public event CommandEventHandler CommandWillStart
        {
            add { this.BaseDocument.CommandWillStart += value; }
            remove { this.BaseDocument.CommandWillStart -= value; }
        }

        public event DrawingOpenEventHandler EndDwgOpen
        {
            add { this.BaseDocument.EndDwgOpen += value; }
            remove { this.BaseDocument.EndDwgOpen -= value; }
        }

        public event EventHandler ImpliedSelectionChanged
        {
            add { this.BaseDocument.ImpliedSelectionChanged += value; }
            remove { this.BaseDocument.ImpliedSelectionChanged -= value; }
        }

        public event EventHandler LispCancelled
        {
            add { this.BaseDocument.LispCancelled += value; }
            remove { this.BaseDocument.LispCancelled -= value; }
        }

        public event EventHandler LispEnded
        {
            add { this.BaseDocument.LispEnded += value; }
            remove { this.BaseDocument.LispEnded -= value; }
        }

        public event LispWillStartEventHandler LispWillStart
        {
            add { this.BaseDocument.LispWillStart += value; }
            remove { this.BaseDocument.LispWillStart -= value; }
        }

        public event UnknownCommandEventHandler UnknownCommand
        {
            add { this.BaseDocument.UnknownCommand += value; }
            remove { this.BaseDocument.UnknownCommand -= value; }
        }

        public event EventHandler ViewChanged
        {
            add { this.BaseDocument.ViewChanged += value; }
            remove { this.BaseDocument.ViewChanged -= value; }
        }

        #endregion

        #region DisposableWrapper implementation

        public void Dispose()
        {
            if (this._beginDocumentDispose != null)
                this._beginDocumentDispose(this, new EventArgs());

            this.ObjectManager.Dispose();
        }

        #endregion

        #region IEqualityComparer implementation

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == this.GetType() && this.Equals((PyrrhaDocument)obj);
        }

        internal bool Equals(PyrrhaDocument other)
        {
            return Equals(this.BaseDocument, other.BaseDocument)
                && Equals(this._objectManager, other._objectManager);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.BaseDocument != null ? this.BaseDocument.GetHashCode() : 0) * 397);
            }
        }

        #endregion

        #endregion
    }
}


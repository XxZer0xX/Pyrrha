#region Referencing

using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsSystem;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Windows.Data;
using Pyrrha.Collections;

using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.Geometry;
using Pyrrha.Attributes;

#endregion
//#pragma warning disable 612,618

namespace Pyrrha
{
    public sealed partial class PyrrhaDocument : MarshalByRefObject, IDisposable
    {
        #region Properties

        internal readonly Document BaseDocument;

        public DocumentManager DocumentManager
        {
            get { return _documentManager ?? (_documentManager = new DocumentManager()); }
        }
        private DocumentManager _documentManager;

        [Scripting]
        public OpenObjectManager ObjectManager
        {
            get
            {
                return _objectManager ??
                       (_objectManager = new OpenObjectManager(Database));
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

        [Scripting]
        public LayerCollection Layers
        {
            get { return _layers ?? (_layers = new LayerCollection(this, OpenMode.ForWrite)); }
            set { _layers = value; }
        }
        private LayerCollection _layers;

        [Scripting]
        public TextStyleCollection TextStyles
        {
            get { return _textstyles ?? (_textstyles = new TextStyleCollection(this, OpenMode.ForWrite)); }
            set { _textstyles = value; }
        }
        private TextStyleCollection _textstyles;

        [Scripting]
        public LinetypeCollection Linetypes
        {
            get { return _linetypes ?? (_linetypes = new LinetypeCollection(this, OpenMode.ForWrite)); }
            set { _linetypes = value; }
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
            //PyrrhaException.IsScriptSource = Thread.CurrentThread.IsScriptSource();
            BaseDocument = doc;
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
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    if (ex.ErrorStatus == Autodesk.AutoCAD.Runtime.ErrorStatus.FilerError)
                        continue;
                    throw;
                }
            }

            return AcApp.DocumentManager.Open(path, false);
        }

        #endregion

        #region Methods

        [Scripting]
        public void ConfirmAllChanges()
        {
            ObjectManager.CommitAll();
        }

        #endregion

        #region Scripting Methods

        #region Editor

        [Scripting]
        public void Print(object message)
        {
            Editor.WriteMessage(string.Format("\n{0}\n", message));
        }

        [Scripting]
        public double? GetDistance() 
        {
            return GetDistance("\nPlease select two points: ");
        }

        [Scripting]
        public double? GetDistance(string message) 
        {
            var result = Editor.GetDistance(message);
            return result.Status.Equals(PromptStatus.OK) ? new double?(result.Value) : null;
        }

        [Scripting]
        public double? GetDouble() 
        {
            return GetDouble("\nPlease input number: ");
        }

        [Scripting]
        public double? GetDouble(string message) 
        {
            var result = Editor.GetDouble(message);
            return result.Status.Equals(PromptStatus.OK) ? new double?(result.Value) : null;
        }
        [Scripting]
        public Entity GetEntity() 
        {
            throw new NotImplementedException();
        }
        [Scripting]
        public Entity GetEntity(string message)
        {
            throw new NotImplementedException();
        }
        [Scripting]
        public string GetFileNameForOpen() 
        {
            return GetFileNameForOpen("\nPlease select file to open: ");
        }
        [Scripting]
        public string GetFileNameForOpen(string message) 
        {
            return Editor.GetFileNameForOpen(message).StringResult;
        }
        [Scripting]
        public string GetFileNameForSave()
        {
            return GetFileNameForOpen("\nPlease input file path: ");
        }
        [Scripting]
        public string GetFileNameForSave(string message)
        {
            return Editor.GetFileNameForSave(message).StringResult;
        }
        [Scripting]
        public int? GetInteger() 
        {
            return GetInteger("\nPlease input number: ");
        }
        [Scripting]
        public int? GetInteger(string message)
        {
            var result = Editor.GetInteger(message);
            return result.Status.Equals(PromptStatus.OK) ? new int?(result.Value) : null;
        }

        //public PromptNestedEntityResult GetNestedEntity(PromptNestedEntityOptions options) { return null; }
        //public PromptNestedEntityResult GetNestedEntity(string message) { return null; }
        [Scripting]
        public Point3d? GetPoint( ) 
        {
            return GetPoint("\nPlease select point: ");
        }
        [Scripting]
        public Point3d? GetPoint(string message) 
        {
            var result = Editor.GetPoint(message);
            return result.Status.Equals(PromptStatus.OK) ? new Point3d?(result.Value) : null;
        }
        [Scripting]
        public string GetString( ) 
        {
            return GetString("\nPlease input string: "); 
        }
        [Scripting]
        public string GetString(string message)
        {
            return Editor.GetString(message).StringResult;
        }

        #endregion

        #region Layer
        [Scripting]
        public LayerTableRecord createlayer(string name)
        {
            var curlayer = Layers[Database.Clayer];
            return createlayer(name, curlayer.Color.ColorIndex , "Contenuous");
        }
        [Scripting]
        public LayerTableRecord createlayer(string name, short colorIndex)
        {
            return createlayer(name, colorIndex, "Contenuous");
        }
        [Scripting]
        public LayerTableRecord createlayer(string name, short colorIndex, string linetype)
        {
            var color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
            return _createLayer(name, color,_getLinetypeId(linetype));
        }

        private LayerTableRecord _createLayer(string name, Color color, ObjectId linetypeId)
        {
            if (!Layers.Has(name))
                using (var transaction = ObjectManager.AddTransaction())
                {
                    var newRecord = new LayerTableRecord()
                    {
                        Name = name,
                        Color = color,
                        LinetypeObjectId = linetypeId
                    };

                    Layers.RecordTable.Add(newRecord);
                    transaction.AddNewlyCreatedDBObject(newRecord, true);
                    transaction.Commit();
                }

            return Layers[name];
        }

        [Scripting]
        public bool LayerExists(string name)
        {
            return Layers.Has(name);
        }

        #endregion

        #endregion

        #region Private Supporting Methods

        private ObjectId _getLinetypeId(string linetypeName)
        {
            if (!Linetypes.Has(linetypeName))
            {
                Linetypes.RecordTable.Dispose();
                Database.LoadLineTypeFile(linetypeName, "acad.lin");
            }
            return Linetypes[linetypeName].ObjectId;
        }

        #endregion

        #region Autocad Document Implementation

        #region Properties

        public object AcadDocument
        {
            get { return BaseDocument.AcadDocument; }
        }

        public string CommandInProgress
        {
            get { return BaseDocument.CommandInProgress; }
        }

        [Scripting]
        public Database Database
        {
            get { return BaseDocument.Database; }
        }

        [Scripting]
        public Editor Editor
        {
            get { return BaseDocument.Editor; }
        }

        [Scripting]
        public Manager GraphicsManager
        {
            get { return BaseDocument.GraphicsManager; }
        }

        public bool IsActive
        {
            get { return BaseDocument.IsActive; }
        }

        [Scripting]
        public bool IsReadOnly
        {
            get { return BaseDocument.IsReadOnly; }
        }

        [Scripting]
        public string Name
        {
            get { return BaseDocument.Name; }
        }

        public StatusBar StatusBar
        {
            get { return BaseDocument.StatusBar; }
        }

        public Hashtable UserData
        {
            get { return BaseDocument.UserData; }
        }

        public Window Window
        {
            get { return BaseDocument.Window; }
        }

        #endregion

        #region Methods

        public System.Drawing.Bitmap CapturePreviewImage(int width, int height)
        {
            return BaseDocument.CapturePreviewImage((uint)width, (uint)height);
        }

        [Scripting(Name = "close")]
        public void CloseAndDiscard()
        {
            BaseDocument.CloseAndDiscard();
        }

        [Scripting(Name = "save")]
        public void CloseAndSave()
        {
            BaseDocument.CloseAndSave(Name);
        }

        public static Document Create(IntPtr unmanagedPointer)
        {
            return Document.Create(unmanagedPointer);
        }

        public void DowngradeDocOpen(bool bPromptForSave)
        {
            BaseDocument.DowngradeDocOpen(bPromptForSave);
        }

        public static Document FromAcadDocument(object acadDocument)
        {
            return Document.FromAcadDocument(acadDocument);
        }

        [Scripting]
        public DocumentLock LockDocument()
        {
            return BaseDocument.LockDocument();
        }

        public DocumentLock LockDocument(DocumentLockMode lockMode, string globalCommandName, string localCommandName,
            bool promptIfFails)
        {
            return BaseDocument.LockDocument(lockMode, globalCommandName, localCommandName, promptIfFails);
        }

        public DocumentLockMode LockMode()
        {
            return BaseDocument.LockMode();
        }

        public DocumentLockMode LockMode(bool bIncludeMyLocks)
        {
            return BaseDocument.LockMode(bIncludeMyLocks);
        }

        public void PopDbmod()
        {
            BaseDocument.PopDbmod();
        }

        public void PushDbmod()
        {
            BaseDocument.PushDbmod();
        }

        public void SendStringToExecute(string command, bool activate, bool wrapUpInactiveDoc, bool echoCommand)
        {
            BaseDocument.SendStringToExecute(command, activate, wrapUpInactiveDoc, echoCommand);
        }

        public Database TryGetDatabase()
        {
            return BaseDocument.TryGetDatabase();
        }

        public void UpgradeDocOpen()
        {
            BaseDocument.UpgradeDocOpen();
        }

        #endregion

        #region Events

        public event DisposingEventHandler BeginDocumentDispose
        {
            add { _beginDocumentDispose += value; }
            remove { _beginDocumentDispose -= value; }
        }
        private event DisposingEventHandler _beginDocumentDispose;

        public event DocumentBeginCloseEventHandler BeginDocumentClose
        {
            add { BaseDocument.BeginDocumentClose += value; }
            remove { BaseDocument.BeginDocumentClose -= value; }
        }

        public event DrawingOpenEventHandler BeginDwgOpen
        {
            add { BaseDocument.BeginDwgOpen += value; }
            remove { BaseDocument.BeginDwgOpen -= value; }
        }

        public event EventHandler CloseAborted
        {
            add { BaseDocument.CloseAborted += value; }
            remove { BaseDocument.CloseAborted -= value; }
        }

        public event EventHandler CloseWillStart
        {
            add { BaseDocument.CloseWillStart += value; }
            remove { BaseDocument.CloseWillStart -= value; }
        }

        public event CommandEventHandler CommandCancelled
        {
            add { BaseDocument.CommandCancelled += value; }
            remove { BaseDocument.CommandCancelled -= value; }
        }

        public event CommandEventHandler CommandEnded
        {
            add { BaseDocument.CommandEnded += value; }
            remove { BaseDocument.CommandEnded -= value; }
        }

        public event CommandEventHandler CommandFailed
        {
            add { BaseDocument.CommandFailed += value; }
            remove { BaseDocument.CommandFailed -= value; }
        }

        public event CommandEventHandler CommandWillStart
        {
            add { BaseDocument.CommandWillStart += value; }
            remove { BaseDocument.CommandWillStart -= value; }
        }

        public event DrawingOpenEventHandler EndDwgOpen
        {
            add { BaseDocument.EndDwgOpen += value; }
            remove { BaseDocument.EndDwgOpen -= value; }
        }

        public event EventHandler ImpliedSelectionChanged
        {
            add { BaseDocument.ImpliedSelectionChanged += value; }
            remove { BaseDocument.ImpliedSelectionChanged -= value; }
        }

        public event EventHandler LispCancelled
        {
            add { BaseDocument.LispCancelled += value; }
            remove { BaseDocument.LispCancelled -= value; }
        }

        public event EventHandler LispEnded
        {
            add { BaseDocument.LispEnded += value; }
            remove { BaseDocument.LispEnded -= value; }
        }

        public event LispWillStartEventHandler LispWillStart
        {
            add { BaseDocument.LispWillStart += value; }
            remove { BaseDocument.LispWillStart -= value; }
        }

        public event UnknownCommandEventHandler UnknownCommand
        {
            add { BaseDocument.UnknownCommand += value; }
            remove { BaseDocument.UnknownCommand -= value; }
        }

        public event EventHandler ViewChanged
        {
            add { BaseDocument.ViewChanged += value; }
            remove { BaseDocument.ViewChanged -= value; }
        }

        #endregion

        #region DisposableWrapper implementation

        public void Dispose()
        {
            if (_beginDocumentDispose != null)
                _beginDocumentDispose(this, new EventArgs());

            ObjectManager.Dispose();
        }

        #endregion

        #region IEqualityComparer implementation

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((PyrrhaDocument)obj);
        }

        internal bool Equals(PyrrhaDocument other)
        {
            return Equals(BaseDocument, other.BaseDocument)
                && Equals(_objectManager, other._objectManager);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((BaseDocument != null ? BaseDocument.GetHashCode() : 0) * 397);
            }
        }

        #endregion

        #endregion
    }
}


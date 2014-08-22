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
using System.Drawing;
using Autodesk.AutoCAD.Colors;
using System.Linq;

#endregion

//#pragma warning disable 612,618

namespace Pyrrha
{
    public sealed class PyrrhaDocument
        : MarshalByRefObject,
          IDisposable
    {
        #region Properties

        internal readonly Document BaseDocument;

        private DocumentManager _documentManager;

        private LayerCollection _layers;

        private LinetypeCollection _linetypes;

        private OpenObjectManager _objectManager;

        private TextStyleCollection _textstyles;

        public DocumentManager DocumentManager
        {
            get { return _documentManager ?? (_documentManager = new DocumentManager()); }
        }

        [ScriptingProperty]
        public OpenObjectManager ObjectManager
        {
            get
            {
                return _objectManager ??
                       (_objectManager = new OpenObjectManager(this));
            }
        }

        public BlockTableRecord ModelSpace
        {
            get
            {
                return (BlockTableRecord)ObjectManager.GetObject(
                SymbolUtilityServices.GetBlockModelSpaceId(Database));
            }
        }

        public BlockTableRecord PaperSpace
        {
            get
            {
                return (BlockTableRecord)ObjectManager.GetObject(
                SymbolUtilityServices.GetBlockPaperSpaceId(Database));
            }
        }

        [ScriptingProperty]
        public LayerCollection Layers
        {
            get { return _layers ?? (_layers = new LayerCollection(this, OpenMode.ForWrite)); }
            set { _layers = value; }
        }

        [ScriptingProperty]
        public TextStyleCollection TextStyles
        {
            get { return _textstyles ?? (_textstyles = new TextStyleCollection(this, OpenMode.ForWrite)); }
            set { _textstyles = value; }
        }

        [ScriptingProperty]
        public LinetypeCollection Linetypes
        {
            get { return _linetypes ?? (_linetypes = new LinetypeCollection(this, OpenMode.ForWrite)); }
            set { _linetypes = value; }
        }

        #endregion

        #region Constructors

        public PyrrhaDocument()
            : this(AcApp.DocumentManager.MdiActiveDocument)
        {
        }

        public PyrrhaDocument(string path)
            : this(FindDocument(path))
        {
        }

        public PyrrhaDocument(Func<Document> func)
            : this(func())
        {
        }

        private PyrrhaDocument(Document doc)
        {
            if (doc == null)
                throw new NullReferenceException("Document is null.");

            BaseDocument = doc;
            DocumentManager.AddDocument(this);
        }

        #endregion

        #region Scripting Methods

        #region Document

        [ScriptingMethod]
        public static Document FindDocument(string path)
        {
            var openDocument = AcApp.DocumentManager.Cast<Document>()
                                    .FirstOrDefault(doc => doc.Name.IndexOf(path, StringComparison.CurrentCultureIgnoreCase) > -1);
            return openDocument ?? AcApp.DocumentManager.Open(path, false);
        }

        [ScriptingMethod]
        public void SendCommandAsync(string command)
        {
            var acadDoc = AcadDocument;
            acadDoc.GetType().InvokeMember(
                "SendCommand",
                System.Reflection.BindingFlags.InvokeMethod,
                null,
                acadDoc,
                new[] { command + "\n" });
        }

        [ScriptingMethod]
        public void ConfirmAllChanges()
        {
            ObjectManager.CommitAll();
        }

        [ScriptingMethod]
        public DBObject Import(string path)
        {
            return Import(path, ModelSpace);
        }

        [ScriptingMethod]
        public DBObject Import(string path, BlockTableRecord selectedSpace)
        {
            var blockName = Path.GetFileNameWithoutExtension(path);
            Database tmpDb = new Database(false, true);
            tmpDb.ReadDwgFile(path, FileShare.Read, true, null);
            return ObjectManager.GetObject(Database.Insert(blockName, tmpDb, true), OpenMode.ForWrite);
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

        #region Editor

        [ScriptingMethod]
        public void Write(object message)
        {
            Editor.WriteMessage(string.Format("\n{0}\n", message));
        }

        [ScriptingMethod]
        public double? GetDistance(string message)
        {
            var result = Editor.GetDistance(message);
            return result.Status.Equals(PromptStatus.OK)
                ? new double?(result.Value)
                : null;
        }

        [ScriptingMethod]
        public double? GetDouble(string message)
        {
            var result = Editor.GetDouble(message);
            return result.Status.Equals(PromptStatus.OK)
                ? new double?(result.Value)
                : null;
        }

        [ScriptingMethod]
        public Entity GetEntity(string message)
        {
            throw new NotImplementedException();
        }

        [ScriptingMethod]
        public string GetFileNameForOpen(string message)
        {
            return Editor.GetFileNameForOpen(message)
                         .StringResult;
        }

        [ScriptingMethod]
        public string GetFileNameForSave(string message)
        {
            return Editor.GetFileNameForSave(message)
                         .StringResult;
        }

        [ScriptingMethod]
        public int? GetInteger(string message)
        {
            var result = Editor.GetInteger(message);
            return result.Status.Equals(PromptStatus.OK)
                ? new int?(result.Value)
                : null;
        }

        //public PromptNestedEntityResult GetNestedEntity(PromptNestedEntityOptions options) { return null; }
        //public PromptNestedEntityResult GetNestedEntity(string message) { return null; }

        [ScriptingMethod]
        public Point3d? GetPoint(string message)
        {
            var result = Editor.GetPoint(message);
            return result.Status.Equals(PromptStatus.OK)
                ? new Point3d?(result.Value)
                : null;
        }

        [ScriptingMethod]
        public string GetString(string message, bool spacesAllowed)
        {
            return Editor.GetString(
                new PromptStringOptions(message)
                {
                    AllowSpaces = spacesAllowed
                })
                         .StringResult;
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
            Application.SetSystemVariable(varName, value);
        }

        [ScriptingMethod]
        public object GetVar(string varName)
        {
            return Application.GetSystemVariable(varName);
        }

        [ScriptingMethod]
        public void Alert(string message)
        {
            Application.ShowAlertDialog(message);
        }

        [ScriptingMethod]
        public void Update()
        {
            Application.UpdateScreen();
        }

        #endregion

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

        [ScriptingProperty]
        public Database Database
        {
            get { return BaseDocument.Database; }
        }

        [ScriptingProperty]
        public Editor Editor
        {
            get { return BaseDocument.Editor; }
        }

        [ScriptingProperty]
        public Manager GraphicsManager
        {
            get { return BaseDocument.GraphicsManager; }
        }

        public bool IsActive
        {
            get { return BaseDocument.IsActive; }
        }

        [ScriptingProperty]
        public bool IsReadOnly
        {
            get { return BaseDocument.IsReadOnly; }
        }

        [ScriptingProperty]
        public string Name
        {
            get { return BaseDocument.Name; }
        }

        [ScriptingProperty]
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

        public Bitmap CapturePreviewImage(int width, int height)
        {
            return BaseDocument.CapturePreviewImage((uint)width, (uint)height);
        }

        [ScriptingMethod]
        public void CloseAndDiscard()
        {
            BaseDocument.CloseAndDiscard();
        }

        [ScriptingMethod]
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

        [ScriptingMethod]
        public DocumentLock LockDocument()
        {
            return BaseDocument.LockDocument();
        }

        public DocumentLock LockDocument(
            DocumentLockMode lockMode,
            string globalCommandName,
            string localCommandName,
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
                return ((BaseDocument != null
                    ? BaseDocument.GetHashCode()
                    : 0) * 397);
            }
        }

        #endregion

        #endregion
    }
}

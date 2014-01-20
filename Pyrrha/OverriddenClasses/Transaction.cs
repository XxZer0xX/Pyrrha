//#region Referenceing

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel.Design;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Runtime;
//using Exception = System.Exception;

//#endregion

//namespace Pyrrha.OverriddenClasses
//{
//    internal sealed class Transaction : OpenCloseTransaction , IEnumerable<DBObject>
//    {
//        public TransId TransactionId;

//        public new TransactionManager TransactionManager { get; set; }

//        public Transaction(TransactionManager manager)
//        {
//            TransactionId = new TransId(UnmanagedObject);
//            TransactionManager = manager;
//            TransactionManager.AddToTransactionList(TransactionId , this);
//        }

//        public void AddNewlyCreatedDBObject(DBObject obj)
//        {
//            AddNewlyCreatedDBObject(obj , true);
//        }

//        public override void Commit()
//        {
//            base.Commit();
//            this.Dispose();
//        }

//        public override void AddNewlyCreatedDBObject(DBObject obj , bool add)
//        {
//            TransactionManager.AddNewlyCreatedDBObject(obj);
//        }

//        public DBObject GetObject(ObjectId id)
//        {
//            return GetObject(id , OpenMode.ForWrite , false , true);
//        }

//        public override DBObject GetObject(ObjectId id , OpenMode mode)
//        {
//            return GetObject(id , mode , false , true);
//        }

//        public override DBObject GetObject(ObjectId id , OpenMode mode , bool openErased)
//        {
//            return GetObject(id , mode , openErased , true);
//        }

//        public override DBObject GetObject(ObjectId id , OpenMode mode = OpenMode.ForWrite , bool openErased = false ,
//            bool forceOpenOnLockedLayer = true)
//        {
//            var dbo = base.GetObject(id , mode , openErased , forceOpenOnLockedLayer);
//            appendToCollections(id , dbo);
//            return dbo;
//        }

//        new public IList<DBObject> GetAllObjects()
//        {
//            return OpenObjects;
//        }

//        public T GetOpenObject<T>( ObjectId id ) where T : DBObject
//        {
//            return !TransactionManager.HasOpenObject( id )
//                ? (T) GetObject( id )
//                : (T) TransactionManager.GetOpenObject(id);
//        }

//        #region IEnumerable Implementation

//        private IList<ObjectId> _openObjectsIds;
//        private IList<DBObject> _openObjects;

//        internal IList<ObjectId> OpenObjectsIds
//        {
//            get { return _openObjectsIds ?? (_openObjectsIds = new List<ObjectId>()); }
//            set { _openObjectsIds = value; }
//        }

//        internal IList<DBObject> OpenObjects
//        {
//            get { return _openObjects ?? (_openObjects = new List<DBObject>()); }
//            set { _openObjects = value; }
//        }

//        public IEnumerator<DBObject> GetEnumerator()
//        {
//            return OpenObjects.GetEnumerator();
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        private void appendToCollections(ObjectId id , DBObject obj)
//        {
//            OpenObjectsIds.Add(id);
//            OpenObjects.Add(obj);
//        }

//        #endregion

//        #region IDisposable override Implementation

//        new internal void Dispose()
//        {
//            GC.SuppressFinalize(this);
//            Dispose(true);
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && !IsDisposed)
//            {
//                foreach (var openObject in OpenObjects)
//                    openObject.Dispose();
//            }

//            base.Dispose(disposing);
//        }

//        #endregion
//    }
//}
#region Referencing

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

#endregion

namespace Pyrrha
{
    public class OpenObjectManager : IDisposable, IEqualityComparer<DBObject>, IEnumerable<DBObject>
    {
        #region Properties

        public int Count
        {
            get { return this.OpenObjects.Count; }
        }

        public Database Database {get; private set;}

        public IDictionary<ObjectId, DBObject> OpenObjects
        {
            get { return this._openObjects ?? (this._openObjects = new Dictionary<ObjectId, DBObject>()); }
            private set { this._openObjects = value;}
        }
        private IDictionary<ObjectId, DBObject> _openObjects;

        public IList<Transaction> Transactions { get; private set; }

        #endregion

        #region Constructor

        public OpenObjectManager(Database database)
        {
            this.Database = database;
            this.Transactions = new List<Transaction>();
        }

        #endregion

        #region Methods

        private DBObject AddObject(ObjectId id, Transaction trans, OpenMode mode)
        {
            DBObject returnObj = null;

            try
            {
                returnObj = trans.GetObject(id, mode);
            }
            catch (Exception ex)
            {
                if (ex.ErrorStatus == ErrorStatus.NotOpenForWrite)
                    this.AddObject(id, trans, OpenMode.ForRead).Close();
                
                id.GetObject(OpenMode.ForRead).Close();
                this.AddObject(id, trans, mode);

                throw;
            }

            if (this.OpenObjects.ContainsKey(id))
                this.OpenObjects[id] = returnObj;
            else
                this.OpenObjects.Add(id, returnObj);
            return returnObj;
        }

        public void AbortAll()
        {
            foreach (var trans in this.Transactions)
                trans.Abort();
            this.OpenObjects = null;
        }

        public Transaction AddTransaction()
        {
            var newTrans = this.Database.TransactionManager.StartOpenCloseTransaction();
            this.Transactions.Add(newTrans);

            return newTrans;
        }

        public void CommitAll(bool clearobjects = true)
        {
            foreach (var trans in this.Transactions)
            {
                trans.Commit();
                trans.Dispose();
            }
            this.Transactions.Clear();
        }

        public DBObject GetRecord(ObjectId id, Transaction trans, OpenMode mode)
        {
            //bool inCollection = collection.Contains(id);
            bool inManager = this.OpenObjects.ContainsKey(id);

            // Check the object for a value;
            var returnObj = inManager ? this.OpenObjects[id] : null;

            bool isNull = returnObj == null;
            bool isOpen = !isNull && (this.OpenObjects[id].IsReadEnabled || this.OpenObjects[id].IsWriteEnabled);

            // The DBObject is managed and already open
            if (inManager && isOpen)
            {
                if (!returnObj.IsReadEnabled && mode != OpenMode.ForRead)
                    returnObj.DowngradeOpen();
                else if (returnObj.IsWriteEnabled && mode != OpenMode.ForWrite)
                    returnObj.UpgradeOpen();

                return this.OpenObjects[id];
            }

            // Add the object to the transaction owned by the collection
            return this.AddObject(id, trans, mode);
        }

        internal void RemoveObject(ObjectId id, bool erase = false)
        {
            if (!this.OpenObjects.ContainsKey(id))
                return;

            var obj = this.OpenObjects[id];
            if (obj != null)
            {
                if (erase)
                    obj.Erase();
                obj.Close();                  
            }
                
            this.OpenObjects.Remove(id);
        }

        public void RemoveTransaction(Transaction trans, bool commit = false)
        {
            if (commit)
                trans.Commit();

            this.Transactions.Remove(trans);
            trans.Dispose();
        }

        #endregion

        #region IEnumerable Implementation

        public IEnumerator<DBObject> GetEnumerator()
        {
            return this.OpenObjects.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IEqualityComparer Implementation

        public bool Equals(DBObject x, DBObject y)
        {
            return x.ObjectId.Equals(y.ObjectId);
        }

        public int GetHashCode(DBObject obj)
        {
            // Hashing off the (Long) handle might be better
            return obj.Handle.GetHashCode();
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed;

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected void Dispose(bool isDisposing)
        {
            if (!isDisposing || this._disposed)
                return;

            foreach (var trans in this.Transactions)
                trans.Dispose();

            foreach (var obj in this.OpenObjects.Values.Where(obj => obj != null))
                obj.Close();
                
            this._disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
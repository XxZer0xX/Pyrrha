#region Referencing

using System;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Pyrrha
{
    public delegate void FetchingEvent( object sender , EventArgs args );

    public class OpenObjectManager : IDisposable, IEqualityComparer<DBObject>, IEnumerable<DBObject>
    {

        public Database Database {get; private set;}

        public IDictionary<ObjectId, DBObject> OpenObjects
        {
            get { return _openObjects ?? (_openObjects = GetOpenObjects()); }
            private set { _openObjects = value;}
        }
        private IDictionary<ObjectId, DBObject> _openObjects;

        //private readonly LongTransaction _transaction;

        public IList<Transaction> Transactions { get; private set; }
        public ICollection<Transaction> Tests { get; internal set; }

        #region Properties

        public int Count
        {
            get { return this.OpenObjects.Count; }
        }

        #endregion

        #region Constructor

        // Transaction Collection Members
        internal Transaction AddTransaction()
        {
            var newTrans = Database.TransactionManager.StartTransaction();
            Transactions.Add(newTrans);

            return newTrans;
        }
        internal void RemoveTransaction(Transaction transaction)
        {
            Transactions.Remove(transaction);
        }
        internal void CommitTransaction(Transaction transaction)
        {
            // Check the other transactions for ownership...
            transaction.Commit();
            transaction.Dispose();
        }


        public OpenObjectManager(Database database)
        {
            Database = database;
            Transactions = new List<Transaction>();
            OpenObjects = new Dictionary<ObjectId, DBObject>();

            Database.ObjectOpenedForModify += Database_ObjectOpenedForModify;
        }

        private void Database_ObjectOpenedForModify(object sender, ObjectEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Events

        //public event FetchingEvent FetchingFromObjectId;

        #endregion

        #region Methods

        public IDictionary<ObjectId, DBObject> GetOpenObjects()
        {
            var returnObjects = new Dictionary<ObjectId, DBObject>();
            return returnObjects;
        }

        //public IEnumerable<DBObject> GetObjects()
        //{
            
        //}


        //public DBObject GetObject(ObjectId id)
        //{
        //    return OpenObjects.ContainsKey(id)
        //        ? OpenObjects[id]
        //        : null;
        //        //: _getAddObject(id);
        //}

        public void ConfirmAllChanges()
        {
            //_transaction.
            Dispose();
        }

        internal void Remove(ObjectId id)
        {
            var obj = this.OpenObjects[id];
            if (obj == null) return;
            obj.Dispose();
            this.OpenObjects.Remove(id);
        }

        //private DBObject _getAddObject(ObjectId id)
        //{
        //    var obj = this._transaction.GetObject(id, OpenMode.ForWrite);
        //    this.OpenObjects.Add(id, obj);
        //    return obj;
        //}

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
            return obj.GetHashCode();
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed;

        public void Dispose()
        {
            //this._transaction.Commit();
            //this._transaction.Dispose();
            OpenObjects.Clear();
            Dispose( true );
        }

        public void Dispose(bool isDisposing)
        {
            if (!isDisposing || _disposed)
                return;
            _disposed = true;
            GC.SuppressFinalize( this );
        }

        #endregion
    }
}
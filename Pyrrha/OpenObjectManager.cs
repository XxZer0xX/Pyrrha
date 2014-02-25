#region Referencing

using System;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pyrrha.Collections;
using Pyrrha.Runtime;

#endregion

namespace Pyrrha
{
    public delegate void FetchingEvent( object sender , EventArgs args );

    public class OpenObjectManager : IDisposable, IEqualityComparer<DBObject>, IEnumerable<DBObject>
    {

        public Database Database {get; private set;}

        public IDictionary<ObjectId, DBObject> OpenObjects
        {
            get { return _openObjects ?? (_openObjects = CreateOpenObjects()); }
            private set { _openObjects = value;}
        }
        private IDictionary<ObjectId, DBObject> _openObjects;

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

        private IDictionary<ObjectId, DBObject> CreateOpenObjects()
        {
            var returnObjects = new Dictionary<ObjectId, DBObject>();
            return returnObjects;
        }

        public T GetObject<T>(ObjectId id, RecordCollection<T> collection) where T : SymbolTableRecord
        {
            bool inCollection;
            bool inManager;
            bool isNull;
            bool isOpen;
            DBObject returnObj;

            inCollection = collection.Contains(id);
            inManager = OpenObjects.ContainsKey(id);

            // Check the object for a value;
            returnObj = inManager ? OpenObjects[id] : null;

            isNull = returnObj == null;
            isOpen = !isNull ? OpenObjects[id].IsReadEnabled : false;

            // The DBObject is NOT managed or owned
            if (!inCollection && !inManager)
                return (T)AddObject(id, collection.Transaction, collection.OpenMode);

            // The DBObject is managed, owned and already open
            if (inManager && inCollection && isOpen)
                return (T)OpenObjects[id];

            // The DBObject is owned and NOT managed
            if (!inManager && inCollection)
                return (T)AddObject(id, collection.Transaction, collection.OpenMode);

            // The DbObject is managed, owned but Null
            if(inManager && inCollection && isNull)
                return (T)AddObject(id, collection.Transaction, collection.OpenMode);

            // The DBObject is managed and NOT owned
            if (inManager && !inCollection)
            {
                if (isOpen)
                    returnObj.Close();

                return (T)AddObject(id, collection.Transaction, collection.OpenMode);
            }

            throw new PyrrhaException("How the hell did you achieve this?");
        }

        private DBObject AddObject(ObjectId id, Transaction trans, OpenMode mode)
        {
            try 
	        {
                var obj = trans.GetObject(id, mode);
                if (OpenObjects.ContainsKey(id))
                    OpenObjects[id] = obj;
                else
                    OpenObjects.Add(id, obj);
                return obj;
	        }
	        catch (Exception ex)
	        {
		        
		        throw;
	        }
            
        }


        public bool UpgradeOpen(ObjectId id)
        {
            return false;
        }
        public bool DowngradeClose(ObjectId id)
        {
            return false;
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
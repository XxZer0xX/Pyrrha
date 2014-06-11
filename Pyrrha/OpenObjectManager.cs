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
    public class OpenObjectManager
        : IDisposable,
          IEqualityComparer<DBObject>,
          IEnumerable<DBObject>
    {
        #region Properties

        private IDictionary<ObjectId, DBObject> _openObjects;

        public int Count
        {
            get { return OpenObjects.Count; }
        }

        public Database Database { get; private set; }

        public PyrrhaDocument Document { get; private set; }

        public IDictionary<ObjectId, DBObject> OpenObjects
        {
            get { return _openObjects ?? ( _openObjects = new Dictionary<ObjectId, DBObject>() ); }
            private set { _openObjects = value; }
        }

        public IList<Transaction> Transactions { get; private set; }

        #endregion

        #region Constructor

        public OpenObjectManager( PyrrhaDocument document )
        {
            Document = document;
            Database = document.Database;
            Transactions = new List<Transaction>();
        }

        #endregion

        #region Methods

        private DBObject AddObject( ObjectId id, Transaction trans, OpenMode mode )
        {
            DBObject returnObj = null;

            try
            {
                returnObj = trans.GetObject( id, mode );
            } catch ( Exception ex )
            {
                if (ex.ErrorStatus == ErrorStatus.NotOpenForWrite)
                    AddObject( id, trans, OpenMode.ForRead )
                        .Close();

                id.GetObject( OpenMode.ForRead )
                  .Close();
                AddObject( id, trans, mode );

                throw;
            }

            if (OpenObjects.ContainsKey( id ))
                OpenObjects[id] = returnObj;
            else
                OpenObjects.Add( id, returnObj );
            return returnObj;
        }

        public void AbortAll()
        {
            foreach ( var trans in Transactions )
                trans.Abort();
            OpenObjects = null;
        }

        public Transaction AddTransaction()
        {
            var newTrans = Database.TransactionManager.StartOpenCloseTransaction();
            Transactions.Add( newTrans );

            return newTrans;
        }

        public void CommitAll( bool clearobjects = true )
        {
            foreach ( var trans in Transactions )
            {
                trans.Commit();
                trans.Dispose();
            }
            Transactions.Clear();
        }

        public DBObject GetObject( ObjectId id )
        {
            return GetObject( id, new OpenCloseTransaction(), OpenMode.ForWrite );
        }

        public DBObject GetObject( ObjectId id, OpenMode mode )
        {
            return GetObject( id, new OpenCloseTransaction(), mode );
        }

        public DBObject GetObject( ObjectId id, Transaction trans )
        {
            return GetObject( id, trans, OpenMode.ForWrite );
        }

        public DBObject GetObject( ObjectId id, Transaction trans, OpenMode mode )
        {
            var inManager = OpenObjects.ContainsKey( id );

            var returnObj = inManager
                ? OpenObjects[id]
                : null;

            var isNull = returnObj == null;
            var isOpen = !isNull && ( OpenObjects[id].IsReadEnabled || OpenObjects[id].IsWriteEnabled );

            // The DBObject is managed and already open
            if (inManager && isOpen)
            {
                if (!returnObj.IsReadEnabled &&
                    mode != OpenMode.ForRead)
                    returnObj.DowngradeOpen();
                else if (returnObj.IsWriteEnabled &&
                         mode != OpenMode.ForWrite)
                    returnObj.UpgradeOpen();

                return OpenObjects[id];
            }

            return AddObject( id, trans, mode );
        }

        internal void RemoveObject( ObjectId id, bool erase = false )
        {
            if (!OpenObjects.ContainsKey( id ))
                return;

            var obj = OpenObjects[id];
            if (obj != null)
            {
                if (erase)
                    obj.Erase();
                obj.Close();
            }

            OpenObjects.Remove( id );
        }

        public void RemoveTransaction( Transaction trans, bool commit = false )
        {
            if (commit)
                trans.Commit();

            Transactions.Remove( trans );
            trans.Dispose();
        }

        #endregion

        #region IEnumerable Implementation

        public IEnumerator<DBObject> GetEnumerator()
        {
            return OpenObjects.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEqualityComparer Implementation

        public bool Equals( DBObject x, DBObject y )
        {
            return x.ObjectId.Equals( y.ObjectId );
        }

        public int GetHashCode( DBObject obj )
        {
            return obj.Handle.GetHashCode();
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed;

        public void Dispose()
        {
            Dispose( true );
        }

        protected void Dispose( bool isDisposing )
        {
            if (!isDisposing || _disposed)
                return;

            foreach ( var trans in Transactions )
                trans.Dispose();

            foreach ( var obj in OpenObjects.Values.Where( obj => obj != null ) )
                obj.Close();

            _disposed = true;
            GC.SuppressFinalize( this );
        }

        #endregion
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;
using System;

namespace Pyrrha.Collections
{
    public abstract class RecordCollection<T,R> : ICollection<R>, IDisposable
        where T : SymbolTable
        where R : SymbolTableRecord
    {
        #region Properties
        
        private bool _disposed;
        //private readonly IList<ObjectId> _idList;

        internal Transaction Transaction
        {
            get { return _transaction ?? (_transaction = Manager.AddTransaction()); }
            set { _transaction = value; }
        }
        private Transaction _transaction;

        protected OpenObjectManager Manager { get; private set; }
        protected T RecordTable { get; set; }

        public int Count
        {
            get { return GetCount(); }
        }

        public OpenMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                foreach (var id in RecordTable)
                    GetRecord(id);
            }
        }
        private OpenMode _mode;

        public R this[int index]
        {
            get { return Indexer(index); }
            set { this[index] = value; }
        }

        #endregion

        #region Constructor

        protected RecordCollection(PyrrhaDocument document, ObjectId tableid, OpenMode openMode = OpenMode.ForRead)
        {
            Manager = document.ObjectManager;
            _mode = openMode;

            RecordTable = (T)Transaction.GetObject(tableid, Mode);
            Refresh();
        }

        #endregion

        #region Methods

        private R Indexer(int index)
        {
            using (var iter = RecordTable.GetEnumerator())
            {
                int i = 0;
                while (i++ != index)
                    iter.MoveNext();

                return GetRecord(iter.Current);
            }
        }
        private int GetCount()
        {
            int result = 0;
            using (var iter = RecordTable.GetEnumerator())
            {
                while (iter.MoveNext())
                    result++;
            }

            return result;
        }
        protected R GetRecord(ObjectId id)
        {
            if (id == null)
                throw new PyrrhaException("ObjectId cannot be null");

            if (Manager == null)
                throw new PyrrhaException("ObjectManager is null");

            return (R)Manager.GetRecord(id, Transaction, Mode);
        }

        public virtual void Add(R item)
        {
            if (Contains(item))
                throw new PyrrhaException("This {0} already exists in the collection", item.GetType().Name);

            if (item == null)
                throw new PyrrhaException("Item cannot be null.");

            RecordTable.Add(item); // Add to the Record Table
            Transaction.AddNewlyCreatedDBObject(item, true);
            GetRecord(item.Id); // Make sure its managed
        }

        public virtual void Clear()
        {
            foreach (var obj in this)
                Remove(obj);
        }

        public void Commit()
        {
            Transaction.Commit();
            Transaction.Dispose();
        }

        public bool Contains(R item)
        {
            return RecordTable.Has(item.Id);
        }

        public bool Contains(ObjectId id)
        {
            return RecordTable.Has(id);
        }

        public void CopyTo(R[] array, int arrayIndex)
        {
            var currentObjects = this.ToArray();
            currentObjects.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Refresh()
        {
            foreach (var id in RecordTable)
                GetRecord(id);
        }

        public virtual bool Remove(R item)
        {
            if (!Contains(item))
                throw new PyrrhaException("The {0} does not exist in the collection", item.GetType().Name);

            if (RecordTable.Has(item.Id))
                item.Erase();

            return true;
        }

        #region IEnumerable Implementation

        public IEnumerator<R> GetEnumerator()
        {
            if (Manager == null)
                throw new PyrrhaException("ObjectManager is null");

            using (var iter = RecordTable.GetEnumerator())
            {
                while (iter.MoveNext())
                    yield return GetRecord(iter.Current);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDisposable Implementation

        ~RecordCollection()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                Transaction.Dispose();

            _disposed = true;
        }

        #endregion

        #endregion
    }
}

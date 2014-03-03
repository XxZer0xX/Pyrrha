using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;

namespace Pyrrha.Collections
{
    public abstract class RecordCollection<TTable,TRecord> : ICollection<TRecord>, IDisposable
        where TTable : SymbolTable
        where TRecord : SymbolTableRecord
    {
        #region Properties
        
        private bool _disposed;
        private readonly ObjectId _tableId;

        protected TTable RecordTable
        {
            get { return _recordTable ?? (_recordTable = GetRecordTable()); }
            set { _recordTable = value; }
        }
        private TTable _recordTable;

        protected Transaction Transaction
        {
            get { return _transaction ?? (_transaction = ObjectManager.AddTransaction()); }
        }
        private Transaction _transaction;

        public int Count
        {
            get { return GetCount(); }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }
        private readonly bool _isReadOnly;

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

        public OpenObjectManager ObjectManager { get; private set; }

        public TRecord this[int index]
        {
            get { return GetIndex(index); }
            set { SetIndex(value); }
        }

        public TRecord this[string name]
        {
            get { return GetIndex(name); }
            set { SetIndex(value); }
        }

        #endregion

        #region Constructor

        protected RecordCollection(PyrrhaDocument document, ObjectId tableid, OpenMode openMode = OpenMode.ForRead)
        {
            ObjectManager = document.ObjectManager;
            _mode = openMode;
            _isReadOnly = false;
            _tableId = tableid;
            _recordTable = null;

            Refresh();
        }

        #endregion

        #region Methods

        private TRecord GetIndex(int index)
        {
            using (var iter = RecordTable.GetEnumerator())
            {
                int i = -1;
                while (i++ != index)
                    iter.MoveNext();

                return GetRecord(iter.Current);
            }
        }
        private TRecord GetIndex(string recordName)
        {
            var id = RecordTable[recordName];
            return GetRecord(id);
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
        private TTable GetRecordTable()
        {
            return (TTable)Transaction.GetObject(_tableId, Mode);
        }
        private void SetIndex(TRecord value)
        {
            if (ObjectManager.OpenObjects.ContainsKey(value.Id))
                ObjectManager.OpenObjects[value.Id] = value;
        }

        protected TRecord GetRecord(ObjectId id)
        {
            if (id == null)
                throw new PyrrhaException("ObjectId cannot be null");

            if (ObjectManager == null)
                throw new PyrrhaException("ObjectManager is null");

            return (TRecord)ObjectManager.GetRecord(id, Transaction, Mode);
        }

        public void Commit()
        {
            Transaction.Commit();
            Transaction.Dispose();
        }
        public bool Contains(ObjectId id)
        {
            return RecordTable.Has(id);
        }
        public void Refresh()
        {
            foreach (var id in RecordTable)
                GetRecord(id);
        }

        #region ICollection

        public virtual void Add(TRecord item)
        {
            if (Contains(item))
                throw new PyrrhaException("This {0} already exists in the collection", item.GetType().Name);

            if (item == null)
                throw new PyrrhaException("Item cannot be null.");

            RecordTable.Add(item); // Add to the Record Table
            Transaction.AddNewlyCreatedDBObject(item, true);
            GetRecord(item.Id); // Make sure its managed
        }
        public void Clear()
        {
            foreach (var obj in this)
                Remove(obj);
        }
        public bool Contains(TRecord item)
        {
            return RecordTable.Has(item.Id);
        }
        public void CopyTo(TRecord[] array, int arrayIndex)
        {
            var currentObjects = this.ToArray();
            currentObjects.CopyTo(array, arrayIndex);
        }
        public virtual bool Remove(TRecord item)
        {
            if (!Contains(item))
                throw new PyrrhaException("The {0} does not exist in the collection", item.GetType().Name);

            if (RecordTable.Has(item.Id))
                item.Erase();

            return true;
        }

        #endregion

        #region IEnumerable

        public IEnumerator<TRecord> GetEnumerator()
        {
            if (ObjectManager == null)
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

        #region IDisposable

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

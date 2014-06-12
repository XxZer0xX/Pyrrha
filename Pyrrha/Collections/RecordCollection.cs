#region Referencing

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;

#endregion

namespace Pyrrha.Collections
{
    public abstract class RecordCollection<TTable,TRecord> : ICollection<TRecord>, IDisposable
        where TTable : SymbolTable
        where TRecord : SymbolTableRecord
    {
        #region Properties
        
        private bool _disposed;
        private readonly ObjectId _tableId;

        internal TTable RecordTable
        {
            get { return this._recordTable ?? (this._recordTable = this.GetRecordTable()); }
            set { this._recordTable = value; }
        }
        private TTable _recordTable;

        protected Transaction Transaction
        {
            get { return this._transaction ?? (this._transaction = this.ObjectManager.AddTransaction()); }
        }
        private Transaction _transaction;

        public int Count
        {
            get { return this.GetCount(); }
        }

        public bool Has(string record)
        {
            return this.RecordTable.Has(record);
        }

        public bool IsReadOnly
        {
            get { return this._isReadOnly; }
        }
        private readonly bool _isReadOnly;

        public OpenMode Mode
        {
            get { return this._mode; }
            set
            {
                this._mode = value;
                foreach (var id in this.RecordTable)
                    this.GetRecord(id);
            }
        }
        private OpenMode _mode;

        public OpenObjectManager ObjectManager { get; private set; }

        public TRecord this[int index]
        {
            get { return GetIndex(index); }
            set { this.SetIndex(value); }
        }

        public TRecord this[string name]
        {
            get { return GetIndex(name); }
            set { this.SetIndex(value); }
        }

        public TRecord this[ObjectId id]
        {
            get { return this.GetRecord(id); }
        }

        #endregion

        #region Constructor

        protected RecordCollection(PyrrhaDocument document, ObjectId tableid, OpenMode openMode = OpenMode.ForRead)
        {
            this.ObjectManager = document.ObjectManager;
            this._mode = openMode;
            this._isReadOnly = false;
            this._tableId = tableid;
            this._recordTable = null;

            this.Refresh();
        }

        #endregion

        #region Methods

        private TRecord GetIndex(ObjectId id)
        {
            throw new NotImplementedException();
        }

        private TRecord GetIndex(int index)
        {
            using (var iter = this.RecordTable.GetEnumerator())
            {
                int i = -1;
                while (i++ != index)
                    iter.MoveNext();

                return this.GetRecord(iter.Current);
            }
        }
        private TRecord GetIndex(string recordName)
        {
            var id = this.RecordTable[recordName];
            return this.GetRecord(id);
        }

        private int GetCount()
        {
            int result = 0;
            using (var iter = this.RecordTable.GetEnumerator())
            {
                while (iter.MoveNext())
                    result++;
            }

            return result;
        }
        private TTable GetRecordTable()
        {
            return (TTable)this.Transaction.GetObject(this._tableId, this.Mode);
        }
        private void SetIndex(TRecord value)
        {
            if (this.ObjectManager.OpenObjects.ContainsKey(value.Id))
                this.ObjectManager.OpenObjects[value.Id] = value;
        }

        protected TRecord GetRecord(ObjectId id)
        {
            if (id == null)
                throw new PyrrhaException("ObjectId cannot be null");

            if (this.ObjectManager == null)
                throw new PyrrhaException("ObjectManager is null");

            return (TRecord) this.ObjectManager.GetObject(id, this.Transaction, this.Mode);
        }

        public void Commit()
        {
            this.Transaction.Commit();
            this.Transaction.Dispose();
        }
        public bool Contains(ObjectId id)
        {
            return this.RecordTable.Has(id);
        }
        public void Refresh()
        {
            foreach (var id in this.RecordTable)
                this.GetRecord(id);
        }

        #region ICollection

        public virtual void Add(TRecord item)
        {
            if (Contains(item))
                throw new PyrrhaException("This {0} already exists in the collection", item.GetType().Name);

            if (item == null)
                throw new PyrrhaException("Item cannot be null.");

            this.RecordTable.Add(item); // Add to the Record Table
            this.Transaction.AddNewlyCreatedDBObject(item, true);
            this.GetRecord(item.Id); // Make sure its managed
        }
        public void Clear()
        {
            foreach (var obj in this)
                this.Remove(obj);
        }
        public bool Contains(TRecord item)
        {
            return this.RecordTable.Has(item.Id);
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

            if (this.RecordTable.Has(item.Id))
                item.Erase();

            return true;
        }

        #endregion

        #region IEnumerable

        public IEnumerator<TRecord> GetEnumerator()
        {
            if (this.ObjectManager == null)
                throw new PyrrhaException("ObjectManager is null");

            using (var iter = this.RecordTable.GetEnumerator())
            {
                while (iter.MoveNext())
                    yield return this.GetRecord(iter.Current);
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IDisposable

        ~RecordCollection()
        {
            this.Dispose(false);
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
                return;

            if (disposing)
                this.Transaction.Dispose();

            this._disposed = true;
        }

        #endregion

        #endregion
    }
}

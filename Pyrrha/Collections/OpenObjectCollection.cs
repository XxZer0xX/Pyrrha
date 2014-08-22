using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace Pyrrha.Collections
{
    public abstract class OpenObjectCollection<T> : ICollection<T>, IDisposable where T : DBObject
    {
        private ICollection<T> _innerList;
        private Transaction _transaction;

        protected Transaction Transaction
        {
            get { return this._transaction ?? (this._transaction = this.ObjectManager.GetTransaction()); }
        }

        protected OpenObjectManager ObjectManager { get; private set; }

        #region Constructors

        protected internal OpenObjectCollection(OpenObjectManager manager)
        {
            ObjectManager = manager;
            _innerList = new Collection<T>();
        }

        #endregion

        #region ICollection Implementation

        public virtual int Count { get { return _innerList.Count; } }
        
        // non multithreading FTM
        public object SyncRoot { get { return null; } }
        public bool IsSynchronized { get { return false; } }

        public virtual void Add(T item)
        {
            if (!_innerList.Contains(item))
                _innerList.Add(item);
        }

        public virtual void Clear()
        {
            _innerList.Clear();
        }

        public virtual bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public virtual bool Remove(T item)
        {
            return _innerList.Remove(item);
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed;

        ~OpenObjectCollection()
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
            if (this._disposed || !disposing)
                return;

            this.Transaction.Dispose();
            this._disposed = true;
        }

        #endregion
    }
}

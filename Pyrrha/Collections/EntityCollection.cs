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
    public abstract class EntityCollection<T> : ICollection<T> where T : Entity
    {
               
        #region Properties

        private readonly Transaction _transaction;
        private IList<T> _innerList;
        private bool _disposed;

        public int Count
        {
            get { return _innerList.Count; }
        }

        public bool IsReadOnly { get; private set; }

        public OpenObjectManager ObjectManager { get; private set; }

        public OpenMode OpenMode
        {
            get { return _openMode; }
            set
            {
                switch (value)
                {
                    case OpenMode.ForWrite:
                        foreach (var obj in _innerList)
                            obj.UpgradeOpen();
                        break;
                    case OpenMode.ForRead:
                        foreach (var obj in _innerList)
                            obj.DowngradeOpen();
                        break;
                }
                _openMode = value;
            }
        }
        private OpenMode _openMode;

        #endregion

        #region Constructors

        protected EntityCollection(OpenObjectManager manager, OpenMode openmode)
        {
            IsReadOnly = false;
            ObjectManager = manager;
            _transaction = ObjectManager.AddTransaction();
            _openMode = openmode;
            _innerList = new List<T>();
        }

        protected EntityCollection(OpenObjectManager manager, IEnumerable<ObjectId> ids, OpenMode openmode = OpenMode.ForRead)
            : this(manager, openmode)
        {
            foreach (var id in ids)
                _innerList.Add(GetObject(id));
        }

        
        #endregion

        #region Methods

        public T this[ObjectId id]
        {
            get { return (T)ObjectManager.OpenObjects[id]; }
            set { ObjectManager.OpenObjects[id] = value; }
        }

        public T this[int index]
        {
            get { return _innerList[index]; }
            set { _innerList[index] = value; }
        }

        private T GetObject(ObjectId id)
        {
            if (ObjectManager.OpenObjects.ContainsKey(id))
            {
                return (T)ObjectManager.OpenObjects[id];
            }

            var obj = (T) _transaction.GetObject(id, OpenMode);
            ObjectManager.OpenObjects.Add(id, obj);

            return obj;
        }

        #endregion

        #region IDisposable Implementation

        ~EntityCollection()
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
                Clear();

            _disposed = true;
        }

        #endregion

        #region ICollection Implementation

        public void Add(T item)
        {
            if (Contains(item))
                throw new PyrrhaException("This {0} already exists in the collection", item.GetType().Name);

            _innerList.Add(item);
            GetObject(item.Id);
        }

        public void Clear()
        {
            foreach (var obj in _innerList)
                Remove(obj);
            _innerList.Clear();
        }

        public void Commit()
        {
            _transaction.Commit();
            ObjectManager.Transactions.Remove(_transaction);
            _transaction.Dispose();
            
        }

        public bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        public bool ContainsId(ObjectId id)
        {
            return _innerList.Any(o => o.Id == id);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public void Refresh()
        {
            var refreshList = _innerList.Select(obj => GetObject(obj.Id)).ToList();
            _innerList = refreshList;
        }

        public bool Remove(T item)
        {
            if (!Contains(item))
                throw new PyrrhaException("The {0} does not exist in the collection", item.GetType().Name);

            ObjectManager.OpenObjects.Remove(item.Id);
            return _innerList.Remove(item);
        }

        public bool RemoveAt(int index)
        {
            return Remove(this[index]);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

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
            get { return this._innerList.Count; }
        }

        public bool IsReadOnly { get; private set; }

        public OpenObjectManager ObjectManager { get; private set; }

        public OpenMode OpenMode
        {
            get { return this._openMode; }
            set
            {
                switch (value)
                {
                    case OpenMode.ForWrite:
                        foreach (var obj in this._innerList)
                            obj.UpgradeOpen();
                        break;
                    case OpenMode.ForRead:
                        foreach (var obj in this._innerList)
                            obj.DowngradeOpen();
                        break;
                }
                this._openMode = value;
            }
        }
        private OpenMode _openMode;

        #endregion

        #region Constructors

        protected EntityCollection(OpenObjectManager manager, OpenMode openmode)
        {
            this.IsReadOnly = false;
            this.ObjectManager = manager;
            this._transaction = this.ObjectManager.AddTransaction();
            this._openMode = openmode;
            this._innerList = new List<T>();
        }

        protected EntityCollection(OpenObjectManager manager, IEnumerable<ObjectId> ids, OpenMode openmode = OpenMode.ForRead)
            : this(manager, openmode)
        {
            foreach (var id in ids)
                this._innerList.Add(this.GetObject(id));
        }

        
        #endregion

        #region Methods

        public T this[ObjectId id]
        {
            get { return (T)this.ObjectManager.OpenObjects[id]; }
            set { this.ObjectManager.OpenObjects[id] = value; }
        }

        public T this[int index]
        {
            get { return this._innerList[index]; }
            set { this._innerList[index] = value; }
        }

        private T GetObject(ObjectId id)
        {
            if (this.ObjectManager.OpenObjects.ContainsKey(id))
            {
                return (T)this.ObjectManager.OpenObjects[id];
            }

            var obj = (T) this._transaction.GetObject(id, this.OpenMode);
            this.ObjectManager.OpenObjects.Add(id, obj);

            return obj;
        }

        #endregion

        #region IDisposable Implementation

        ~EntityCollection()
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
                this.Clear();

            this._disposed = true;
        }

        #endregion

        #region ICollection Implementation

        public void Add(T item)
        {
            if (this.Contains(item))
                throw new PyrrhaException("This {0} already exists in the collection", item.GetType().Name);

            this._innerList.Add(item);
            this.GetObject(item.Id);
        }

        public void Clear()
        {
            foreach (var obj in this._innerList)
                this.Remove(obj);
            this._innerList.Clear();
        }

        public void Commit()
        {
            this._transaction.Commit();
            this.ObjectManager.Transactions.Remove(this._transaction);
            this._transaction.Dispose();
            
        }

        public bool Contains(T item)
        {
            return this._innerList.Contains(item);
        }

        public bool ContainsId(ObjectId id)
        {
            return this._innerList.Any(o => o.Id == id);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._innerList.CopyTo(array, arrayIndex);
        }

        public void Refresh()
        {
            var refreshList = this._innerList.Select(obj => this.GetObject(obj.Id)).ToList();
            this._innerList = refreshList;
        }

        public bool Remove(T item)
        {
            if (!this.Contains(item))
                throw new PyrrhaException("The {0} does not exist in the collection", item.GetType().Name);

            this.ObjectManager.OpenObjects.Remove(item.Id);
            return this._innerList.Remove(item);
        }

        public bool RemoveAt(int index)
        {
            return this.Remove(this[index]);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}

#region Referencing

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;
using Pyrrha.Runtime.Exception;

#endregion

namespace Pyrrha.Collections
{
    public abstract class BaseObjectCollection<T> : ICollection<T> where T : DBObject
    {
        private bool isReadOnly;
        private readonly IList<T> _innerList;
        private OpenMode openMode;
        public OpenObjectManager<T> ObjectManager;

        #region Properties

        public int Count
        {
            get { return this._innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public OpenMode OpenMode
        {
            get { return this.openMode; }
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
                this.isReadOnly = value == OpenMode.ForRead;
                this.openMode = value;
            }
        }

        #endregion

        #region Constructors

        protected BaseObjectCollection(OpenMode openmode, OpenObjectManager<T> manager)
        {
            this.openMode = openmode;
            this._innerList = new List<T>();
            ObjectManager = manager;
        }

        protected BaseObjectCollection(IEnumerable<ObjectId> ids, OpenObjectManager<T> manager, OpenMode openmode = OpenMode.ForRead)
            : this(openmode, manager)
        {
            foreach (var id in ids)
                this._innerList.Add(this.Refresh(id));
        }

        #endregion

        #region Methods

        public T this[ObjectId id]
        {
            get { return this._innerList.FirstOrDefault(obj => obj.ObjectId.Equals(id)) ?? this.Refresh(id); }
            set { this.SetObjinList(id,value); }
        }

        private void SetObjinList(ObjectId id, T tObj)
        {
            var collectionObj = this._innerList.FirstOrDefault(obj => obj.ObjectId.Equals(id));
            if(collectionObj == null)
                (new InvalidAccessException("Object Doesn't exist in collection.")).ThrowException();
            
            collectionObj = tObj;
        }

        private T Refresh(ObjectId id)
        {
            return (T)this.ObjectManager.GetObject(id);
        }

        #endregion

        #region ICollection Implementation

        public void Add(T item)
        {
            if (this.Contains(item))
                (new InvalidAccessException(string.Format("This {0} already exists in the collection",
                    item.GetType().Name))).ThrowException();

            this._innerList.Add(item);
        }

        public void Clear()
        {
            this._innerList.Clear();
        }

        public bool Contains(T item)
        {
            return this._innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (!this.Contains(item))
            {
                var exception =
                    new InvalidAccessException(string.Format("The {0} does not exist in the collection",
                                                               item.GetType().Name));
                exception.ThrowException();
            }
            return this._innerList.Remove(item);
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

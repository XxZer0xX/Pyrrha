using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;

namespace Pyrrha.Collections
{
    public abstract class RecordCollection<T> : ICollection<T> where T : SymbolTableRecord
    {
        #region Properties

        private readonly IList<ObjectId> _innerList;

        internal Transaction Transaction
        {
            get { return _transaction ?? (_transaction = Manager.AddTransaction()); }
            set { _transaction = value; }
        }
        private Transaction _transaction;

        protected OpenObjectManager Manager { get; private set; }
        protected SymbolTable Table { get; private set; }
        protected ObjectId TableId { get; private set; }

        public OpenMode OpenMode
        {
            get { return _openMode; }
            set
            {
                _openMode = value;
                foreach (var id in _innerList)
                    GetManagedRecord(id);
            }
        }
        private OpenMode _openMode;

        public T this[int index]
        {
            get
            { 
                return (T)GetManagedRecord(_innerList[index]);
            }
            set
            {
                if (value == null)
                    throw new PyrrhaException("Item cannot be null.");

                Manager.OpenObjects[_innerList[index]] = value;
            }
        }

        #endregion

        #region Constructor

        protected RecordCollection(PyrrhaDocument document, ObjectId tableid, OpenMode openMode = OpenMode.ForRead)
        {
            Manager = document.ObjectManager;
            TableId = tableid;

            _openMode = openMode;
            _innerList = new List<ObjectId>();
            Table = (SymbolTable)Transaction.GetObject(TableId, OpenMode);

            Refresh();
        }

        #endregion

        #region Methods

        private T GetManagedRecord(ObjectId id)
        {
            if (Manager == null)
                throw new PyrrhaException("ObjectManager is null");

            return Manager.GetRecord(id, this);
        }

        public virtual void Add(T item)
        {
            if (Contains(item))
                throw new PyrrhaException("This {0} already exists in the collection", item.GetType().Name);

            if (item == null)
                throw new PyrrhaException("Item cannot be null.");

            Table.Add(item);
            Transaction.AddNewlyCreatedDBObject(item, true);
            GetManagedRecord(item.Id);
            _innerList.Add(item.Id);
        }

        public virtual void Clear()
        {
            foreach (var item in _innerList)

            _innerList.Clear();
        }

        public void CommitChanges()
        {
            Manager.CommitTransaction(Transaction);
            Refresh();
        }

        public bool Contains(T item)
        {
            return _innerList.Contains(item.Id);
        }

        public bool Contains(ObjectId id)
        {
            return _innerList.Contains(id);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var currentObjects = GetAllObjects().ToArray();
            currentObjects.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _innerList.Count; }
        }

        public IEnumerable<T> GetAllObjects()
        {
            if (Manager == null)
                throw new PyrrhaException("ObjectManager is null");

            // Return all objects in the Manager that match the collection Ids
            return _innerList.Select(id => Manager.GetRecord(id, this))
                             .Where(obj => obj != null);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Refresh()
        {
            foreach (var id in Table)
            {
                GetManagedRecord(id);
                _innerList.Add(id);
            }
        }

        public virtual bool Remove(T item)
        {
            if (!Contains(item))
                throw new PyrrhaException("The {0} does not exist in the collection", item.GetType().Name);

            Manager.RemoveObject(item.Id, true);
            return _innerList.Remove(item.Id);
        }

        #region IEnumerable Implementation

        public IEnumerator<T> GetEnumerator()
        {
            return GetAllObjects().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #endregion

        
    }
}

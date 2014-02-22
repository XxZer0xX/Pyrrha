using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;

namespace Pyrrha.Collections
{
    public abstract class RecordCollection<T> : ICollection<T> where T : SymbolTableRecord
    {
        private readonly SymbolTable _symbolTable;
        private readonly IList<T> _innerList;

        internal Transaction Transaction
        {
            get { return _transaction ?? (_transaction = Manager.AddTransaction()); }
            set { _transaction = value; }
        }
        private Transaction _transaction;

        protected OpenObjectManager Manager { get; private set; }
        protected ObjectId TableId { get; private set; }

        public OpenMode OpenMode
        {
            get { return _openMode; }
            set
            {
                switch (value)
                {
                    case OpenMode.ForWrite:
                        foreach (var item in _innerList)
                            item.UpgradeOpen();
                        break;
                    case OpenMode.ForRead:
                        foreach (var item in _innerList)
                            item.DowngradeOpen();
                        break;
                }

                _openMode = value;
            }
        }
        private OpenMode _openMode;

        public T this[int index]
        {
            get { return _innerList[index]; }
            set
            {
                if (value == null)
                    throw new PyrrhaException("Item cannot be null.");

                _innerList[index] = value;
            }
        }
        public T this[string name]
        {
            get { return _innerList.First(o => o.Name == name); }
            set
            {
                if (value == null)
                    throw new PyrrhaException("Item cannot be null.");

                this[name] = value;
            }
        }

        protected RecordCollection(PyrrhaDocument document, ObjectId tableid, OpenMode openMode = OpenMode.ForRead)
        {
            Manager = document.ObjectManager;
            TableId = tableid;

            _openMode = openMode;
            _innerList = new List<T>();
            _symbolTable = (SymbolTable)Transaction.GetObject(TableId, OpenMode);

            Refresh();
        }

        public void CommitChanges()
        {
            Manager.CommitTransaction(Transaction);
        }

        public void Refresh()
        {
            foreach (var record in _symbolTable)
                _innerList.Add((T)Transaction.GetObject(record, OpenMode));
        }

        public void Add(T item)
        {
            if (Contains(item))
                throw new PyrrhaException("This {0} already exists in the collection", item.GetType().Name);

            _symbolTable.Add(item);
            Transaction.AddNewlyCreatedDBObject(item, true);
            _innerList.Add(item);
        }

        public void Clear()
        {
            foreach (var item in _innerList)
                item.Erase();
            _innerList.Clear();
        }

        public bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            if (!Contains(item))
                throw new PyrrhaException("The {0} does not exist in the collection", item.GetType().Name);

            item.Erase();
            return _innerList.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

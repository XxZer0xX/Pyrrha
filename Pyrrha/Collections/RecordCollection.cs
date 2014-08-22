#region Referencing

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Pyrrha.Runtime;

#endregion

namespace Pyrrha.Collections
{
    public class RecordCollection<TTable,TRecord> : OpenObjectCollection<TRecord>
        where TTable : SymbolTable
        where TRecord : SymbolTableRecord
    {
        #region Properties
        
        private readonly ObjectId _tableId;

        internal TTable RecordTable { get; set; }

        public bool Has(string record)
        {
            return this.RecordTable.Has(record);
        }

        public OpenMode Mode
        {
            get { return this._mode; }
            set
            {
                this._mode = value;

                // What is this doing?
                foreach (var id in this.RecordTable)
                    this.GetRecord(id);
            }
        }
        private OpenMode _mode;

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
            : base(document.ObjectManager)
        {
            this._mode = openMode;
            this._tableId = tableid;
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
                var i = -1;
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

            return (TRecord)this.ObjectManager.GetObject(id, this.Transaction, this.Mode);
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
            RecordTable = (TTable)this.Transaction.GetObject(this._tableId, this.Mode);
            foreach (var id in this.RecordTable)
                this.GetRecord(id);
        }

        #endregion

        #region Overridden ICollection

        new public virtual void Add(TRecord item)
        {
            if (Contains(item))
                throw new PyrrhaException("This {0} already exists in the collection", item.GetType().Name);

            if (item == null)
                throw new PyrrhaException("Item cannot be null.");

            this.RecordTable.Add(item); // Add to the Record Table
            this.Transaction.AddNewlyCreatedDBObject(item, true);
            this.GetRecord(item.Id); // Make sure its managed
        }

        // What is this?
        public override void Clear()
        {
            foreach (var obj in this)
                this.Remove(obj);
        }

        public override bool Contains(TRecord item)
        {
            return this.RecordTable.Has(item.Id);
        }

        public override void CopyTo(TRecord[] array, int arrayIndex)
        {
            var currentObjects = this.ToArray();
            currentObjects.CopyTo(array, arrayIndex);
        }

        new public virtual bool Remove(TRecord item)
        {
            if (!Contains(item))
                throw new PyrrhaException("The {0} does not exist in the collection", item.GetType().Name);

            if (this.RecordTable.Has(item.Id))
                item.Erase();

            return true;
        }

        #endregion

        #region Overridden IEnumerable

        public override IEnumerator<TRecord> GetEnumerator()
        {
            if (this.ObjectManager == null)
                throw new PyrrhaException("ObjectManager is null");

            using (var iter = this.RecordTable.GetEnumerator())
            {
                while (iter.MoveNext())
                    yield return this.GetRecord(iter.Current);
            }
        }

        #endregion
    }
}

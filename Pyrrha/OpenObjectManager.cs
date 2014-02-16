#region Referencing

using Autodesk.AutoCAD.DatabaseServices;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Pyrrha
{
    public class OpenObjectManager<TStored> : IEqualityComparer<TStored>, IEnumerable<TStored> where TStored : DBObject
    {
        private readonly IDictionary<ObjectId, TStored> _openObjects;
        private readonly OpenCloseTransaction _transaction;

        #region Properties

        public int Count
        {
            get { return this._openObjects.Count; }
        }

        #endregion

        #region Constructor

        public OpenObjectManager()
        {
            this._transaction = new OpenCloseTransaction();
            this._openObjects = new Dictionary<ObjectId, TStored>();
        }

        #endregion

        #region Methods

        public DBObject GetObject(ObjectId id)
        {
            return this._openObjects.ContainsKey(id)
                ? this._openObjects[id]
                : this._getAddObject(id);
        }

        public IEnumerable<TDesired> GetAllOfType<TDesired>(IEnumerable source)
            where TDesired : DBObject
        {
            return source.OfType<ObjectId>().Any()
                ? source.Cast<ObjectId>().Select(this.GetObject).OfType<TDesired>()
                : source.OfType<TDesired>();
        }

        public void ConfirmAllChanges()
        {
            this._transaction.Commit();
            this._transaction.Dispose();
        }

        internal void Remove(ObjectId id)
        {
            var obj = this._openObjects[id];
            if (obj == null) return;
            obj.Dispose();
            this._openObjects.Remove(id);
        }

        private DBObject _getAddObject(ObjectId id)
        {
            var obj = this._transaction.GetObject(id, OpenMode.ForWrite);
            this._openObjects.Add(id, (TStored)obj);
            return obj;
        }

        #endregion

        #region IEnumerable Implementation

        public IEnumerator<TStored> GetEnumerator()
        {
            return this._openObjects.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IEqualityComparer Implementation

        public bool Equals(TStored x, TStored y)
        {
            return x.ObjectId.Equals(y.ObjectId);
        }

        public int GetHashCode(TStored obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace Pyrrha
{
    internal class OpenObjectManager<T> : IEqualityComparer<T>, IEnumerable<T> where T : DBObject
    {
        private readonly IDictionary<ObjectId,T> _openObjects;

        internal TransactionManager _transManager;

        public int Count
        {
            get { return _openObjects.Count; }
        }

        public OpenObjectManager(TransactionManager manager)
        {
            _transManager = manager;
            this._openObjects = new Dictionary<ObjectId,T>();
        }

        public T GetObject(ObjectId id)
        {
            var returnObj = _openObjects.Keys.Contains(id)? _openObjects[id]:;
        }

        public IEnumerable<T> GetAllOfType(Type type)
        {
            return _openObjects.Values.Where(obj => obj.GetType() == type);
                
        }

        internal void Add(T obj)
        {
            if (!_openObjects.Contains(obj))
                _openObjects.Add(obj);
        }

        internal void Remove(ObjectId id)
        {
            _openObjects.Remove( id );
        }

        private DBObject _getAddObject(ObjectId id)
        {
            var obj = id.Open( OpenMode.ForWrite );
            _openObjects.Add( id, (T) obj );
            return obj;
        }

        #region IEnumerable Implementation

        public IEnumerator<T> GetEnumerator()
        {
            return this._openObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IEqualityComparer Implementation

        public bool Equals( T x , T y )
        {
            return x.ObjectId.Equals( y.ObjectId );
        }

        public int GetHashCode( T obj )
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}

#region Referenceing

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

#endregion

namespace PyrrhaExtenstion.Collections
{
    public abstract class DbObjectCollection<T> : IList<T>, IDisposable where T : DBObject
    {
        private static Dictionary<ObjectId, DBObject> _openDbObjects;

        internal static Dictionary<ObjectId, DBObject> OpenDbObjects
        {
            get { return _openDbObjects ?? ( _openDbObjects = new Dictionary<ObjectId, DBObject>() ); }
            set { _openDbObjects = value; }
        }

        #region IList Implementation

        private IList<T> _backingCollection;

        protected IList<T> CurrentObjectCollection
        {
            get { return _backingCollection ?? ( _backingCollection = new List<T>() ); }
        }

        public virtual T this[ int index ]
        {
            get { return CurrentObjectCollection[index]; }
            set { CurrentObjectCollection[index] = value; }
        }

        public int Count
        {
            get { return CurrentObjectCollection.Count; }
        }

        public int Length
        {
            get { return CurrentObjectCollection.Count - 1; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return CurrentObjectCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add( T item )
        {
            if ( OpenDbObjects.ContainsKey( item.ObjectId ) )
                return;
            OpenDbObjects.Add( item.ObjectId, item );
            CurrentObjectCollection.Add( item );
        }

        public virtual void Clear()
        {
            foreach ( var obj in CurrentObjectCollection )
            {
                OpenDbObjects.Remove( obj.ObjectId );
                obj.Dispose();
            }
            CurrentObjectCollection.Clear();
        }

        public virtual bool Contains( T item )
        {
            return CurrentObjectCollection.Contains( item );
        }

        public virtual void CopyTo( T[] array, int arrayIndex )
        {
            CurrentObjectCollection.CopyTo( array, arrayIndex );
        }

        public virtual bool Remove( T item )
        {
            if ( !OpenDbObjects.ContainsKey( item.ObjectId ) )
                return false;
            var obj = OpenDbObjects[item.ObjectId];
            OpenDbObjects.Remove( item.ObjectId );
            obj.Dispose();
            return CurrentObjectCollection.Remove( item );
        }

        public bool Remove( ObjectId objectId )
        {
            OpenDbObjects.Remove( objectId );
            return CurrentObjectCollection.Remove( 
                CurrentObjectCollection.First( obj => obj.ObjectId == objectId ) 
                );
        }

        public virtual int IndexOf( T item )
        {
            return CurrentObjectCollection.IndexOf( item );
        }

        public virtual void Insert( int index, T item )
        {
            if ( OpenDbObjects.ContainsKey( item.ObjectId ) )
                return;
            CurrentObjectCollection.Insert( index, item );
            OpenDbObjects.Add( item.ObjectId, item );
        }

        protected void AddRange( IEnumerable<T> items )
        {
            foreach ( var item in items )
                Add( item );
        }

        public virtual void RemoveAt( int index )
        {
            T obj = CurrentObjectCollection[index];
            OpenDbObjects.Remove( obj.ObjectId );
            CurrentObjectCollection.RemoveAt( index );
            obj.Dispose();
        }

        public abstract void Commit();

        #endregion

        protected DbObjectCollection() { }

        protected DbObjectCollection( IEnumerable<T> objects )
        {
            foreach ( var dbObject in objects )
            {
                OpenDbObjects.Add( dbObject.ObjectId, dbObject );
                CurrentObjectCollection.Add( dbObject );
            }
        }

        #region IDisposable Implementation

        private bool _isDisposed;

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        public virtual void Dispose( bool disposing )
        {
            if ( _isDisposed || !disposing )
                return;
            _isDisposed = true;
            Clear();
        }

        #endregion
    }
}
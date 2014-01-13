#region Referenceing

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

#endregion

namespace Pyrrha.OverriddenClasses
{
    public class Transaction : OpenCloseTransaction, IEnumerable<DBObject>
    {
        public TransId TransactionId;

        public new TransactionManager TransactionManager { get; set; }

        public Transaction( TransactionManager manager )
        {
            TransactionManager = manager;
            TransactionId = new TransId( UnmanagedObject );
            TransactionManager.AddToTransactionList( TransactionId, this );
        }

        public void AddNewlyCreatedDBObject( DBObject obj )
        {
            AddNewlyCreatedDBObject( obj, true );
        }

        public override void AddNewlyCreatedDBObject( DBObject obj, bool add )
        {
            TransactionManager.AddNewlyCreatedDBObject( obj );
        }

        public override DBObject GetObject( ObjectId id, OpenMode mode = OpenMode.ForWrite, bool openErased = false,
            bool forceOpenOnLockedLayer = true )
        {
            DBObject dbo = TransactionManager.HasOpenObject( id );
            if ( dbo != null )
                return dbo;
            dbo = base.GetObject( id, mode, openErased, forceOpenOnLockedLayer );
            appendToCollections( id, dbo );
            return dbo;
        }

        public override void Commit()
        {
            for ( int i = OpenObjects.Count - 1; i > -1; i-- )
            {
                if ( TransactionManager.HasMultipleInstancesOfObjectOnCommit( this, OpenObjects[i].ObjectId ) )
                {
                    OpenObjects.RemoveAt( i );
                    OpenObjects[i].Dispose();
                }
            }
            base.Commit();
        }

        public IList<DBObject> GetAllObjects()
        {
            return OpenObjects;
        }

        #region IEnumerable Implementation

        private IList<ObjectId> _openObjectsIds;
        private IList<DBObject> _openObjects;

        internal IList<ObjectId> OpenObjectsIds
        {
            get { return _openObjectsIds ?? ( _openObjectsIds = new List<ObjectId>() ); }
            set { _openObjectsIds = value; }
        }

        internal IList<DBObject> OpenObjects
        {
            get { return _openObjects ?? ( _openObjects = new List<DBObject>() ); }
            set { _openObjects = value; }
        }

        public IEnumerator<DBObject> GetEnumerator()
        {
            IEnumerator<DBObject> i = OpenObjects.GetEnumerator();
            i.Current.UpgradeOpen();
            return i;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void appendToCollections( ObjectId id, DBObject obj )
        {
            OpenObjectsIds.Add( id );
            OpenObjects.Add( obj );
        }

        #endregion

        #region IDisposable override Implementation

        protected override void Dispose( bool disposing )
        {
            if ( disposing && !IsDisposed )
            {
                foreach ( var openObject in OpenObjects )
                    openObject.Dispose();

                GC.SuppressFinalize( this );
            }

            base.Dispose( disposing );
        }

        #endregion
    }
}
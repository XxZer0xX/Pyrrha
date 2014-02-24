using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace Pyrrha
{
    public class PyrrhaTransaction
    {
        private readonly Transaction _innerTransaction;

        public OpenObjectManager Manager {get; private set;}

        public IList<ObjectId> OwnedObjects
        {
            get { return _ownedObjects ?? (_ownedObjects = GetOwnedObjects()); }
        }
        private IList<ObjectId> _ownedObjects; 

        public delegate void ObjectAdded(object sender, TransactionEventArgs e);
        public event ObjectAdded OnObjectAdded;

        public delegate void ObjectRemoved(object sender, TransactionEventArgs e);
        public event ObjectRemoved OnObjectRemoved;

        public delegate void Committed(object sender, EventArgs e);
        public event Committed OnCommitted;

        public PyrrhaTransaction(OpenObjectManager manager)
        {
            Manager = manager;
            _innerTransaction = Manager.AddTransaction();
        }

        private IList<ObjectId> GetOwnedObjects()
        {
            var openIds = _innerTransaction.GetAllObjects();
            return openIds.Cast<ObjectId>().ToList();
        }

        public DBObject QueueGetObject(ObjectId id, OpenMode mode)
        {
            var obj = _innerTransaction.GetObject(id, mode);
            OwnedObjects.Add(id);

            if (OnObjectAdded != null)
            {
                var args = new TransactionEventArgs(id);
                OnObjectAdded(this, args);
            }

            return obj;
        }

        public void QueueRemoveObject(ObjectId id)
        {
            if (OnObjectRemoved != null)
            {
                var args = new TransactionEventArgs(id);
                OnObjectRemoved(this, args);
            }

            OwnedObjects.Remove(id);
        }


    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PyrrhaExtenstion.Util;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using PyrrhaExtenstion;

namespace PyrrhaExtenstion.Collections
{
    public class IndependantCollection<T>
        : DbObjectCollection<T> where T : DBObject
    {

        private static IList<Transaction> TransList
        {
            get { return Pyrrha.TransList; }
            set { Pyrrha.TransList = value; }
        } 

        public IndependantCollection( params ObjectId[] objectIds )
        {
            foreach ( var objectId in objectIds )
            {
                var trans = new OpenCloseTransaction();
                var obj = trans.GetObject( objectId, OpenMode.ForWrite );
                CurrentObjectCollection.Add((T)obj);
                TransList.Add(trans);
            }
        }

        public override bool Remove( T item )
        {
            base.Remove( item );
            var trans = item.GetTransaction();

            if (!TransList.Remove(trans) || trans.IsDisposed) 
                return false;

            trans.Abort();
            trans.Dispose();
            return true;
        }

        public override void RemoveAt( int index )
        {
            base.RemoveAt(index);
            var trans = TransList[index];

            if(trans == null || trans.IsDisposed)
                return;

            TransList.Remove(trans);
            trans.Abort();
            trans.Dispose();
        }

        [Obsolete("Non-Functional. Must Pass ObjectId" , true)]
        public override void Add(T item) { }

        public void Add( ObjectId objectId )
        {
            var trans = new OpenCloseTransaction();
            var obj = trans.GetObject( objectId, OpenMode.ForWrite );
            CurrentObjectCollection.Add((T)obj);
            TransList.Add(trans);
        }

        public override void Clear()
        {
            TransList.ForEach( trans =>
            {
                trans.Abort();
                trans.Dispose();
            });
            base.Clear();
        }

        [Obsolete("Non-Functional. Must Pass ObjectId" , true)]
        public override void Insert(int index , T item) { }

        public override void Commit()
        {
            foreach ( var transaction in TransList )
            {
                transaction.Commit();
                transaction.Dispose();
            }

            Dispose();
        }

        public override void Dispose( bool disposing )
        {
            TransList.ForEach( trans =>
            {
                trans.Abort();
                trans.Dispose();
            });
            base.Dispose( disposing );
        }
    }
    
}

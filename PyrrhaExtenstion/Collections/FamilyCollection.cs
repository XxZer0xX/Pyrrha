using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace PyrrhaExtenstion.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FamilyCollection<T> : DbObjectCollection<T> where T : DBObject
    {
        private readonly Transaction trans;

        public FamilyCollection(params ObjectId[] objectIds)
        {
            trans = AcApp.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction();
            AddRange(objectIds.Select(objId => (T)trans.GetObject(objId,OpenMode.ForWrite)));
        }

        public void Add( ObjectId objectId )
        {
            base.Add((T)trans.GetObject(objectId,OpenMode.ForWrite));
        }

        public void Insert( int index, ObjectId objectId )
        {
            var obj = trans.GetObject( objectId, OpenMode.ForWrite );
            base.Insert(index , (T) obj);
        }

        [Obsolete("Non-Functional. Must Pass ObjectId",true)]
        public override void Add( T item ) { } 

        [Obsolete("Non-Functional. Must Pass ObjectId",true)]
        public override void Insert(int index , T item) { }

        public override void Commit()
        {
            trans.Commit();
            trans.Dispose();
            this.Dispose(true);
        }

        public override void Dispose( bool disposing )
        {
            if ( !trans.IsDisposed )
            {
                trans.Abort();
                trans.Dispose();
            }
            Dispose();
        }
    }
}

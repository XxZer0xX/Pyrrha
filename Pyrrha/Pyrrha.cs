using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace PyrrhaExtenstion
{
    public static class Pyrrha
    {
        internal static Transaction LayerTransaction;
        internal static Transaction TextStyleTransaction;
        internal static Transaction EntityTransaction;
        private static IList<Transaction> _transList;


        internal static IList<Transaction> TransList
        {
            get { return _transList ?? ( _transList = new List<Transaction>() ); }
            set { _transList = value; }
        }

        public static Transaction GetTransaction<T>(this T ent) where T : DBObject
        {
            return TransList.FirstOrDefault(t => t.GetAllObjects().Contains(ent));
        }


        public static void CommitChanges<T>( this IList<T> entList) where T : DBObject
        {
            if (entList.Count == 0)
                return;

            var trans = entList[0].GetTransaction();

            if(trans == null) 
                throw new NullReferenceException();

            trans.Commit();
            TransList.Remove( trans );
            trans.Dispose();
        }

        public static IList<LayerTableRecord> GetLayers(this Document document)
        {
            Transaction trans = document.TransactionManager.StartOpenCloseTransaction();
            TransList.Add(trans);
            return ( (LayerTable) trans.GetObject( document.Database.LayerTableId, OpenMode.ForRead ) )
                .Cast<ObjectId>()
                .Select( objId => (LayerTableRecord) trans.GetObject( objId, OpenMode.ForWrite ) )
                .ToList();
        }

        public static void CommitChanges(this Document document, bool dispose = false)
        {
            foreach ( var transaction in TransList )
            {
                transaction.Commit();
                transaction.Dispose();
                TransList = null;
            }
        }
    }
}

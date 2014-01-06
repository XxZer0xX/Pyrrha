using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using PyrrhaExtenstion.Util;

namespace PyrrhaExtenstion
{
    public static class Pyrrha
    {
        private static IList<Transaction> _transList;
        internal static LanguageType CallerLanguage;

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

        public static IList<TextStyleTableRecord> GetTextStyles(this Document document)
        {
            Transaction trans = document.TransactionManager.StartOpenCloseTransaction();
            TransList.Add(trans);
            return ((TextStyleTable)trans.GetObject(document.Database.LayerTableId , OpenMode.ForRead))
                .Cast<ObjectId>()
                .Select(objId => (TextStyleTableRecord)trans.GetObject(objId , OpenMode.ForWrite))
                .ToList();
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

        /// <summary>
        ///     Loads a linetype into the database.
        /// </summary>
        public static void LoadLinetype( this Database database, string value )
        {
            using ( OpenCloseTransaction trans = database.TransactionManager.StartOpenCloseTransaction() )
            {
                var lineTypeTable = trans.GetObject(
                    database.LinetypeTableId, OpenMode.ForRead ) as LinetypeTable;

                if ( lineTypeTable == null )
                    throw new NullReferenceException( "AutoDesk.AutoCad.DatabaseServices.LineTypeTable" );

                if ( !lineTypeTable.Has( value ) )
                    database.LoadLineTypeFile( value, "acad.lin" );

                trans.Commit();
            }
        }

        /// <summary>
        ///     If document has specified linetype loaded.
        /// </summary>
        public static bool LinetypeIsLoaded( this Document document, string linetype )
        {
            using ( DBObject lineTypeTable = document.Database.LinetypeTableId.Open( OpenMode.ForRead ) )
                return ( (LinetypeTable) lineTypeTable ).Has( linetype );
        }
    }
}

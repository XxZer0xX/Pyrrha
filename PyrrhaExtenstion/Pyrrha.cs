#region Referenceing

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using PyrrhaExtenstion.Collections;
using PyrrhaExtenstion.Util;

#endregion

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

        public static Transaction GetTransaction<T>( this T ent ) where T : DBObject
        {
            return TransList.FirstOrDefault( t => t.GetAllObjects().Contains( ent ) );
        }


        public static void Commit<T>( this T ent ) where T : DBObject
        {
            var trans = ent.GetTransaction();
            ent.Dispose();
            if ( trans != null )
            {
                TransList.Remove(trans);
                trans.Commit();
                trans.Dispose();
            }
            
        }

        public static DbObjectCollection<TextStyleTableRecord> GetTextStyles( this Document document,
            LifetimeManager managmentType )
        {
            using ( var textStyleTable = (TextStyleTable) document.Database.TextStyleTableId.Open( OpenMode.ForRead ) )
                switch ( managmentType )
                {
                    case LifetimeManager.Family:
                        return textStyleTable.ToFamilyCollection<TextStyleTableRecord>();
                    case LifetimeManager.Independant:
                        return textStyleTable.ToIndependantCollection<TextStyleTableRecord>();
                    case LifetimeManager.Open:
                        return textStyleTable.ToOpenCollection<TextStyleTableRecord>();
                    default:
                        throw new ArgumentOutOfRangeException( "managmentType" );
                }
        }

        public static DbObjectCollection<LayerTableRecord> GetLayers( this Document document,
            LifetimeManager managmentType )
        {
            using ( var layerTable = (LayerTable) document.Database.LayerTableId.Open( OpenMode.ForRead ) )
                switch ( managmentType )
                {
                    case LifetimeManager.Family:
                        return layerTable.ToFamilyCollection<LayerTableRecord>();
                    case LifetimeManager.Independant:
                        return layerTable.ToIndependantCollection<LayerTableRecord>();
                    case LifetimeManager.Open:
                        return layerTable.ToOpenCollection<LayerTableRecord>();
                    default:
                        throw new ArgumentOutOfRangeException( "managmentType" );
                }
        }

        public static void CommitChanges( this Document document, bool dispose = false )
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

        public static FamilyCollection<T> ToFamilyCollection<T>( this SymbolTable table ) where T : DBObject
        {
            return new FamilyCollection<T>( table.Cast<ObjectId>().ToArray() );
        }

        public static IndependantCollection<T> ToIndependantCollection<T>( this SymbolTable table ) where T : DBObject
        {
            return new IndependantCollection<T>( table.Cast<ObjectId>().ToArray() );
        }

        public static OpenCollection<T> ToOpenCollection<T>( this SymbolTable table ) where T : DBObject
        {
            return new OpenCollection<T>( table.Cast<ObjectId>().ToArray() );
        }
    }
}
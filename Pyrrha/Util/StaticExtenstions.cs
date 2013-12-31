#region Referenceing

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Pyrrha.SelectionFilter;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

#endregion

namespace Pyrrha.Util
{
    public static class StaticExtenstions
    {

        internal const string Pyrrha = "PYRRHA";
        /// <summary>
        ///     Loads a linetype into the MdiActiveDocument
        /// </summary>
        public static void LoadLinetype( string value )
        {
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = AcApp.DocumentManager.MdiActiveDocument;

            using ( OpenCloseTransaction trans = acDoc.TransactionManager.StartOpenCloseTransaction() )
            {
                var lineTypeTable = trans.GetObject(
                    acDoc.Database.LinetypeTableId, OpenMode.ForRead ) as LinetypeTable;

                if ( lineTypeTable == null )
                    throw new NullReferenceException( "AutoDesk.AutoCad.DatabaseServices.LineTypeTable" );

                if ( !lineTypeTable.Has( value ) )
                    acDoc.Database.LoadLineTypeFile( value, "acad.lin" );

                trans.Commit();
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

        /// <summary>
        ///     If document has specified linetype loaded.
        /// </summary>
        public static void PurgeDataBase( this Document document )
        {
            ObjectIdCollection objIdCollection = IterateDb( document.Database );
            using ( OpenCloseTransaction trans = document.TransactionManager.StartOpenCloseTransaction() )
            {
                document.Database.Purge( objIdCollection );

                //
                //  This is crap and need to find a better way... TODO!
                //  --------------------------------------------------

                foreach ( ObjectId objId in objIdCollection )
                    try
                    {
                        ( trans.GetObject( objId, OpenMode.ForWrite ) ).Erase();
                    }
                    catch ( Exception e )
                    {
                        if ( !e.ErrorStatus.Equals( ErrorStatus.WasErased ) &&
                             !e.ErrorStatus.Equals( ErrorStatus.CannotBeErasedByCaller ) &&
                             !e.Message.Equals( "eVSIsAcadDefault", StringComparison.CurrentCultureIgnoreCase ) ) throw;
                    }
                //  --------------------------------------------------

                trans.Commit();
            }
        }

        public static void SendCommandSynchronously( this Autodesk.AutoCAD.ApplicationServices.Document document,
            string commandString )
        {
            object[] data = {commandString + "\n"};
            object comDocument = document.AcadDocument;
            comDocument.GetType().InvokeMember( "SendCommand", BindingFlags.InvokeMethod,
                null, comDocument, data );
        }

        public static void WriteToActiveDocument( string message )
        {
            if ( AcApp.DocumentManager.MdiActiveDocument != null )
                AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage( message );
        }

        private static ObjectIdCollection IterateDb( Database dbParameter )
        {
            var objIdCollection = new ObjectIdCollection();
            Handle handseed = dbParameter.Handseed;
            long totalHandles = handseed.Value;

            for ( long i = 1; i < totalHandles; ++i )
            {
                var objId = new ObjectId();

                if ( !( dbParameter.TryGetObjectId( new Handle( i ), out objId ) ) )
                    continue;

                if ( !objId.IsValid || objId.IsEffectivelyErased )
                    continue;

                using ( OpenCloseTransaction trans = dbParameter.TransactionManager.StartOpenCloseTransaction() )
                {
                    using ( DBObject obj = trans.GetObject( objId, OpenMode.ForRead, false, false ) )

                        objIdCollection.Add( objId );

                    trans.Commit();
                }
            }
            return objIdCollection;
        }

        public static Color GenerateAutoCadColor( short colorIndex )
        {
            return Color.FromColorIndex( ColorMethod.ByAci, colorIndex );
        }

        public static BlockReference CreateNewBlock( this Autodesk.AutoCAD.ApplicationServices.Document acDoc,
            string definitionName,
            Scale3d scale,
            Point3d pos,
            string layerName )
        {
            // Create placeholder for new block
            BlockReference blkRef = null;

            Database Database = acDoc.Database;

            // Start transaction
            using ( OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction() )
            {
                // Open the block table
                var blkTbl = (BlockTable) trans.GetObject( Database.BlockTableId, OpenMode.ForRead );

                // Check for the block definition in the block table
                if ( !blkTbl.Has( definitionName ) )
                    return null;

                // Get the record
                var blkRcd = (BlockTableRecord) trans.GetObject( blkTbl[definitionName], OpenMode.ForWrite );

                var layerTable = (LayerTable) trans.GetObject( Database.LayerTableId, OpenMode.ForRead );

                if ( !layerTable.Has( layerName ) )
                    layerName = "0";

                blkRef = new BlockReference( pos.X != 0 || pos.Y != 0
                    ? pos

                    // Have user specify point of insertion and get value
                    : acDoc.Editor.GetPoint(
                        new PromptPointOptions( "Please choose insertion point" ) ).Value, blkRcd.ObjectId )
                {
                    LayerId = layerName == "0" ? layerTable["0"] : layerTable[layerName],
                    ScaleFactors = scale
                };

                if ( blkRef.Position.X == 0 && blkRef.Position.Y == 0 )
                    return null;

                using (
                    var modelSpace =
                        (BlockTableRecord)
                            SymbolUtilityServices.GetBlockModelSpaceId( Database ).Open( OpenMode.ForWrite ) )
                    modelSpace.AppendEntity( blkRef );

                foreach (
                    var attrDef in
                        blkRcd.Cast<ObjectId>().Select( objid => trans.GetObject( objid, OpenMode.ForWrite ) )
                            .Where( obj => obj is AttributeDefinition ) )
                {
                    var attRef = new AttributeReference();
                    attRef.SetAttributeFromBlock( (AttributeDefinition) attrDef, blkRef.BlockTransform );
                    blkRef.AttributeCollection.AppendAttribute( attRef );
                    trans.AddNewlyCreatedDBObject( attRef, true );
                }

                // Append the new block
                trans.AddNewlyCreatedDBObject( blkRef, true );

                //Close transaction
                trans.Commit();
            }
            return blkRef;
        }

        public static IList<T> ApplyFilter<T>( this IList<T> entList, EntitySelectionFilter filter ) where T : Entity
        {
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = AcApp.DocumentManager.MdiActiveDocument;
            Editor acEd = acDoc.Editor;
            Database acDb = acDoc.Database;

            var handles = new List<Handle>();

            using ( OpenCloseTransaction trans = acDb.TransactionManager.StartOpenCloseTransaction() )
            {
                PromptSelectionResult selection = acEd.SelectAll( filter.Selection );
                if ( selection.Status == PromptStatus.Error )
                    return null;
                handles = selection.Value.GetObjectIds().Select( objId =>
                {
                    using ( var ent = (Entity) trans.GetObject( objId, OpenMode.ForRead ) )
                        return ent.Handle;
                } ).ToList();
                trans.Commit();
            }

            var rtnList = new List<T>();
            for ( int i = entList.Count - 1; i >= 0; i-- )
            {
                T entity = entList[i];
                Handle handle = GetHandle( entity );
                if ( !handles.Any( han => handle.Equals( han ) ) )
                {
                    entity.Dispose();
                    continue;
                }

                rtnList.Add( entity );
                handles.Remove( handle );
            }
            return rtnList;
        }

        public static Handle GetHandle<T>( this T ent ) where T : Entity
        {
            return new Handle( Int64.Parse( (string) ent
                .GetXDataForApplication( Pyrrha )
                .AsArray()[1].Value
                , NumberStyles.AllowHexSpecifier ) );
        }

        public static IList<Entity> GetEntityClones( IEnumerable<ObjectId> objectIds )
        {
            var rtnList = new List<Entity>();
            foreach ( var objId in objectIds )
            {
                using ( DBObject actualEntity = objId.Open( OpenMode.ForRead ) )
                {
                    rtnList.Add(
                        CloneAddXData( (Entity) actualEntity ) );
                    actualEntity.Dispose();
                }
            }
            return rtnList;
        }

        public static T CloneAddXData<T>( this T entity ) where T : Entity
        {
            var moddedEntity = (T) entity.Clone();
            var resBuffer = new ResultBuffer(
                new TypedValue( 1001, Pyrrha ), new TypedValue( 1005, entity.Handle ) );
            using ( resBuffer )
                moddedEntity.XData = resBuffer;
            entity.Dispose();
            return moddedEntity;
        }

        public static bool IsScriptSource( this Thread thread )
        {
            var stackFrames = new StackTrace(thread , true).GetFrames();
            return stackFrames.Any(frame
                => frame.GetMethod()
                    .Name.Equals( "ExecutePythonScript",
                        StringComparison.CurrentCultureIgnoreCase ) );
        }

        public static AttributeReference GetAttribute( this BlockReference block, string tag )
        {
            return
                block.AttributeCollection.Cast<AttributeReference>()
                    .FirstOrDefault( attr => attr.Tag.Equals( tag, StringComparison.CurrentCultureIgnoreCase ) );
        }

        public static IDictionary<string , AttributeReference> AttributeDictionary(this BlockReference block)
        {
            return block.AttributeCollection.Cast<AttributeReference>()
                .ToDictionary(attr => attr.Tag , attr => attr);
        }

        public static bool HasAttribute( this BlockReference block, string tag )
        {
            return
                block.AttributeCollection.Cast<AttributeReference>()
                    .Any( attr => attr.Tag.Equals( tag, StringComparison.CurrentCultureIgnoreCase ) );
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration , Action<T> action)
        {
            foreach (T item in enumeration)
                action(item);
        }
    }
}
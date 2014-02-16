#region Referencing

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion

namespace Pyrrha.Util
{
    public static class StaticExtenstions
    {
        internal const string Pyrrha = "PYRRHA";

        /// <summary>
        ///     Loads a linetype into the database.
        /// </summary>
        public static void LoadLinetype(this Database database, string value)
        {
            using (var lineTypeTable = (LinetypeTable)database.LinetypeTableId.Open(OpenMode.ForRead))
                if (lineTypeTable.Has(value))
                    return;

            database.LoadLineTypeFile(value, "acad.lin");
        }

        public static void SendCommandSynchronously(this Document document,
            string commandString)
        {
            object[] data = { commandString + "\n" };
            object comDocument = document.AcadDocument;
            comDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod,
                null, comDocument, data);
        }

        public static void WriteToActiveDocument(string message)
        {
            if (Application.DocumentManager.MdiActiveDocument != null)
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(message);
        }

        public static Color GenerateAutoCadColor(short colorIndex)
        {
            return Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
        }

        //public static BlockReference CreateNewBlock(this Document acDoc,
        //    string definitionName,
        //    Scale3d scale,
        //    Point3d pos,
        //    string layerName)
        //{
        //    // Create placeholder for new block
        //    BlockReference blkRef = null;

        //    Database Database = acDoc.Database;

        //    // Start transaction
        //    using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
        //    {
        //        // Open the block table
        //        var blkTbl = (BlockTable)trans.GetObject(Database.BlockTableId, OpenMode.ForRead);

        //        // Check for the block definition in the block table
        //        if (!blkTbl.Has(definitionName))
        //            return null;

        //        // Get the record
        //        var blkRcd = (BlockTableRecord)trans.GetObject(blkTbl[definitionName], OpenMode.ForWrite);

        //        var layerTable = (LayerTable)trans.GetObject(Database.LayerTableId, OpenMode.ForRead);

        //        if (!layerTable.Has(layerName))
        //            layerName = "0";

        //        blkRef = new BlockReference(pos.X != 0 || pos.Y != 0
        //            ? pos

        //            // Have user specify point of insertion and get value
        //            : acDoc.Editor.GetPoint(
        //                new PromptPointOptions("Please choose insertion point")).Value, blkRcd.ObjectId)
        //        {
        //            LayerId = layerName == "0" ? layerTable["0"] : layerTable[layerName],
        //            ScaleFactors = scale
        //        };

        //        if (blkRef.Position.X == 0 && blkRef.Position.Y == 0)
        //            return null;

        //        using (
        //            var modelSpace =
        //                (BlockTableRecord)
        //                    SymbolUtilityServices.GetBlockModelSpaceId(Database).Open(OpenMode.ForWrite))
        //            modelSpace.AppendEntity(blkRef);

        //        foreach (
        //            var attrDef in
        //                blkRcd.Cast<ObjectId>().Select(objid => trans.GetObject(objid, OpenMode.ForWrite))
        //                    .Where(obj => obj is AttributeDefinition))
        //        {
        //            var attRef = new AttributeReference();
        //            attRef.SetAttributeFromBlock((AttributeDefinition)attrDef, blkRef.BlockTransform);
        //            blkRef.AttributeCollection.AppendAttribute(attRef);
        //            trans.AddNewlyCreatedDBObject(attRef, true);
        //        }

        //        // Append the new block
        //        trans.AddNewlyCreatedDBObject(blkRef, true);

        //        //Close transaction
        //        trans.Commit();
        //    }
        //    return blkRef;
        //}

        //public static IList<T> ApplyFilter<T>( this IList<T> entList, EntitySelectionFilter filter ) where T : Entity
        //{
        //    Autodesk.AutoCAD.ApplicationServices.Document acDoc = AcApp.DocumentManager.MdiActiveDocument;
        //    Editor acEd = acDoc.Editor;
        //    Database acDb = acDoc.Database;

        //    var handles = new List<Handle>();

        //    using ( OpenCloseTransaction trans = acDb.TransactionManager.StartOpenCloseTransaction() )
        //    {
        //        PromptSelectionResult selection = acEd.SelectAll( filter.Selection );
        //        if ( selection.Status == PromptStatus.Error )
        //            return null;

        //        handles = selection.Value.GetObjectIds().Select( objId =>
        //        {
        //            using ( var ent = (Entity) trans.GetObject( objId, OpenMode.ForRead ) )
        //                return ent.Handle;
        //        } ).ToList();
        //        trans.Commit();
        //    }

        //    var rtnList = new List<T>();
        //    for ( int i = entList.Count - 1; i >= 0; i-- )
        //    {
        //        T entity = entList[i];
        //        Handle handle = GetHandle( entity );
        //        if ( !handles.Any( han => handle.Equals( han ) ) )
        //        {
        //            entity.Dispose();
        //            continue;
        //        }

        //        rtnList.Add( entity );
        //        handles.Remove( handle );
        //    }
        //    return rtnList;
        //}

        public static Handle GetHandle<T>(this T ent) where T : Entity
        {
            return new Handle(Int64.Parse((string)ent
                .GetXDataForApplication(Pyrrha)
                .AsArray()[1].Value
                , NumberStyles.AllowHexSpecifier));
        }

        public static AttributeReference GetAttribute(this BlockReference block, string tag)
        {
            return
                block.AttributeCollection.Cast<AttributeReference>()
                    .FirstOrDefault(attr => attr.Tag.Equals(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IDictionary<string, AttributeReference> AttributeDictionary(this BlockReference block)
        {
            return block.AttributeCollection.Cast<AttributeReference>()
                .ToDictionary(attr => attr.Tag, attr => attr);
        }

        public static bool HasAttribute(this BlockReference block, string tag)
        {
            return
                block.AttributeCollection.Cast<AttributeReference>()
                    .Any(attr => attr.Tag.Equals(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action) where T : DBObject
        {
            foreach (var item in enumeration)
                action(item);
        }

        public static IEnumerable<TResult> ForEach<T, TResult>(this IEnumerable enumeration, Func<T, TResult> func) 
            where T : struct
            where TResult : DBObject
        {
            return from object item in enumeration
                   select func( (T) item );
        }

        public static bool IsScriptSource(this Thread thread)
        {
            var stackFrames = new StackTrace(thread, true).GetFrames();
            return stackFrames != null 
                && stackFrames.Any(frame
                => frame.GetMethod()
                        .Name.Equals("ExecutePythonScript",
                                    StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
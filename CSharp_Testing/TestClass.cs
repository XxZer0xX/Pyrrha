#region Referenceing

using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Pyrrha;

using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;

#endregion

namespace CSharp_Testing
{
    public class TestClass
    {
        [CommandMethod("tst1")]
        public void Test1()
        {
            var acDoc = AcApp.DocumentManager.MdiActiveDocument;
            var transA = acDoc.TransactionManager.StartTransaction();
            var transB = acDoc.TransactionManager.StartOpenCloseTransaction();

            var mSpace =
                transA.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(acDoc.Database), OpenMode.ForRead) as BlockTableRecord;

            if (mSpace == null)
                return;

            var linesA = (from ObjectId id in mSpace
                          select transA.GetObject(id, OpenMode.ForRead)).OfType<Line>()
                          .ToList();

            try
            {
                // Get ID from handle
                var idFromHandle = acDoc.Database.GetObjectId(false, linesA[3].Handle, 0);

                // Original transaction
                var normal = linesA[3];
                normal.UpgradeOpen();
                normal.StartPoint = new Autodesk.AutoCAD.Geometry.Point3d(0, 0, 0);
                normal.Close();

                // From ObjectId
                var awesome = (Line)idFromHandle.Open(OpenMode.ForWrite, false);
                awesome.ColorIndex = 2;
                awesome.Close();

                // Original transaction
                var cool = (Line)transA.GetObject(idFromHandle, OpenMode.ForWrite);
                cool.Thickness = 3;
                cool.Close();

                // New Open/Close Transaction
                var amazing = (Line)transB.GetObject(idFromHandle, OpenMode.ForWrite);
                amazing.Layer = "Layer2";

                // Get ID from handle
                var newId = acDoc.Database.GetObjectId(false, amazing.Handle, 0);

            }
            catch (Exception ex)
            {
                acDoc.Editor.WriteMessage(ex.ToString());
            }
            finally
            {
                transB.Commit();
                transA.Commit();

                transB.Dispose();
                transA.Dispose();
            }

            //var layerTableA = (LayerTable)transA.GetObject(acDoc.Database.LayerTableId, OpenMode.ForWrite);

            //var delLayer = transA.GetObject(layerTableA["Layer1"], OpenMode.ForWrite);
            //delLayer.Erase();


            //var layerA = new LayerTableRecord {Name = "LayerATest"};
            //layerTableA.Add(layerA);
            //transA.AddNewlyCreatedDBObject(layerA, true);
            //var pDoc = new PyrrhaDocument();
            //var testLayer = pDoc.Layers[3];
            //var testStyle = pDoc.TextStyles[0];
            //var testLinetype = pDoc.Linetypes[4];


            //transA.Commit();           

            //var doc = new PyrrhaDocument();





            //var layerTest = doc.ObjectManager.GetAllOfType<LayerTableRecord>( doc.LayerTable );
            //var blocksTest = doc.ObjectManager.GetAllOfType<BlockReference>( doc.ModelSpace );
            //doc.ConfirmAllChanges();
        }

    }
}
#region Referenceing

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Pyrrha;

using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

#endregion

namespace CSharp_Testing
{
    public class TestClass
    {
        [CommandMethod("tst1")]
        public void Test1()
        {
            //var acDoc = AcApp.DocumentManager.MdiActiveDocument;

            //var transA = acDoc.TransactionManager.StartTransaction();
            //var layerTableA = (LayerTable)transA.GetObject(acDoc.Database.LayerTableId, OpenMode.ForWrite);

            //var delLayer = transA.GetObject(layerTableA["Layer1"], OpenMode.ForWrite);
            //delLayer.Erase();


            //var layerA = new LayerTableRecord {Name = "LayerATest"};
            //layerTableA.Add(layerA);
            //transA.AddNewlyCreatedDBObject(layerA, true);
            var pDoc = new PyrrhaDocument();
            var testLayer = pDoc.Layers[3];
            pDoc.Layers.CommitChanges(); // Can be called through ObjectManager


            //transA.Commit();           

            //var doc = new PyrrhaDocument();





            //var layerTest = doc.ObjectManager.GetAllOfType<LayerTableRecord>( doc.LayerTable );
            //var blocksTest = doc.ObjectManager.GetAllOfType<BlockReference>( doc.ModelSpace );
            //doc.ConfirmAllChanges();
        }

    }
}
#region Referenceing

using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Pyrrha;

using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;
using System.Diagnostics;
using System;
using Autodesk.AutoCAD.EditorInput;
using System.Reflection;
using Autodesk.AutoCAD.Colors;

#endregion

namespace CSharp_Testing
{
    public class TestClass
    {
        [CommandMethod("pyTest")]
        public void pyTest()
        {
            var pyDoc = new PyrrhaDocument();

            pyDoc.Editor.WriteMessage("\nLayers:\n");
            pyDoc.Layers.Clear();
            
            foreach (var layer in pyDoc.Layers)
            {
                pyDoc.Editor.WriteMessage("{0} - {1}\n", layer.Name, layer.Color);
                if (layer.Name != "0")
                    layer.Name += "_TEST";
            }
                
            pyDoc.Editor.WriteMessage("\nLinetypes:\n");
            foreach (var linetype in pyDoc.Linetypes)
                pyDoc.Editor.WriteMessage("{0} - {1}\n", linetype.Name, linetype.AsciiDescription);

            pyDoc.Editor.WriteMessage("\nTextStyles:\n");
            foreach (var textstyle in pyDoc.TextStyles)
                pyDoc.Editor.WriteMessage("{0} - {1}\n", textstyle.Name, textstyle.FileName);

            pyDoc.ConfirmAllChanges();
        }

        [CommandMethod("TransTest")]
        public void transTest()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acTrans = acDoc.TransactionManager.StartOpenCloseTransaction();

            try
            {
                var mSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(acDoc.Database);
                var mSpace = (BlockTableRecord)acTrans.GetObject(mSpaceId, OpenMode.ForRead);

                var oldId = mSpace.Id;
                foreach (var id in mSpace)
                {
                    var line = acTrans.GetObject(id, OpenMode.ForRead) as Line;
                    if (line == null)
                        continue;

                    line.UpgradeOpen();
                    line.ColorIndex = 34;
                    oldId = line.Id;
                    break;
                }

                //var TestLine = acDoc.TransactionManager.GetObject(oldId, OpenMode.ForWrite);
                var TestLine = oldId.GetObject(OpenMode.ForWrite);
            }
            catch (Exception ex)
            {
                acDoc.Editor.WriteMessage(ex.Message);
            }

            acTrans.Commit();
            acTrans.Dispose();
        }

        [CommandMethod("LineTest")]
        public void LineTest()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acEd = acDoc.Editor;

            var pOptions = new PromptStringOptions("Enter the matrix size of the test: \n");
            pOptions.AllowSpaces = false;
            pOptions.DefaultValue = "10";
            pOptions.UseDefaultValue = true;

            var sizePrompt = acEd.GetString(pOptions);
            if (sizePrompt.Status != PromptStatus.OK)
                return;

            var size = Convert.ToInt32(sizePrompt.StringResult);

            CreateBoard(size);
            CalculateScore();

            // COM Zoom? Really?  That's weak Autodesk... Fucking weak.
            Object acadObject = Application.AcadApplication;
            acadObject.GetType().InvokeMember("ZoomExtents",
                        BindingFlags.InvokeMethod, null, acadObject, null);
        }

        public void CreateBoard(int size)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acEd = acDoc.Editor;

            if (size < 2)
                size = 2;

            //var width = Math.Floor(Math.Sqrt(size));
            //var height = width;

            double len = 1.0;
            double leg = (0.5 * ((len / 2) * Math.Sqrt(2.0))); // for angular lines
            var rand = new Random();
            try
            {
                var sw = Stopwatch.StartNew();
                using (var acTrans = acDoc.TransactionManager.StartOpenCloseTransaction())
                {
                    var mSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(acDoc.Database);
                    var mSpace = (BlockTableRecord)acTrans.GetObject(mSpaceId, OpenMode.ForWrite);

                    var selectPrompt = acEd.SelectAll();
                    var ssAll = selectPrompt.Value;
                    if (ssAll != null)
                    {
                        foreach (ObjectId id in ssAll.GetObjectIds())
                        {
                            var obj = acTrans.GetObject(id, OpenMode.ForWrite);
                            obj.Erase();
                        }
                    }

                    for (int i = 0; i < size; i++)
			        {
			            for (int j = 0; j < size; j++)
                        {
                            var startPoint = new Point3d(j * len, i * len, 0);

                            // Line rotation
                            Vector3d offset;
                            var randPosition = rand.Next(4);
                            if (randPosition == 1)
                                offset = new Vector3d(0, len / 2, 0);// | Vertical
                            else if (randPosition == 2)
                                offset = new Vector3d(len / 2, 0, 0);// -- Horizontal
                            else if (randPosition == 3)
                                offset = new Vector3d(leg, leg, 0);//   / Forward angular
                            else
                                offset = new Vector3d(leg, -leg, 0);//  \ Backward angular

                            var line = new Line(startPoint.Add(offset), startPoint.Subtract(offset));
                            line.ColorIndex = rand.Next(1, 6);

                            mSpace.AppendEntity(line);
                            acTrans.AddNewlyCreatedDBObject(line, true);
                        }
			        }

                    acTrans.Commit();
                    sw.Stop();

                    acEd.WriteMessage("{0} Lines created in {1} ms.\n", size * size, sw.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                acEd.WriteMessage("{0}\n", ex.Message);
                Debug.WriteLine(ex.Message);
            }

           
        }

        public void CalculateScore()
        {
            /* Line color is worth the colorindex score:
             * A red line = 1
             * A blue line = 5
             * 
             * Touching lines are additive
             * 
             */

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acEd = acDoc.Editor;

            long score = 0;

            using (var acTrans = acDoc.Database.TransactionManager.StartOpenCloseTransaction())
            {
                try
                {
                    var mSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(acDoc.Database);
                    var mSpace = (BlockTableRecord)acTrans.GetObject(mSpaceId, OpenMode.ForRead);
                    var layers = (LayerTable)acTrans.GetObject(acDoc.Database.LayerTableId, OpenMode.ForWrite);
                    
                    if (!layers.Has("Horizontals"))
                    {
                        var hLayer = new LayerTableRecord { Name = "Horizontals"};
                        layers.Add(hLayer);
                        acTrans.AddNewlyCreatedDBObject(hLayer, true);
                    }
                        
                    var horizontalId = layers["Horizontals"];

                    foreach(ObjectId id in mSpace)
                    {
                        var line = acTrans.GetObject(id, OpenMode.ForRead) as Line;
                        if (line == null)
                            continue;

                        // Add line color to score
                        score += line.ColorIndex;

                        line.UpgradeOpen();
                        if (line.StartPoint.Y == line.EndPoint.Y)
                            line.SetLayerId(layers["Horizontals"], false);
                        line.Close();
                    }
                }
                catch (Exception ex)
                {
                    acEd.WriteMessage("{0}\n", ex.Message);
                    Debug.WriteLine(ex.Message);
                }

                acTrans.Commit();

                acEd.WriteMessage("Final score is: {0}", score);
            }

        }

        private List<DBObject> GetHorizontals(Transaction trans)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var layers = trans.GetObject(acDoc.Database.LayerTableId, OpenMode.ForWrite) as LayerTable;

            if (layers == null)
                return null;

            if (!layers.Has("Horizontals"))
                layers.Add(new LayerTableRecord { Name = "Horizontals" });
            var layerId = layers["Horizontals"];
            layers.Close();
            
            var objects = trans.GetAllObjects();
            for (int i = 0; i < objects.Count; i++)
			{
                var line = objects[i] as Line;
                if (line == null)
                    continue;

                if (line.StartPoint.Y == line.EndPoint.Y)
                {
                    line.SetLayerId(layerId, false);
                }
                    
			}

            return null;
        }
    }
}
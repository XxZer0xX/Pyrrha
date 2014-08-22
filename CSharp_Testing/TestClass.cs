#region Referenceing

using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Pyrrha;
using Exception = System.Exception;
using WinForms = System.Windows.Forms;
using Autodesk.AutoCAD.Windows;

#endregion

namespace CSharp_Testing
{
    public class TestClass
    {
       

        [CommandMethod("DocTest")]
        public void DocTest()
        {
            using (var pyDoc = new PyrrhaDocument())
            {
                pyDoc.Editor.WriteMessage("\nLayers:\n");
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
        
            // Disposing isn't working yet.
            // You can test this by running this routine twice.

        }

        [CommandMethod("pyTest")]
        public void PyTest()
        {
            var pyDoc = new PyrrhaDocument();

            pyDoc.Editor.WriteMessage("\nLayers:\n");
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

        [CommandMethod("LineTest")]
        public void LineTest()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acEd = acDoc.Editor;

            var pOptions = new PromptStringOptions("Enter the matrix size of the test: \n")
            {
                AllowSpaces = false,
                DefaultValue = "10",
                UseDefaultValue = true
            };

            var sizePrompt = acEd.GetString(pOptions);
            if (sizePrompt.Status != PromptStatus.OK)
                return;

            var size = Convert.ToInt32(sizePrompt.StringResult);

            CreateBoard(size);
            CalculateScore();

            // COM Zoom? Really?  That's weak Autodesk... Fucking weak.
            var acadObject = Application.AcadApplication;
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

            const double len = 1.0;
            var leg = (0.5 * ((len / 2) * Math.Sqrt(2.0))); // for angular lines
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
                        foreach (var obj in ssAll.GetObjectIds().Select(id => acTrans.GetObject(id, OpenMode.ForWrite)))
                        {
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
                            switch (randPosition)
                            {
                                case 1:
                                    offset = new Vector3d(0, len / 2, 0);// | Vertical
                                    break;
                                case 2:
                                    offset = new Vector3d(len / 2, 0, 0);// -- Horizontal
                                    break;
                                case 3:
                                    offset = new Vector3d(leg, leg, 0);//   / Forward angular
                                    break;
                                default:
                                    offset = new Vector3d(leg, -leg, 0);//  \ Backward angular
                                    break;
                            }

                            var line = new Line(startPoint.Add(offset), startPoint.Subtract(offset))
                            {
                                ColorIndex = rand.Next(1, 6)
                            };

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

                    foreach (var line in (from ObjectId id in mSpace
                                          select acTrans.GetObject(id, OpenMode.ForRead)).OfType<Line>())
                    {
                        // Add line color to score
                        score += line.ColorIndex;

                        line.UpgradeOpen();
                        if (Math.Abs(line.StartPoint.Y - line.EndPoint.Y) < 0.0001)
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

        [CommandMethod("CommandTest")]
        public void CommandTest()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;

            var test = Autodesk.AutoCAD.Internal.PreviousInput.CommandLineMonitorServices.Instance();
            var mon = test.GetCommandLineMonitor(acDoc);
            mon.UnknownCmd += mon_UnknownCmd;

            acDoc.CommandWillStart += acDoc_CommandWillStart;
            acDoc.LispWillStart += acDoc_LispWillStart;
            acDoc.UnknownCommand += acDoc_UnknownCommand;
            Autodesk.AutoCAD.Internal.Windows.CommandThroat.InputCharactersQueued += CommandThroat_InputCharactersQueued;
        }

        void mon_UnknownCmd(object sender, Autodesk.AutoCAD.Internal.PreviousInput.InputStringEventArgs e)
        {
           
        }

        void acDoc_UnknownCommand(object sender, UnknownCommandEventArgs e)
        {
            
        }

        void acDoc_LispWillStart(object sender, LispWillStartEventArgs e)
        {
            
        }

        void acDoc_CommandWillStart(object sender, CommandEventArgs e)
        {
            
        }

        void CommandThroat_InputCharactersQueued(object sender, Autodesk.AutoCAD.Internal.Windows.InputCharactersQueuedEventArgs e)
        {
            if (pythonStarted)
                sb.Append(e.Characters);

            if (e.Characters.Equals("(pyStart)\n", StringComparison.InvariantCultureIgnoreCase))
            {
                pythonStarted = true;
                sb = new System.Text.StringBuilder();
                Application.SetSystemVariable("NOMUTT", 1);
                Application.SetSystemVariable("CMDECHO", 0);
            }
            else if (e.Characters.Equals("(pyEnd)\n", StringComparison.InvariantCultureIgnoreCase))
            {
                pythonStarted = false;
                Application.SetSystemVariable("NOMUTT", 0);
                Application.SetSystemVariable("CMDECHO", 1);
            }
        }

        private static bool pythonStarted;
        private static System.Text.StringBuilder sb;

        [CommandMethod("DialogTest")]
        public void DialogTest()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var dia = new OpenFileDialog("Open python script", "example", "py", "Python",
                                         OpenFileDialog.OpenFileDialogFlags.DefaultIsFolder);

            if (dia.ShowDialog() == WinForms.DialogResult.OK)
                acDoc.Editor.WriteMessage("{0} chosen.", dia.Filename);
        }
    }
}
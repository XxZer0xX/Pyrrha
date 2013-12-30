using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Pyrrha.SelectionFilter;
<<<<<<< HEAD
using Autodesk.AutoCAD.Geometry;

=======
using Pyrrha.Util;
using Document = Pyrrha.Document;
>>>>>>> master

namespace C_Sharp_Testing
{
    public class Class1
    {

        [CommandMethod("tst1")]
        public void test1()
        {
            
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ActiveDocument = new Document(doc);

            //var entities = ActiveDocument.ModelSpaceEntities.Where(ent => ent is Line).ToArray();

            //foreach (Line lineEnt in entities)
            //{
            //    lineEnt.Color = StaticExtenstions.StaticExtenstions.GenerateAutoCadColor(2);
            //}

            //ActiveDocument.ModelSpaceManager.CommitChanges(entities.ToList());

            //var sw = new Stopwatch();
            //sw.Start();
            //var allText = ActiveDocument.AllText;
            //var mtext = ActiveDocument.MText;
            //var dbtext = ActiveDocument.DBText;

            var lineFilter = new LineSelectionFilter(startX: new PointOperation(">", 4));
            var textFilter = new TextSelectionFilter { TextString = "TEXT", ColorIndex = 3 };
            var lines = ActiveDocument.ModelSpaceEntities.ApplyFilter(lineFilter);
            var dbText = ActiveDocument.ModelSpaceEntities.ApplyFilter(textFilter);

            //doc.Editor.WriteMessage("{0} text entities processed: {1}\n"
            //                            , ActiveDocument.AllText.Count
            //                            , sw.ElapsedMilliseconds);

            //sw = new Stopwatch();
            //sw.Start();

            //dbtext.ToList().ForEach(ent => ent.Color = StaticExtenstions.GenerateAutoCadColor(3));
            //ActiveDocument.ModelSpaceManager.CommitChanges(dbtext);

            //sw.Stop();
            //doc.Editor.WriteMessage("{0} DbText entities modified: {1}\n" 
            //                        , dbtext.Count
            //                        , sw.ElapsedMilliseconds);

        }
    }

}

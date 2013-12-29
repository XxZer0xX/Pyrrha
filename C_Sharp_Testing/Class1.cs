using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Pyrrha.SelectionFilter;
using Pyrrha.Util;
using Document = Pyrrha.Document;

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


            var sw = new Stopwatch();
            sw.Start();
            //var allText = ActiveDocument.AllText;
            //var mtext = ActiveDocument.MText;
            var dbtext = ActiveDocument.DBText;

            doc.Editor.WriteMessage("list full: {0}" , sw.ElapsedMilliseconds);
            sw = new Stopwatch();
            sw.Start();

            dbtext.ToList().ForEach(ent => ent.Color = StaticExtenstions.GenerateAutoCadColor(3));
            ActiveDocument.ModelSpaceManager.CommitChanges(dbtext);

            doc.Editor.WriteMessage("list refactor: {0}" , sw.ElapsedMilliseconds);
        }
    }

}

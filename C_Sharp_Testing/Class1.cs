using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Pyrrha.SelectionFilter;
using Autodesk.AutoCAD.Geometry;
using Pyrrha.Util;
using Document = Pyrrha.Document;


namespace C_Sharp_Testing
{
    public class Class1
    {

        [CommandMethod("tst1")]
        public void test1()
        {
            var ActiveDocument = new Document();

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

            var lineFilter = new LineSelectionFilter(startX: new PointQuery(">", 4));
            var textFilter = new TextSelectionFilter { ColorIndex = 3 };
            var lines = ActiveDocument.ModelSpaceEntities.ApplyFilter(lineFilter);
            var dbText = ActiveDocument.ModelSpaceEntities.ApplyFilter(textFilter);
            var blocks = ActiveDocument.Blocks;

            foreach ( var block in blocks )
            {
                var attr =
                    block.AttributeCollection.Cast<AttributeReference>()
                        .FirstOrDefault( attrr => attrr.Tag.Equals( "app#", StringComparison.CurrentCultureIgnoreCase ) );
                if ( attr != null )
                    attr.TextString = "Success!!";
            }
            ActiveDocument.ModelSpaceManager.CommitChanges(blocks);

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

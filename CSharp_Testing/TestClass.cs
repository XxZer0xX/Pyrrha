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

    }
}
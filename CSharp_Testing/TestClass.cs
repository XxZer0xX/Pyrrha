#region Referenceing

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Pyrrha;

#endregion

namespace CSharp_Testing
{
    public class TestClass
    {
        [CommandMethod("tst1")]
        public void Test1()
        {
            var doc = new PyrrhaDocument();

            var layerTest = doc.ObjectManager.GetAllOfType<LayerTableRecord>( doc.LayerTable );
            var blocksTest = doc.ObjectManager.GetAllOfType<BlockReference>( doc.ModelSpace );
            doc.ConfirmAllChanges();
        }
    }
}
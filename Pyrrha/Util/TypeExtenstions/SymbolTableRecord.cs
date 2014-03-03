using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;

namespace Pyrrha.Util.TypeExtenstions
{
    public static class SymbolTableRecordExt
    {
        public static void ColorIndex(this LayerTableRecord layer, short color)
        {
            layer.Color = Color.FromColorIndex( ColorMethod.ByAci , color );
        }

        public static bool ColorEquals(this LayerTableRecord layer, short color)
        {
            return layer.Color == Color.FromColorIndex(ColorMethod.ByAci, color);
        }

        public static void SetLinetype(this LayerTableRecord layer, string linetype)
        {
            var dataBaseType = Assembly.GetAssembly( typeof (Database) ).GetTypes().First(type => type == typeof(Database));
            var tableProp = dataBaseType.GetProperty( "LinetypeTableId", BindingFlags.Public | BindingFlags.Instance );
            using(var lineTable = (LinetypeTable) tableProp.GetValue(tableProp,null))
            {
                if (!lineTable.Has(linetype))
                    throw new KeyNotFoundException( string.Format( "{0} is not defined in the current context." , linetype ));

                layer.LinetypeObjectId = lineTable[linetype];
            }
        }

        //public static bool LinetypeEquals(this LayerTableRecord layer, string linetype)
        //{
            
        //}

        //public static void setLineWeight(this LayerTableRecord layer, string lineWeight)
        //{
            
        //}

        //public static bool LineWeightEquals(this LayerTableRecord layer, string lineweight)
        //{
            
        //}

        //public static void setMaterial(this LayerTableRecord layer, string material)
        //{
            
        //}

        //public static bool MaterialEquals(this LayerTableRecord layer,string material)
        //{
            
        //}
    }
}

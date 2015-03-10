#region Referenceing

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DataExtraction;
using Autodesk.AutoCAD.Internal.Windows;
using Autodesk.AutoCAD.Windows.ToolPalette;

#endregion

namespace PyrrhaIntellisence
{
    public class IntellisenceGenerator
    {
        public IntellisenceGenerator()
        {
            Assemblies = new List<Assembly>
            {
                typeof (ButtonGroup).Assembly,
                typeof (DBObject).Assembly,
                typeof (AdoOutput).Assembly,
                typeof (Application).Assembly,
                typeof (Catalog).Assembly,
                typeof (RolloverColor).Assembly
            };
            Init();
        }

        public IList<Assembly> Assemblies { get; set; }

        public void Init()
        {
            foreach (var assembly in Assemblies)
            {
                var validTypes = assembly.GetTypes().Where(type => type.IsPublic);
                foreach (var type in validTypes)
                {
                    var curDir = type.FullName.Replace('.', '\\');
                    IterateType(type);
                }
            }
        }

        public void WriteToFile(FileStream stream, IEnumerable<MemberInfo> memebers)
        {
        }

        public void IterateType<T>(T type) where T : Type
        {
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance).GroupBy(m => m.MemberType);
        }
    }
}
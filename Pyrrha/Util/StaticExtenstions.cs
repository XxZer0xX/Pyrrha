#region Referencing

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

using Pyrrha.Collections;

#endregion

namespace Pyrrha.Util
{
    public static class StaticExtenstions
    {
        internal const string Pyrrha = "PYRRHA";

        /// <summary>
        ///     Loads a linetype into the database.
        /// </summary>
        //public static void LoadLinetype(this Database database, string value)
        //{
        //    using (var lineTypeTable = (LinetypeTable)database.LinetypeTableId.Open(OpenMode.ForRead))
        //        if (lineTypeTable.Has(value))
        //            return;

        //    database.LoadLineTypeFile(value, "acad.lin");
        //}

        public static string ToString(this SymbolTableRecord record)
        {
            return record.Name;
        }

        public static void SendCommandSynchronously(this Document document,
            string commandString)
        {
            object[] data = { commandString + "\n" };
            var comDocument = document.AcadDocument;
            comDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod,
                null, comDocument, data);
        }

        public static void WriteToActiveDocument(string message)
        {
            if (Application.DocumentManager.MdiActiveDocument != null)
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(message);
        }

        public static Color GenerateAutoCadColor(short colorIndex)
        {
            return Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
        }

        public static AttributeReference GetAttribute(this BlockReference block, string tag)
        {
            return
                block.AttributeCollection.Cast<AttributeReference>()
                    .FirstOrDefault(attr => attr.Tag.Equals(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IDictionary<string, AttributeReference> AttributeDictionary(this BlockReference block)
        {
            return block.AttributeCollection.Cast<AttributeReference>()
                .ToDictionary(attr => attr.Tag, attr => attr);
        }

        public static bool HasAttribute(this BlockReference block, string tag)
        {
            return
                block.AttributeCollection.Cast<AttributeReference>()
                    .Any(attr => attr.Tag.Equals(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action) where T : DBObject
        {
            foreach (var item in enumeration)
                action(item);
        }

        public static IEnumerable<TResult> ForEach<T, TResult>(this IEnumerable enumeration, Func<T, TResult> func) 
            where T : struct
            where TResult : DBObject
        {
            return from object item in enumeration
                   select func( (T) item );
        }

        public static bool IsScriptSource(this Thread thread)
        {
            var stackFrames = new StackTrace(thread, true).GetFrames();
            return stackFrames != null 
                && stackFrames.Any(frame
                => frame.GetMethod()
                        .Name.Equals("ExecutePythonScript",
                                    StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
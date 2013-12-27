using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using IronPython.Hosting;

namespace Pyrrha.Util.Scripting
{
    public class CommandLineLoader
    {
        [CommandMethod("-PYLOAD")]
        public static void PythonLoadCmdLine()
        {
            PythonLoad(true);
        }

        [CommandMethod("PYLOAD")]
        public static void PythonLoadUI()
        {
            PythonLoad(false);
        }


        public static void PythonLoad(bool useCmdLine)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            var fd = (short)Application.GetSystemVariable("FILEDIA");

            // Todo Implement Custom Loader View

            var pfo = new PromptOpenFileOptions(
                  "Select Python script to load"
                )
            {
                Filter = "Python script (*.py)|*.py",
                PreferCommandLine = (useCmdLine || fd == 0)
            };

            var pr = ed.GetFileNameForOpen(pfo);

            if (pr.Status == PromptStatus.OK)
                ExecutePythonScript(pr.StringResult);
        }

        [LispFunction("PYLOAD")]
        public ResultBuffer PythonLoadLisp(ResultBuffer rb)
        {
            const int RTSTR = 5005;

            var doc =
              Application.DocumentManager.MdiActiveDocument;
            if (rb == null)
            {
                doc.Editor.WriteMessage("\nError: too few arguments\n");
                return null;
            }

            var args = rb.AsArray();
            var typedValue = (TypedValue)args.GetValue(0);

            if (typedValue.TypeCode != RTSTR)
                return null;

            bool success =
              ExecutePythonScript(Convert.ToString(typedValue.Value));
            return success ? new ResultBuffer(
                    new TypedValue(RTSTR, typedValue.Value))
                    : null;
        }

        private static bool ExecutePythonScript(string file)
        {
            if (!File.Exists(file)) return false;

            var engine = Python.CreateEngine();
            engine.ExecuteFile(file);
            return true;
        }
    }
}

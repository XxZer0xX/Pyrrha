#region Referencing

using System;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using IronPython.Hosting;
using Pyrrha.Scripting.Compiler;
using Pyrrha.Util;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;

#endregion

namespace Pyrrha.Scripting.AutoCad
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
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            var fd = (short)AcApp.GetSystemVariable("FILEDIA");

            // Todo Implement Custom Loader View

            var pfo = new PromptOpenFileOptions(
                  "Select Python script to load"
                )
            {
                Filter = "Python script (*.py)|*.py" ,
                PreferCommandLine = (useCmdLine || fd == 0)
            };

            var pr = ed.GetFileNameForOpen(pfo);

            if (pr.Status == PromptStatus.OK)
                ExecutePythonScript(pr.StringResult);
        }

        [LispFunction("PYLOAD")]
        public ResultBuffer PythonLoadLisp(ResultBuffer rb)
        {
            const int rtstr = 5005;

            var doc =
              AcApp.DocumentManager.MdiActiveDocument;
            if (rb == null)
            {
                doc.Editor.WriteMessage("\nError: too few arguments\n");
                return null;
            }

            var args = rb.AsArray();
            var typedValue = (TypedValue)args.GetValue(0);

            if (typedValue.TypeCode != rtstr)
                return null;

            var success =
              ExecutePythonScript(Convert.ToString(typedValue.Value));
            return success ? new ResultBuffer(
                    new TypedValue(rtstr , typedValue.Value))
                    : null;
        }

        private static bool ExecutePythonScript(string filePath)
        {
            if (!File.Exists(filePath))
            {
                StaticExtenstions.WriteToActiveDocument(string.Format(
                    "{0} does not exist" , filePath
                    ));
                return false;
            }

            var scriptSource = Python.CreateEngine().CreateScriptSourceFromFile(filePath);
            var errorListener = new ComplieTimeErrorListener();
            var compliedScript = scriptSource.Compile(errorListener);

            if (!errorListener.FoundError)

                try
                {
                    compliedScript.Execute();
                    StaticExtenstions.WriteToActiveDocument(Path.GetFileName(filePath) + " Execution Successful.");
                    return true;

                } catch (Exception e)
                {
                    StaticExtenstions.WriteToActiveDocument(
                        string.Format("\nMessage: {0}\nSource:{1}" , e.Message , e.Source));
                    return false;
                }

            foreach (var error in errorListener.ErrorDataList)

                StaticExtenstions.WriteToActiveDocument(
                    string.Format("{1} Error: {0}" , error.Message , error.Severity)
                    );

            return false;
        }
    }
}

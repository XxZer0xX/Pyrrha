#region Referencing



#endregion

namespace Pyrrha.Loader.AutoCad
{
    #region Referenceing

    using System;
    using System.IO;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Runtime;
    using Engine;
    using PyrrhaAppLoad;

    #endregion

    public class CommandLineLoader
    {
        [CommandMethod("-PYLOAD")]
        public void PythonLoadCmdLine()
        {
            this.PythonLoad(true);
        }

        [CommandMethod("PYLOAD")]
        public void PythonLoadUI()
        {
            this.PythonLoad(false);
        }

        public void PythonLoad(bool useCmdLine)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            var fd = (short) Application.GetSystemVariable("FILEDIA");

            if (useCmdLine)
            {
                var promptStrOptions = new PromptStringOptions("\n Input Python File Path: ");
                var promptStrRes = ed.GetString(promptStrOptions);
            }
            else
            {
                var pyLoad = new PyrrhaAppLoad.App();
            }
            // Todo Implement Custom Loader View

           

            //if (pr.Status == PromptStatus.OK && File.Exists(pr.StringResult))
            //    LoadSciptFromFile(pr.StringResult);
        }

        [LispFunction("PYLOAD")]
        public ResultBuffer PythonLoadLisp(ResultBuffer rb)
        {
            const int rtstr = 5005;

            var doc =
                Application.DocumentManager.MdiActiveDocument;
            if (rb == null)
            {
                doc.Editor.WriteMessage("\nError: too few arguments\n");
                return null;
            }

            var args = rb.AsArray();
            var typedValue = (TypedValue) args.GetValue(0);

            if (typedValue.TypeCode != rtstr)
                return null;

            var filePath = Convert.ToString(typedValue.Value);

            return LoadSciptFromFile(filePath)
                ? new ResultBuffer(new TypedValue(rtstr, typedValue.Value))
                : null;
        }

        private static bool LoadSciptFromFile(string filePath)
        {
            var eng = new PyrrhaEngine();
            eng.CompileAndExecute(eng.CreateScriptSourceFromFile(filePath));
            return true;
        }
    }
}
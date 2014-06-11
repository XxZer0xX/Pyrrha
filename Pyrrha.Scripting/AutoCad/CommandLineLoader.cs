#region Referencing

using System;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Pyrrha.Scripting.Runtime;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

#endregion

namespace Pyrrha.Scripting.AutoCad
{
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
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var editor = doc.Editor;

            var fd = (short)AcApp.GetSystemVariable("FILEDIA");

            // Todo Implement Custom Loader View

            var pfo = new PromptOpenFileOptions(
                  "Select Python script to load"
                )
            {
                Filter = "Python script (*.py)|*.py",
                PreferCommandLine = (useCmdLine || fd == 0)
            };

            var pr = editor.GetFileNameForOpen(pfo);

            if (pr.Status == PromptStatus.OK && File.Exists(pr.StringResult))
                LoadSciptFromFile(pr.StringResult);
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

            var filePath = Convert.ToString(typedValue.Value);


            return LoadSciptFromFile(filePath)
                ? new ResultBuffer(new TypedValue(rtstr, typedValue.Value))
                : null;
        }

        private static bool LoadSciptFromFile(string filePath)
        {
            var session = new PythonSession();
            session.ExecuteScriptFile(filePath);
            return session.EncounterdErrors;
        }
    }
}

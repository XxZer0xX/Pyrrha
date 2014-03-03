#region Referencing

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Pyrrha.Scripting.Compiler;
using Pyrrha.Scripting.Runtime;
using Pyrrha.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;

#endregion

namespace Pyrrha.Scripting.AutoCad
{
    public class CommandLineLoader
    {
        [CommandMethod("-PYLOAD")]
        public void PythonLoadCmdLine()
        {
            PythonLoad(true);
        }

        [CommandMethod("PYLOAD")]
        public void PythonLoadUI()
        {
            PythonLoad(false);
        }

        public void PythonLoad(bool useCmdLine)
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;

            var fd = (short)AcApp.GetSystemVariable("FILEDIA");

            // Todo Implement Custom Loader View

            var pfo = new PromptOpenFileOptions(
                  "Select Python script to load"
                )
            {
                Filter = "Python script (*.py)|*.py",
                PreferCommandLine = (useCmdLine || fd == 0)
            };

            var pr = ed.GetFileNameForOpen(pfo);

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

            return LoadSciptFromFile(Convert.ToString(typedValue.Value))
                ? new ResultBuffer(new TypedValue(rtstr, typedValue.Value))
                : null;
        }

        private Queue<string> _sessionCodeRepo;
        private Queue<string> SessionCodeRepo
        {
            get { return _sessionCodeRepo ?? (_sessionCodeRepo = new Queue<string>()); }
            set { _sessionCodeRepo = value; }
        }

        private ScriptEngine SessionPythonEngine;

        [CommandMethod("pystart")]
        public void StartPythonScripting()
        {
            try
            {
                SessionPythonEngine = PyrrhaHosting.CreateEngine();
                var commandEcho = AcApp.GetSystemVariable("CMDECHO");
                AcApp.SetSystemVariable("CMDECHO", 0);

                string ValidatedCode = string.Empty;
                AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Python Compiler Initialized... Enter Python code.\n");
                while (!ValidatedCode.Equals("pyexecute"))
                {
                    var promptOptions = new PromptStringOptions(">>> ") { AllowSpaces = true };

                    var response = AcApp.DocumentManager.MdiActiveDocument.Editor.GetString(promptOptions);

                    if (string.IsNullOrEmpty(response.StringResult))
                        return;

                    ValidatedCode = LoadedFromCommandLine(response.StringResult);

                    if ( !string.IsNullOrEmpty( ValidatedCode ) )
                        continue;

                    this.DisposeScriptingInstanceDocument();
                    return;
                }

                SessionPythonEngine.Runtime.Shutdown();
                AcApp.SetSystemVariable("CMDECHO", commandEcho);
                CopyCodeToFile_RequestSave();
            }
            catch (Exception e)
            {
                StaticExtenstions.WriteToActiveDocument(
                        string.Format("\nMessage: {0}\nSource:{1}", e.Message, e.Source));
                DisposeScriptingInstanceDocument();
            }
        }

        private void DisposeScriptingInstanceDocument()
        {
            var doc = PyrrhaHosting.InstanceScope.GetVariable("self");
            doc.Dispose();
        }

        private string LoadedFromCommandLine(string code)
        {
            var errorListener = new ComplieTimeErrorListener();
            var compiledcode = SessionPythonEngine.CreateScriptSourceFromString(code).Compile(errorListener);

            if (errorListener.FoundError)
            {
                AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("****___________  Errors thrown  ___________****\n");
                foreach (var error in errorListener.ErrorDataList)
                    StaticExtenstions.WriteToActiveDocument(
                        string.Format("{1} Error: {0}\n", error.Message, error.Severity)
                        );
                AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("****___________  End Errors  ___________****\n");
                return null;
            }

            SessionPythonEngine.Execute(code, PyrrhaHosting.InstanceScope);

            return code;
        }

        internal string tempFilePath = string.Format(@"{0}\local\temp\PyrrhaScriptsTempFolder\{1}_{2}.py", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Environment.UserName, DateTime.Now.ToString().Replace('\\', ('_')));

        private bool CopyCodeToFile_RequestSave()
        {
            var sfd = new SaveFileDialog()
            {
                SupportMultiDottedExtensions = true,
                CreatePrompt = false,
                AddExtension = true,
                DefaultExt = Path.GetFileNameWithoutExtension(tempFilePath) + ".py",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            bool willSave = false;

            if (MessageBox.Show("Would you like to save this script source?", "Save File", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                willSave = true;
                if (sfd.ShowDialog() != DialogResult.OK)
                    willSave = false;
            }

            using (var stream = new FileStream(willSave ? sfd.FileName : tempFilePath, FileMode.Create))
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var line in SessionCodeRepo)
                        writer.WriteLine(line);
                    stream.Flush();
                }
            }

            return LoadSciptFromFile(willSave ? sfd.FileName : tempFilePath);
        }

        private bool LoadedFromIDE(params string[] code)
        {
            throw new NotImplementedException();
        }

        private bool LoadSciptFromFile(string filePath)
        {
            var scriptSource = Python.CreateEngine().CreateScriptSourceFromFile(filePath);
            return proccessPythonScript(scriptSource);
        }

        private bool proccessPythonScript(ScriptSource source)
        {
            // TODO Implement checking and loading the default imports statments
            return complieAndRunScriptSource(source);
        }

        private bool complieAndRunScriptSource(ScriptSource source)
        {
            var errorListener = new ComplieTimeErrorListener();
            var compliedScript = source.Compile(errorListener);

            if (!errorListener.FoundError)
                try
                {
                    compliedScript.Execute();
                    StaticExtenstions.WriteToActiveDocument("Python Execution Successful.");
                    return true;

                }
                catch (Exception e)
                {
                    StaticExtenstions.WriteToActiveDocument(
                        string.Format("\nMessage: {0}\nSource:{1}", e.Message, e.Source));
                    return false;
                }

            foreach (var error in errorListener.ErrorDataList)

                StaticExtenstions.WriteToActiveDocument(
                    string.Format("{1} Error: {0}", error.Message, error.Severity)
                    );

            return false;
        }
    }
}

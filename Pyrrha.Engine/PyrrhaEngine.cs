#region Referenceing

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using AcadExc = Autodesk.AutoCAD.Runtime.Exception;

#endregion

namespace Pyrrha.Engine
{
    #region Referenceing

    

    #endregion

    public sealed class PyrrhaEngine
    {
        private readonly ScriptEngine _engine;

        private ComplieTimeErrorListener _errorListener;

        public PyrrhaEngine()
        {
            _engine = Python.CreateEngine();

            var runtime = _engine.Runtime;
            runtime.LoadAssembly(typeof (Application).Assembly);
            runtime.LoadAssembly(typeof (DBObject).Assembly);
            DefaultScope = _engine.CreateScope();
        }

        public ComplieTimeErrorListener ErrorListener
        {
            get { return _errorListener ?? (_errorListener = new ComplieTimeErrorListener()); }
            private set { _errorListener = value; }
        }

        public ScriptScope DefaultScope { get; private set; }

        public ScriptSource CreateScriptSourceFromFile(string path)
        {
            return _engine.CreateScriptSourceFromFile(path);
        }

        public ScriptSource CreateScriptSourceFromString(string source)
        {
            return _engine.CreateScriptSourceFromString(source);
        }

        public CompiledCode Compile(ScriptSource source)
        {
            return source.Compile(ErrorListener);
        }

        public bool HasCompilationError(ScriptSource source)
        {
            var code = Compile(source);
            return ErrorListener.FoundError;
        }

        public void Execute(CompiledCode code)
        {
            code.Execute(DefaultScope);
        }

        public void CompileAndExecute(ScriptSource source)
        {
            try
            {
                var code = Compile(source);
                if (!ErrorListener.FoundError)
                    code.Execute();
            }
            catch (AcadExc ex)
            {
                writeExceptions(ex);
            }
        }

        private void writeExceptions(AcadExc ex)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                string.Format("{0}: {1}\n\t{2}", ex.ErrorStatus, ex.Message, ex.StackTrace));
        }
    }
}
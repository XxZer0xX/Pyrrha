using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using AcadExc = Autodesk.AutoCAD.Runtime.Exception;

namespace Pyrrha.Engine
{
    using Exception = System.Exception;

    public sealed class PyrrhaEngine
    {
        private readonly ScriptEngine _engine;

        private ComplieTimeErrorListener _errorListener;
        public ComplieTimeErrorListener ErrorListener
        {
            get { return this._errorListener ?? (this._errorListener = new ComplieTimeErrorListener()); }
            private set { this._errorListener = value; }
        }

        public ScriptScope DefaultScope { get; private set; }

        public PyrrhaEngine()
        {
            _engine = Python.CreateEngine();

            var runtime = _engine.Runtime;
            runtime.LoadAssembly(typeof(Application).Assembly);
            runtime.LoadAssembly(typeof(DBObject).Assembly);
            DefaultScope = _engine.CreateScope();  
        }

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
            return source.Compile(this.ErrorListener);
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

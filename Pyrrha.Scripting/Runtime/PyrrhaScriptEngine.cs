#region Referencing

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Pyrrha.Attributes;
using Pyrrha.Scripting.Compiler;

#endregion

namespace Pyrrha.Scripting.Runtime
{
    public class PyrrhaScriptEngine : IDisposable
    {
        private readonly object _commandEcho;
        private readonly ScriptEngine _engine;

        internal readonly PyrrhaDocument LinkedDocument;

        public IDictionary<string, ScriptScope> Scopes { get; private set; }

        public IList<string> AvailableScopes
        {
            get { return this.Scopes.Keys.ToList(); }
        }

        public string CurrentScopeName { get; private set; }

        private ComplieTimeErrorListener _errorListener;
        public ComplieTimeErrorListener ErrorListener
        {
            get { return this._errorListener ?? (this._errorListener = new ComplieTimeErrorListener()); }
            private set { this._errorListener = value; }
        }

        public ScriptScope CurrentScope
        {
            get { return this.Scopes[this.CurrentScopeName]; }
        }

        public PyrrhaScriptEngine()
        {
            this._engine = Python.CreateEngine();

            this.Runtime.LoadAssembly(typeof(Application).Assembly);
            this.Runtime.LoadAssembly(typeof(DBObject).Assembly);

            var initalScope = new Dictionary<string, object> { { "self", this.LinkedDocument = new PyrrhaDocument() } };

            foreach (var obj in this.LinkedDocument.GetType().GetProperties().Where(
             prop => prop.GetCustomAttributes(typeof(ScriptingPropertyAttribute), true).Length != 0))
                initalScope.Add(obj.Name, obj.GetValue(this.LinkedDocument, null));

            foreach (var obj in this.LinkedDocument.GetType().GetMethods().Where(
                            method => method.GetCustomAttributes(typeof(ScriptingMethodAttribute), true).Length != 0))
            {
                Delegate method;

                switch (obj.GetParameters().Count())
                {
                    case 0:
                        method = new Func<dynamic>(() => obj.Invoke(this.LinkedDocument, null));
                        break;
                    case 1:
                        method = new Func<object, dynamic>(param => obj.Invoke(this.LinkedDocument, new[] { param }));
                        break;
                    case 2:
                        method = new Func<object, object, dynamic>((param1, param2) => obj.Invoke(this.LinkedDocument, new[] { param1, param2 }));
                        break;
                    case 3:
                        method = new Func<object, object, object, dynamic>((param1, param2, param3) => obj.Invoke(this.LinkedDocument, new[] { param1, param2, param3 }));
                        break;
                    default:
                        throw new NotImplementedException();
                }

                initalScope.Add(obj.Name.ToLower(), method);
            }

            this.Scopes = new Dictionary<string, ScriptScope> { { this.CurrentScopeName = "initial", this._engine.CreateScope(initalScope) } };

            this._commandEcho = Application.GetSystemVariable("CMDECHO");
            Application.SetSystemVariable("CMDECHO", 0);
            this.LinkedDocument.Editor.WriteMessage("Python Compiler Initialized...\n");
        }

        public CompiledCode Compile(string code)
        {
            return this._engine.CreateScriptSourceFromString(code, SourceCodeKind.AutoDetect).Compile(this.ErrorListener);
        }

        public CompiledCode Compile(ScriptSource source)
        {
            return source.Compile(this.ErrorListener);
        }

        public void SetCurrentScope(string scopeName)
        {
            this.CurrentScopeName = scopeName;
        }

        public dynamic ExecuteInNewScope(string expression, string newScopeName)
        {
            this.Scopes.Add(newScopeName, this._engine.CreateScope());
            return Execute(expression, newScopeName);
        }

        public T ExecuteInNewScope<T>(string expression, string newScopeName)
        {
            this.Scopes.Add(newScopeName, this._engine.CreateScope());
            return Execute<T>(expression, newScopeName);
        }

        public dynamic Execute(CompiledCode code)
        {
            return code.Execute(this.CurrentScope);
        }

        public dynamic Execute(string expression)
        {
            return this._engine.Execute(expression, this.CurrentScope);
        }

        public dynamic Execute(string expression, string scopeName)
        {
            return this.Execute(expression, this.Scopes[scopeName]);
        }

        public dynamic Execute(string expression, ScriptScope scope)
        {
            return this._engine.Execute(expression, scope);
        }

        public T Execute<T>(string expression)
        {
            return this._engine.Execute<T>(expression, this.CurrentScope);
        }

        public T Execute<T>(string expression, string scopeName)
        {
            return this._engine.Execute<T>(expression, this.Scopes[scopeName]);
        }

        public T Execute<T>(string expression, ScriptScope scope)
        {
            return this._engine.Execute<T>(expression, scope);
        }

        public ScriptScope ExecuteFile(string path)
        {
            return this._engine.ExecuteFile(path, this.CurrentScope);
        }

        public ScriptScope ExecuteFile(string path, string scopeName)
        {
            return this._engine.ExecuteFile(path, this.Scopes[scopeName]);
        }

        public ScriptScope ExecuteFile(string path, ScriptScope scope)
        {
            return this._engine.ExecuteFile(path, scope);
        }

        public ScriptScope ExecuteFileInNewScope(string path, string newScopeName)
        {
            this.Scopes.Add(newScopeName, this._engine.CreateScope());
            return this._engine.ExecuteFile(path, this.CurrentScope);
        }

        public ScriptScope GetInstanceScope(string scopeName)
        {
            return this.Scopes[scopeName];
        }

        #region ScriptEngine

        public Version LanguageVersion
        {
            get { return this._engine.LanguageVersion; }
        }

        public ObjectOperations Operations
        {
            get { return this._engine.Operations; }
        }

        public ScriptRuntime Runtime
        {
            get { return this._engine.Runtime; }
        }

        public LanguageSetup Setup
        {
            get { return this._engine.Setup; }
        }

        public ObjectOperations CreateOperations()
        {
            return this._engine.CreateOperations();
        }

        public ObjectOperations CreateOperations(ScriptScope scope)
        {
            return this._engine.CreateOperations(scope);
        }

        public ScriptScope CreateScope()
        {
            return this._engine.CreateScope();
        }

        public ScriptScope CreateScope(IDictionary<string, object> dictionary)
        {
            return this._engine.CreateScope(dictionary);
        }

        public ScriptScope CreateScope(IDynamicMetaObjectProvider storage)
        {
            return this._engine.CreateScope(storage);
        }

        public ScriptSource CreateScriptSource(CodeObject content)
        {
            return this._engine.CreateScriptSource(content);
        }
        public ScriptSource CreateScriptSource(CodeObject content, SourceCodeKind kind)
        {
            return this._engine.CreateScriptSource(content, kind);
        }

        public ScriptSource CreateScriptSource(CodeObject content, string path)
        {
            return this._engine.CreateScriptSource(content, path);
        }

        public ScriptSource CreateScriptSource(StreamContentProvider content, string path)
        {
            return this._engine.CreateScriptSource(content, path);
        }

        public ScriptSource CreateScriptSource(CodeObject content, string path, SourceCodeKind kind)
        {
            return this._engine.CreateScriptSource(content, path, kind);
        }

        public ScriptSource CreateScriptSource(StreamContentProvider content, string path, Encoding encoding)
        {
            return this._engine.CreateScriptSource(content, path, encoding);
        }

        public ScriptSource CreateScriptSource(TextContentProvider contentProvider, string path, SourceCodeKind kind)
        {
            return this._engine.CreateScriptSource(contentProvider, path, kind);
        }

        public ScriptSource CreateScriptSource(StreamContentProvider content, string path, Encoding encoding, SourceCodeKind kind)
        {
            return this._engine.CreateScriptSource(content, path, encoding, kind);
        }

        public ScriptSource CreateScriptSourceFromFile(string path)
        {
            return this._engine.CreateScriptSourceFromFile(path);
        }

        public ScriptSource CreateScriptSourceFromFile(string path, Encoding encoding)
        {
            return this._engine.CreateScriptSourceFromFile(path, encoding);
        }

        public ScriptSource CreateScriptSourceFromFile(string path, Encoding encoding, SourceCodeKind kind)
        {
            return this._engine.CreateScriptSourceFromFile(path, encoding, kind);
        }

        public ScriptSource CreateScriptSourceFromString(string expression)
        {
            return this._engine.CreateScriptSourceFromString(expression);
        }

        public ScriptSource CreateScriptSourceFromString(string code, SourceCodeKind kind)
        {
            return this._engine.CreateScriptSourceFromString(code, kind);
        }

        public ScriptSource CreateScriptSourceFromString(string expression, string path)
        {
            return this._engine.CreateScriptSourceFromString(expression, path);
        }

        public ScriptSource CreateScriptSourceFromString(string code, string path, SourceCodeKind kind)
        {
            return this._engine.CreateScriptSourceFromString(code, path, kind);
        }

        public ObjectHandle ExecuteAndWrap(string expression)
        {
            return this._engine.ExecuteAndWrap(expression);
        }

        public ObjectHandle ExecuteAndWrap(string expression, ScriptScope scope)
        {
            return this._engine.ExecuteAndWrap(expression, scope);
        }

        public CompilerOptions GetCompilerOptions()
        {
            return this._engine.GetCompilerOptions();
        }

        public CompilerOptions GetCompilerOptions(ScriptScope scope)
        {
            return this._engine.GetCompilerOptions(scope);
        }

        public ScriptScope GetScope(string path)
        {
            return this._engine.GetScope(path);
        }

        public ICollection<string> GetSearchPaths()
        {
            return this._engine.GetSearchPaths();
        }

        public TService GetService<TService>(params object[] args) where TService : class
        {
            return this._engine.GetService<TService>(args);
        }

        public object InitializeLifetimeService()
        {
            return this._engine.InitializeLifetimeService();
        }

        public void SetSearchPaths(ICollection<string> paths)
        {
            this._engine.SetSearchPaths(paths);
        }

        #endregion

        #region IDisposable Implementaiton

        private bool _isDisposed;

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || this._isDisposed)
                return;

            foreach (var prop in this.GetType().GetProperties().Where(obj => obj.CanWrite))
                prop.SetValue(this, null, null);

            Application.SetSystemVariable("CMDECHO", this._commandEcho);
            this.LinkedDocument.Dispose();
            this._isDisposed = true;
        }

        #endregion
    }
}

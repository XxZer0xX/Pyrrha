using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;

namespace Pyrrha.Scripting.Runtime
{
    public class PyrrhaScriptingEngine
    {
        private ScriptEngine _engine;

        public IDictionary<string, ScriptScope> Scopes;

        public Version LanguageVersion { get; }
        
        public ObjectOperations Operations { get; }
       
        public ScriptRuntime Runtime { get; }
       
        public LanguageSetup Setup { get; }

        public PyrrhaScriptingEngine(ScriptScope currentScope)
        {

        }

        public ObjectOperations CreateOperations();
       
        public ObjectOperations CreateOperations(ScriptScope scope);

        public ScriptScope CreateScope();

        public ScriptScope CreateScope(IDictionary<string, object> dictionary);
       
        public ScriptScope CreateScope(IDynamicMetaObjectProvider storage);
        
        public ScriptSource CreateScriptSource(CodeObject content);
        
        public ScriptSource CreateScriptSource(CodeObject content, SourceCodeKind kind);
        
        public ScriptSource CreateScriptSource(CodeObject content, string path);
        
        public ScriptSource CreateScriptSource(StreamContentProvider content, string path);
        
        public ScriptSource CreateScriptSource(CodeObject content, string path, SourceCodeKind kind);
        
        public ScriptSource CreateScriptSource(StreamContentProvider content, string path, Encoding encoding);
        
        public ScriptSource CreateScriptSource(TextContentProvider contentProvider, string path, SourceCodeKind kind);
        
        public ScriptSource CreateScriptSource(StreamContentProvider content, string path, Encoding encoding, SourceCodeKind kind);
       
        public ScriptSource CreateScriptSourceFromFile(string path);
        
        public ScriptSource CreateScriptSourceFromFile(string path, Encoding encoding);
        
        public ScriptSource CreateScriptSourceFromFile(string path, Encoding encoding, SourceCodeKind kind);
        
        public ScriptSource CreateScriptSourceFromString(string expression);
        
        public ScriptSource CreateScriptSourceFromString(string code, SourceCodeKind kind);
        
        public ScriptSource CreateScriptSourceFromString(string expression, string path);
        
        public ScriptSource CreateScriptSourceFromString(string code, string path, SourceCodeKind kind);
        
        public T Execute<T>(string expression);
        
        public dynamic Execute(string expression);
       
        public dynamic Execute(string expression, ScriptScope scope);
        
        public T Execute<T>(string expression, ScriptScope scope);

        public dynamic Execute(string expression, string scopeName);

        public T Execute<T>(string expression, string scopeName);

        public ObjectHandle ExecuteAndWrap(string expression);
        
        public ObjectHandle ExecuteAndWrap(string expression, ScriptScope scope);
        
        public ScriptScope ExecuteFile(string path);
       
        public ScriptScope ExecuteFile(string path, ScriptScope scope);

        public CompilerOptions GetCompilerOptions();

        public CompilerOptions GetCompilerOptions(ScriptScope scope);
       
        public ScriptScope GetScope(string path);
       
        public ICollection<string> GetSearchPaths();
       
        public TService GetService<TService>(params object[] args) where TService : class;

        public override object InitializeLifetimeService();
       
        public void SetSearchPaths(ICollection<string> paths);
    }
}

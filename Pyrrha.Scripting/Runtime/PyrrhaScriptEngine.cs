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
        internal readonly PyrrhaDocument LinkedDocument;

        private readonly object _commandEcho;

        private readonly ScriptEngine _engine;

        private ComplieTimeErrorListener _errorListener;

        public PyrrhaScriptEngine()
        {
            _engine = Python.CreateEngine();

            Runtime.LoadAssembly( typeof (Application).Assembly );
            Runtime.LoadAssembly( typeof (DBObject).Assembly );

            var initalScope = new Dictionary<string, object>
            {
                {"self", LinkedDocument = new PyrrhaDocument()}
            };

            foreach ( var obj in LinkedDocument.GetType()
                                               .GetProperties()
                                               .Where(
                                                   prop =>
                                                       prop.GetCustomAttributes(
                                                           typeof (ScriptingPropertyAttribute), true )
                                                           .Length != 0 ) )
                initalScope.Add( obj.Name, obj.GetValue( LinkedDocument, null ) );

            foreach ( var obj in LinkedDocument.GetType()
                                               .GetMethods()
                                               .Where(
                                                   method =>
                                                       method.GetCustomAttributes(
                                                           typeof (ScriptingMethodAttribute), true )
                                                             .Length != 0 ) )
            {
                Delegate method;

                switch (obj.GetParameters()
                           .Count())
                {
                    case 0:
                        method = new Func<dynamic>( () => obj.Invoke( LinkedDocument, null ) );
                        break;
                    case 1:
                        method = new Func<object, dynamic>(
                            param => obj.Invoke(
                                LinkedDocument, new[]
                                {
                                    param
                                } ) );
                        break;
                    case 2:
                        method =
                            new Func<object, object, dynamic>(
                                ( param1, param2 ) => obj.Invoke(
                                    LinkedDocument, new[]
                                    {
                                        param1,
                                        param2
                                    } ) );
                        break;
                    case 3:
                        method =
                            new Func<object, object, object, dynamic>(
                                ( param1, param2, param3 ) => obj.Invoke(
                                    LinkedDocument, new[]
                                    {
                                        param1,
                                        param2,
                                        param3
                                    } ) );
                        break;
                    default:
                        throw new NotImplementedException();
                }

                initalScope.Add( obj.Name.ToLower(), method );
            }

            CurrentScope = _engine.CreateScope( initalScope );

            _commandEcho = Application.GetSystemVariable( "CMDECHO" );
            Application.SetSystemVariable( "CMDECHO", 0 );
            LinkedDocument.Editor.WriteMessage( "Python Compiler Initialized...\n" );
        }


        public ScriptScope CurrentScope{ get; private set; }

        public ComplieTimeErrorListener ErrorListener
        {
            get { return _errorListener ?? ( _errorListener = new ComplieTimeErrorListener() ); }
            private set { _errorListener = value; }
        }

        public CompiledCode Compile( string code )
        {
            return _engine.CreateScriptSourceFromString( code, SourceCodeKind.AutoDetect )
                          .Compile( ErrorListener );
        }

        public CompiledCode Compile( ScriptSource source )
        {
            return source.Compile( ErrorListener );
        }


        public dynamic Execute( CompiledCode code )
        {
            return code.Execute( CurrentScope );
        }

        public dynamic Execute( string expression )
        {
            return _engine.Execute( expression, CurrentScope );
        }

        public dynamic Execute( string expression, string scopeName )
        {
            return Execute(expression, CurrentScope);
        }

        public dynamic Execute( string expression, ScriptScope scope )
        {
            return _engine.Execute( expression, scope );
        }

        public T Execute<T>( string expression )
        {
            return _engine.Execute<T>( expression, CurrentScope );
        }

        public T Execute<T>( string expression, string scopeName )
        {
            return _engine.Execute<T>(expression, CurrentScope);
        }

        public T Execute<T>( string expression, ScriptScope scope )
        {
            return _engine.Execute<T>( expression, scope );
        }

        public ScriptScope ExecuteFile( string path )
        {
            return _engine.ExecuteFile( path, CurrentScope );
        }

        public ScriptScope ExecuteFile( string path, string scopeName )
        {
            return _engine.ExecuteFile(path, CurrentScope);
        }

        public ScriptScope ExecuteFile( string path, ScriptScope scope )
        {
            return _engine.ExecuteFile( path, scope );
        }

        #region ScriptEngine

        public Version LanguageVersion
        {
            get { return _engine.LanguageVersion; }
        }

        public ObjectOperations Operations
        {
            get { return _engine.Operations; }
        }

        public ScriptRuntime Runtime
        {
            get { return _engine.Runtime; }
        }

        public LanguageSetup Setup
        {
            get { return _engine.Setup; }
        }

        public ObjectOperations CreateOperations()
        {
            return _engine.CreateOperations();
        }

        public ObjectOperations CreateOperations( ScriptScope scope )
        {
            return _engine.CreateOperations( scope );
        }

        public ScriptScope CreateScope()
        {
            return _engine.CreateScope();
        }

        public ScriptScope CreateScope( IDictionary<string, object> dictionary )
        {
            return _engine.CreateScope( dictionary );
        }

        public ScriptScope CreateScope( IDynamicMetaObjectProvider storage )
        {
            return _engine.CreateScope( storage );
        }

        public ScriptSource CreateScriptSource( CodeObject content )
        {
            return _engine.CreateScriptSource( content );
        }

        public ScriptSource CreateScriptSource( CodeObject content, SourceCodeKind kind )
        {
            return _engine.CreateScriptSource( content, kind );
        }

        public ScriptSource CreateScriptSource( CodeObject content, string path )
        {
            return _engine.CreateScriptSource( content, path );
        }

        public ScriptSource CreateScriptSource( StreamContentProvider content, string path )
        {
            return _engine.CreateScriptSource( content, path );
        }

        public ScriptSource CreateScriptSource( CodeObject content, string path, SourceCodeKind kind )
        {
            return _engine.CreateScriptSource( content, path, kind );
        }

        public ScriptSource CreateScriptSource( StreamContentProvider content, string path, Encoding encoding )
        {
            return _engine.CreateScriptSource( content, path, encoding );
        }

        public ScriptSource CreateScriptSource( TextContentProvider contentProvider, string path, SourceCodeKind kind )
        {
            return _engine.CreateScriptSource( contentProvider, path, kind );
        }

        public ScriptSource CreateScriptSource(
            StreamContentProvider content,
            string path,
            Encoding encoding,
            SourceCodeKind kind )
        {
            return _engine.CreateScriptSource( content, path, encoding, kind );
        }

        public ScriptSource CreateScriptSourceFromFile( string path )
        {
            return _engine.CreateScriptSourceFromFile( path );
        }

        public ScriptSource CreateScriptSourceFromFile( string path, Encoding encoding )
        {
            return _engine.CreateScriptSourceFromFile( path, encoding );
        }

        public ScriptSource CreateScriptSourceFromFile( string path, Encoding encoding, SourceCodeKind kind )
        {
            return _engine.CreateScriptSourceFromFile( path, encoding, kind );
        }

        public ScriptSource CreateScriptSourceFromString( string expression )
        {
            return _engine.CreateScriptSourceFromString( expression );
        }

        public ScriptSource CreateScriptSourceFromString( string code, SourceCodeKind kind )
        {
            return _engine.CreateScriptSourceFromString( code, kind );
        }

        public ScriptSource CreateScriptSourceFromString( string expression, string path )
        {
            return _engine.CreateScriptSourceFromString( expression, path );
        }

        public ScriptSource CreateScriptSourceFromString( string code, string path, SourceCodeKind kind )
        {
            return _engine.CreateScriptSourceFromString( code, path, kind );
        }

        public ObjectHandle ExecuteAndWrap( string expression )
        {
            return _engine.ExecuteAndWrap( expression );
        }

        public ObjectHandle ExecuteAndWrap( string expression, ScriptScope scope )
        {
            return _engine.ExecuteAndWrap( expression, scope );
        }

        public CompilerOptions GetCompilerOptions()
        {
            return _engine.GetCompilerOptions();
        }

        public CompilerOptions GetCompilerOptions( ScriptScope scope )
        {
            return _engine.GetCompilerOptions( scope );
        }

        public ScriptScope GetScope( string path )
        {
            return _engine.GetScope( path );
        }

        public ICollection<string> GetSearchPaths()
        {
            return _engine.GetSearchPaths();
        }

        public TService GetService<TService>( params object[] args ) where TService : class
        {
            return _engine.GetService<TService>( args );
        }

        public object InitializeLifetimeService()
        {
            return _engine.InitializeLifetimeService();
        }

        public void SetSearchPaths( ICollection<string> paths )
        {
            _engine.SetSearchPaths( paths );
        }

        #endregion

        #region IDisposable Implementaiton

        private bool _isDisposed;

        public void Dispose()
        {
            Dispose( true );
        }

        private void Dispose( bool disposing )
        {
            if (!disposing || _isDisposed)
                return;

            foreach ( var prop in GetType()
                .GetProperties()
                .Where( obj => obj.CanWrite ) )
                prop.SetValue( this, null, null );

            Application.SetSystemVariable( "CMDECHO", _commandEcho );
            LinkedDocument.Dispose();
            _isDisposed = true;
        }

        #endregion
    }
}
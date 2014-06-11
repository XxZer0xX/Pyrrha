#region Referencing

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using IronPython.Runtime.Exceptions;
using Exception = System.Exception;

#endregion

namespace Pyrrha.Scripting.Runtime
{
    public sealed class PythonSession : IDisposable
    {
        internal PyrrhaScriptEngine SessionEngine;

        internal string TempFilePath = string.Format(
            @"{0}\local\temp\PyrrhaScriptsTempFolder\{1}_{2}.py",
            Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ),
            Environment.UserName,
            DateTime.Now.ToString( CultureInfo.InvariantCulture )
                    .Replace( '\\', ( '_' ) ) );

        private Queue<string> _sessionCodeRepo;

        public PythonSession()
        {
            SessionEngine = new PyrrhaScriptEngine();
        }

        private Queue<string> SessionCodeRepo
        {
            get { return _sessionCodeRepo ?? ( _sessionCodeRepo = new Queue<string>() ); }
        }

        private bool _userCanceled { get; set; }

        public bool EncounterdErrors
        {
            get { return SessionEngine.ErrorListener.FoundError; }
        }

        [CommandMethod( "pystart" )]
        public void StartPythonScripting()
        {
            try
            {
                SourceAccumulate();
                if (!EncounterdErrors &&
                    !_userCanceled)
                    CopyCodeToFile_RequestSave();
            } catch ( Exception e )
            {
                SessionEngine.LinkedDocument.Editor.WriteMessage(
                    string.Format( "\nMessage: {0}\nSource:{1}", e.Message, e.Source ) );
            }
            Dispose();
        }

        private void SourceAccumulate()
        {
            var isMultilining = 0;
            var deferedExecutableCodeBuilder = new StringBuilder();

            while (true)
            {
                var promptOptions = new PromptStringOptions(
                    isMultilining.Equals( 0 )
                        ? ">>> "
                        : _generateMLPrompt( isMultilining ) )
                {
                    AllowSpaces = true
                };

                var response = SessionEngine.LinkedDocument.Editor.GetString( promptOptions );

                if (response.Status != PromptStatus.OK)
                {
                    SessionEngine.Dispose();
                    _userCanceled = true;
                    return;
                }

                var stringResult = response.StringResult;

                if (stringResult.Contains( '{' ) ||
                    stringResult.Contains( '}' ))
                {
                    deferedExecutableCodeBuilder.Append( string.Format( " {0}", stringResult ) );

                    foreach ( var character in response.StringResult.Where( c => c.Equals( '{' ) || c.Equals( '}' ) ) )

                        if (character.Equals( '{' ))
                            isMultilining++;
                        else
                            isMultilining--;

                    if (isMultilining < 0)
                        throw new SyntaxWarningException( string.Format( "Missing opening bracket.", stringResult ) );

                    if (isMultilining > 0)
                        continue;

                    stringResult = new Regex( "[{}]" ).Replace( deferedExecutableCodeBuilder.ToString(), " " );
                }

                if (stringResult.Equals( "end", StringComparison.InvariantCultureIgnoreCase ) ||
                    !_execute( stringResult ))
                    return;

                deferedExecutableCodeBuilder = new StringBuilder();
            }
        }

        private string _generateMLPrompt( int iter )
        {
            var stringbuilder = new StringBuilder();
            for ( var i = 0; i < iter; i++ )
                stringbuilder.Append( '{' );
            stringbuilder.Append( "_>" );
            return stringbuilder.ToString();
        }

        private bool _execute( string code )
        {
            var compiledcode = SessionEngine.Compile( code );

            if (SessionEngine.ErrorListener.FoundError)
            {
                ReportErrors();
                return false;
            }

            string scopeKey = null;
            if (code.Contains( "=" ))
                scopeKey = code.Split( '=' )[0].Replace( " ", string.Empty );

            var scopeObj = SessionEngine.Execute( compiledcode );
            SessionCodeRepo.Enqueue( code );

            if (scopeKey != null &&
                scopeObj != null)
                SessionEngine.CurrentScope.SetVariable( scopeKey, scopeObj );
            return true;
        }

        private void CopyCodeToFile_RequestSave()
        {
            var sfd = new SaveFileDialog()
            {
                SupportMultiDottedExtensions = true,
                CreatePrompt = false,
                AddExtension = true,
                DefaultExt = Path.GetFileNameWithoutExtension( TempFilePath ) + ".py",
                InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments )
            };

            var willSave = false;

            if (
                MessageBox.Show(
                    "Would you like to save this script source?", "Save File", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question ) == DialogResult.OK)
            {
                willSave = true;
                if (sfd.ShowDialog() != DialogResult.OK)
                    willSave = false;
            }

            if (willSave)
                TempFilePath = sfd.FileName;

            File.WriteAllLines( TempFilePath, SessionCodeRepo );
        }

        public void ExecuteScriptFile( string path )
        {
            try
            {
                var scriptSource = SessionEngine.CreateScriptSourceFromFile( path );
                var compiledCode = SessionEngine.Compile( scriptSource );

                if (EncounterdErrors)
                {
                    ReportErrors();
                    return;
                }

                SessionEngine.Execute( compiledCode );
            } catch ( Exception e )
            {
                SessionEngine.LinkedDocument.Editor.WriteMessage(
                    string.Format( "\nMessage: {0}\nSource:{1}", e.Message, e.Source ) );
                Dispose();
            }
        }

        private void ReportErrors()
        {
            SessionEngine.LinkedDocument.Editor.WriteMessage( "****___________  Errors thrown  ___________****\n" );
            foreach ( var error in SessionEngine.ErrorListener.ErrorData )
                SessionEngine.LinkedDocument.Editor.WriteMessage(
                    string.Format( "{1} Error: {0}\n", error.Message, error.Severity )
                    );

            SessionEngine.LinkedDocument.Editor.WriteMessage( "****___________  End Errors  ___________****\n" );
        }

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

            SessionEngine.Dispose();
            _isDisposed = true;
        }

        #endregion

        //private bool LoadedFromIDE(params string[] code)
        //{
        //    throw new NotImplementedException();
        //}

        //private bool proccessPythonScript(ScriptSource source)
        //{
        //    // TODO Implement checking and loading the default imports statments
        //    return complieAndRunScriptSource(source);
        //}
    }
}
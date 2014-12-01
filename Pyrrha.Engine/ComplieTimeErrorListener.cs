#region Referenceing

using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

#endregion

namespace Pyrrha.Engine
{
    #region Referenceing

    

    #endregion

    public class ComplieTimeErrorListener : ErrorListener
    {
        public ComplieTimeErrorListener()
        {
            ErrorData = new List<ErrorData>();
        }

        public IList<ErrorData> ErrorData { get; set; }
        public bool FoundError { get; set; }

        public override void ErrorReported(
            ScriptSource source,
            string message,
            SourceSpan span,
            int errorCode,
            Severity severity)
        {
            FoundError = true;

            ErrorData.Add(new ErrorData
            {
                Message = message,
                Span = span,
                ErrorCode = errorCode,
                Severity = severity,
                ErroredCode = GetErroredCode(source, span)
            });
        }

        // TODO
        public string GetErroredCode(ScriptSource source, SourceSpan span)
        {
            return null;
        }
    }
}
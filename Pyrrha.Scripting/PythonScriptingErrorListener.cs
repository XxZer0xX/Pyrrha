using System.Collections.Generic;
using System.Linq;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Pyrrha.Scripting.Util;

namespace Pyrrha.Scripting
{
    public class PythonScriptingErrorListener : ErrorListener
    {
        public IList<ErrorData> ErrorDataList { get; set; }

        public override void ErrorReported(
            ScriptSource source,
            string message,
            SourceSpan span,
            int errorCode,
            Severity severity)
        {
            this.ErrorDataList.Add(new ErrorData
            {
                Message = message,
                Span = span,
                ErrorCode = errorCode,
                Severity = severity,
                ErroredCode = StaticExtensions.GetErroredCode(source,span)
            });
        }

        public PythonScriptingErrorListener()
        {
            this.ErrorDataList = new List<ErrorData>();
        }
    }
}

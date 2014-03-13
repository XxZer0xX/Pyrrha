﻿#region Referencing

using System.Collections.Generic;
using System.Linq;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Pyrrha.Scripting.Util;

#endregion

namespace Pyrrha.Scripting.Compiler
{
    public class ComplieTimeErrorListener : ErrorListener
    {
        public IList<ErrorData> ErrorData { get; set; }
        public bool FoundError { get; set; }

        public override void ErrorReported(
            ScriptSource source,
            string message,
            SourceSpan span,
            int errorCode,
            Severity severity)
        {
            this.FoundError = true;

            this.ErrorData.Add(new ErrorData
            {
                Message = message,
                Span = span,
                ErrorCode = errorCode,
                Severity = severity,
                ErroredCode = StaticExtensions.GetErroredCode(source,span)
            });
        }

        public ComplieTimeErrorListener()
        {
            this.ErrorData = new List<ErrorData>();
        }
    }
}

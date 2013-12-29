using System.Linq;
using Microsoft.Scripting;

namespace Pyrrha.Scripting
{
    public class ErrorData
    {
        public string Message { get; set; }
        public SourceSpan Span { get; set; }
        public int ErrorCode { get; set; }
        public Severity Severity { get; set; }
        public string ErroredCode { get; set; }
    }
}

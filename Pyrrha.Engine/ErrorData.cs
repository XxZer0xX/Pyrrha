#region Referencing

#endregion

#region Referenceing

using Microsoft.Scripting;

#endregion

namespace Pyrrha.Engine
{
    #region Referenceing

    

    #endregion

    public class ErrorData
    {
        public string Message { get; set; }
        public SourceSpan Span { get; set; }
        public int ErrorCode { get; set; }
        public Severity Severity { get; set; }
        public string ErroredCode { get; set; }
    }
}
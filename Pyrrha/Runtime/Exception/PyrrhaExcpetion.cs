#region Referencing

using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;

#endregion

namespace Pyrrha.Runtime.Exception
{
    public abstract class PyrrhaException : System.Exception
    {
        public static bool IsScriptSource;

        new public string Message { get; private set; }

        protected PyrrhaException(string message)
        {
            this.Message = message;
        }

        protected PyrrhaException()
        { 
        }

        public void ThrowException()
        {
            if (!IsScriptSource)
                throw this;
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                string.Format("\nMessage: {0}\nSource:{1}", this.Message, this.Source)
                );
        }
    }
}

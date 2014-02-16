#region Referencing

using System;
using System.Linq;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

#endregion

namespace Pyrrha.Runtime
{
    public abstract class PyrrhaException : Exception
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
            AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                string.Format("\nMessage: {0}\nSource:{1}", this.Message, this.Source)
                );
        }
    }
}

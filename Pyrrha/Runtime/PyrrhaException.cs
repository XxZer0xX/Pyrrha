#region Referencing

using System;
using System.Linq;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

#endregion

namespace Pyrrha.Runtime
{
    public class PyrrhaException : Exception
    {
        new public string Message { get; private set; }

        public PyrrhaException(string message, params object[] args) : base (string.Format(message, args))
        {
            Message = message;
        }

        public PyrrhaException()
        { 
        }
    }
}

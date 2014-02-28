#region Referencing

using System;
using System.Linq;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

#endregion

namespace Pyrrha.Runtime
{
    public class PyrrhaException : Exception
    {
        public PyrrhaException(string message, params object[] args)
            : base (string.Format(message, args))
        {
        }

        public PyrrhaException()
        { 
        }
    }
}

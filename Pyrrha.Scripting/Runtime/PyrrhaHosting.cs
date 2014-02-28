using Autodesk.AutoCAD.ApplicationServices;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pyrrha.Scripting.Runtime
{
    public static class PyrrhaHosting
    {
        internal static ScriptScope InstanceScope { get; set; }

        public static ScriptEngine CreateEngine()
        {
            var engine = Python.CreateEngine();
            engine.Runtime.LoadAssembly(typeof(Autodesk.AutoCAD.DatabaseServices.DBObject).Assembly);
            engine.Runtime.LoadAssembly(typeof(Application).Assembly);
            InstanceScope = engine.CreateScope(new Dictionary<string, object>());
            InstanceScope.SetVariable("self", new PyrrhaDocument());
            return engine;
        }
    }
}

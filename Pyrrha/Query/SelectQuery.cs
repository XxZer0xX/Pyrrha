using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Internal.PropertyInspector;

namespace Pyrrha.Query
{
    public sealed class SelectQuery
    {
        public const string Statement =
            @"SELECT `LINE` AND `CIRCLE` FROM `MODELSPACE` WHERE (COLOR=`4` AND ( RADIUS > 3 OR LENGTH > 3))"; 

        public bool Execute()
        {
            throw new NotImplementedException();
        }

        public bool Execute(string statement)
        {
            throw new NotImplementedException();
        }

        public bool ParameterizedExecute(string statement, params object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}

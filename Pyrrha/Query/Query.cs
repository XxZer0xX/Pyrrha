using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;

namespace Pyrrha.Query
{
    public abstract class Query
    {
        protected internal string Statment { get; set; }

        protected internal Tuple<string,IEnumerable<string>,IEnumerable<string>,IEnumerable<object>>    _searchConditons { get; set; }

        protected internal string _target { get; set; }

        
    }
}

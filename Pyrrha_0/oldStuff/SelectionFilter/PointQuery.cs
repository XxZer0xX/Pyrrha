#region Referenceing

using System;
using System.Diagnostics.Eventing.Reader;
using Pyrrha.Util;

#endregion

namespace Pyrrha.SelectionFilter
{
    public struct PointQuery
    {
        private readonly string _op;
        private readonly double _val;

        public string Operator
        {
            get { return _op; }
        }

        public double PointValue
        {
            get { return _val; }
        }

        public PointQuery( string op, double val )
        {
            if ( op == "*" ||
                 op == "=" ||
                 op == "!=" ||
                 op == "/=" ||
                 op == "<>" ||
                 op == "<" ||
                 op == "<=" ||
                 op == ">" ||
                 op == ">=" )
            {
                _op = op;
                _val = val;
                return;
            }

            #region ErrorHandleing

            if ( !Document.InvokedFromScripting )
                throw new Exception( "Invalid operator string!" );

            StaticExtenstions.WriteToActiveDocument(
                string.Format( "{0} Error: Invalid operator \"{1}\"",
                    StandardEventLevel.Warning,
                    op )
                );
            Environment.Exit(13);

            _op = null;
            _val = default(double);

            #endregion
        }
    }
}
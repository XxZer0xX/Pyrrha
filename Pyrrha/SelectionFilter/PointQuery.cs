using System;

namespace Pyrrha.SelectionFilter
{
    public struct PointQuery
    {
        private readonly string _op;
        private readonly double _val;

        public string Operator { get { return _op; } }
        public double PointValue { get { return _val; } }

        public PointQuery(string op , double val)
        {
            if (op == "*" ||
                op == "=" ||
                op == "!=" ||
                op == "/=" ||
                op == "<>" ||
                op == "<" ||
                op == "<=" ||
                op == ">" ||
                op == ">=")
            {
                this._op = op;
                this._val = val;
            } else
                throw new Exception("Invalid operator string!");
        }

    }
}

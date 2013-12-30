using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pyrrha.SelectionFilter
{
    public struct PointOperation
    {
        private string op;
        private double val;

        public string Operator { get { return op; } }
        public double PointValue { get { return val; } }

        public PointOperation(string op, double val)
        {
            if (op != "*" &&
                op != "=" &&
                op != "!=" &&
                op != "/=" &&
                op != "<>" &&
                op != "<" &&
                op != "<=" &&
                op != ">" &&
                op != ">=")
                throw new Exception("Invalid operator string!");

            this.op = op;
            this.val = val;
        }

    }
}

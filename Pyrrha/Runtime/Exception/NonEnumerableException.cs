#region Referencing

using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

#endregion

namespace Pyrrha.Runtime.Exception
{
    public class NonEnumerableException : PyrrhaException
    {
        public NonEnumerableException(DBObject obj)
            : base(string.Format("{0} is not enumerable", obj.GetType()))
        {
        }
    }
}

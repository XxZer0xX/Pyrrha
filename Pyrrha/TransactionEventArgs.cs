using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace Pyrrha
{
    public class TransactionEventArgs
    {
        public ObjectId ObjectId {get; private set;}

        public TransactionEventArgs(ObjectId id)
        {
            ObjectId = id;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.DatabaseServices;

namespace Pyrrha
{
    class PyrrhaTransaction : OpenCloseTransaction
    {

        public PyrrhaTransaction()
        {
            
        }
        public override void Commit()
        {
            // Check objects
            base.Commit();
        }

        public override DBObject GetObject(ObjectId id, OpenMode mode)
        {
            // Check if 

            return base.GetObject(id, mode);
        }
    }
}

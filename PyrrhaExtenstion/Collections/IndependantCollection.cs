using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace PyrrhaExtenstion.Collections
{
    public class IndependantCollection<T>
        : DbObjectCollection<T> where T : DBObject
    {
        private IList<Transaction> transList;

        public override void Commit()
        {
            throw new NotImplementedException();
        }
    }
    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace PyrrhaExtenstion.Collections
{
    public class OpenCollection<T> : DbObjectCollection<T> where T : DBObject
    {
        public OpenCollection(params ObjectId[] objectIds)
            : this(objectIds.Select(objid => objid.Open(OpenMode.ForWrite)).ToArray()) { }

        public OpenCollection(params DBObject[] objects)
            : base((IEnumerable<T>) objects) { }

        [ Obsolete( "Not needed for this collection type." ) ]
        public override void Commit() { }
    }
}

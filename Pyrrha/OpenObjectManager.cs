#region Referenceing

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

#endregion

namespace Pyrrha
{
    public class OpenObjectManager<TStored> : IEqualityComparer<TStored>, IEnumerable<TStored> 
        where TStored : DBObject
    {
        private readonly IDictionary<ObjectId , TStored> _openObjects;
        private readonly OpenCloseTransaction _transaction;
        private readonly PyrrhaDocument _doc;

        #region Properties

        public int Count
        {
            get { return _openObjects.Count; }
        }

        #endregion

        #region Constructor

        public OpenObjectManager( TransactionManager manager, PyrrhaDocument _doc )
        {
            _transaction = new OpenCloseTransaction();
            this._doc = _doc;
            _openObjects = new Dictionary<ObjectId , TStored>();
        }

        #endregion

        #region Methods

        public TDesired GetObject<TDesired>(ObjectId id) where TDesired : DBObject
        {
            return (TDesired)(_openObjects.ContainsKey(id)
                ? _openObjects[id]
                : _getAddObject( id ));
        }

        public IEnumerable<TDesired> GetAllOfType<TDesired>() where TDesired : DBObject
        {
            if (typeof(TDesired) == typeof(LayerTableRecord))
                return _doc.LayerTable.Cast<ObjectId>().Select(GetObject<TDesired>);

            if (typeof(TDesired) == typeof(LinetypeTableRecord))
                return _doc.LinetypeTable.Cast<ObjectId>().Select(GetObject<TDesired>);

            if (typeof(TDesired) == typeof(TextStyleTableRecord))
                return _doc.TextStyleTable.Cast<ObjectId>().Select(GetObject<TDesired>);

            var fromObjCollection = _openObjects.Values.Where(obj => obj.GetType() == typeof(TDesired))
                .Cast<TDesired>();

            var modelSpace = GetObject<BlockTableRecord>(SymbolUtilityServices.GetBlockModelSpaceId(_doc.Database));
            
            var idsOfType = new List<ObjectId>();
            IList<ObjectId> idsFromDb =
                modelSpace.Cast<ObjectId>().Where(id => !_openObjects.ContainsKey(id)).ToList();

            for ( int i = idsFromDb.Count() ; i >= 0; i-- )
            {
                using ( var dbObject = _transaction.GetObject(idsFromDb[i], OpenMode.ForRead ) )
                    if (dbObject.GetType() == typeof(TDesired))
                        idsOfType.Add(idsFromDb[i]);
            }

            return idsFromDb.Count == 0
                ? fromObjCollection
                : !fromObjCollection.Any() 
                ? idsOfType.Select(GetObject<TDesired>) 
                : idsOfType.Select(GetObject<TDesired>).Union(fromObjCollection);
        }

        public void ConfirmAllChanges()
        {
            _transaction.Commit();
            _transaction.Dispose();
        }

        internal void Remove( ObjectId id )
        {
            var obj = _openObjects[id];
            if ( obj == null ) return;
            obj.Dispose();
            _openObjects.Remove( id );
        }

        private DBObject _getAddObject( ObjectId id )
        {
            DBObject obj = _transaction.GetObject(id, OpenMode.ForWrite );
            _openObjects.Add(id , (TStored) obj);
            return obj;
        }

        #endregion

        #region IEnumerable Implementation

        public IEnumerator<TStored> GetEnumerator()
        {
            return _openObjects.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEqualityComparer Implementation

        public bool Equals(TStored x , TStored y)
        {
            return x.ObjectId.Equals( y.ObjectId );
        }

        public int GetHashCode(TStored obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}
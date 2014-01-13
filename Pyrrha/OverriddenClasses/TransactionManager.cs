#region Referenceing

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

#endregion

namespace Pyrrha.OverriddenClasses
{
    [Wrapper( "Autodesk.AutoCAD.DatabaseServices.TransactionManager" )]
    public class TransactionManager : Autodesk.AutoCAD.DatabaseServices.TransactionManager,
        IEnumerable<Transaction>
    {
        /// <summary>
        ///     Calls - this.AddNewlyCreatedDBObject(obj , true)
        ///     Adds DBObject to the Database and ModelSpace.
        /// </summary>
        /// <param name="obj"></param>
        public void AddNewlyCreatedDBObject( DBObject obj )
        {
            AddNewlyCreatedDBObject( obj, true );
        }

        /// <summary>
        ///     Adds DBObject to the Database and ModelSpace.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="add"></param>
        public override void AddNewlyCreatedDBObject( DBObject obj, bool add )
        {
            using ( var trans = new OpenCloseTransaction() )
            {
                trans.AddNewlyCreatedDBObject( obj, add );
                ObjectId modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId( obj.Database );
                using ( DBObject modelSpace = trans.GetObject( modelSpaceId, OpenMode.ForWrite ) )
                    ( (BlockTableRecord) modelSpace ).AppendEntity( (Entity) obj );
                trans.Commit();
            }
        }

        /// <returns> NULL </returns>
        [Obsolete( "Use Transaction.GetObject()", false )]
        public override DBObject GetObject( ObjectId id, OpenMode mode )
        {
            return null;
        }

        /// <returns> NULL </returns>
        [Obsolete( "Use Transaction.GetObject()", false )]
        public override DBObject GetObject( ObjectId id, OpenMode mode, bool openErased )
        {
            return null;
        }

        /// <returns> NULL </returns>
        [Obsolete( "Use Transaction.GetObject()", false )]
        public override DBObject GetObject( ObjectId id, OpenMode mode, bool openErased, bool forceOpenOnLockedLayer )
        {
            return null;
        }

        /// <summary>
        ///     Useable - calls this.StartTransaction();
        /// </summary>
        /// <returns></returns>
        [Obsolete( "Use StartTransaction()", false )]
        public new Transaction StartOpenCloseTransaction()
        {
            return StartTransaction();
        }

        /// <summary>
        ///     Starts and loggs new Transaction
        /// </summary>
        /// <returns> Transaction </returns>
        public new Transaction StartTransaction()
        {
            var trans = new Transaction( this );
            _transactions.Add( trans );
            return trans;
        }

        #region Constructor

        protected internal TransactionManager( Autodesk.AutoCAD.DatabaseServices.TransactionManager manager )
            : base( manager.UnmanagedObject, false )
        {
            _transIds = new List<TransId>();
            _transactions = new List<Transaction>();
        }

        #endregion

        #region IEnumerable Implementation

        private readonly IList<TransId> _transIds;
        private readonly IList<Transaction> _transactions;

        public Transaction this[ TransId id ]
        {
            get { return _transactions[_transIds.IndexOf( id )]; }
        }

        public IEnumerator<Transaction> GetEnumerator()
        {
            return _transactions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected internal void AddToTransactionList( TransId id, Transaction trans )
        {
            _transIds.Add( id );
            _transactions.Add( trans );
        }

        protected internal DBObject HasOpenObject( ObjectId id )
        {
            return
                _transactions.SelectMany( trans => trans.OpenObjects )
                    .FirstOrDefault( obj => obj.ObjectId.Equals( id ) );
        }

        protected internal Boolean HasMultipleInstancesOfObjectOnCommit( Transaction trans, ObjectId id )
        {
            return _transactions.Count( listTrans
                => listTrans.TransactionId != trans.TransactionId
                   && listTrans.OpenObjectsIds.Contains( id ) ) > 1;
        }

        #endregion
    }
}
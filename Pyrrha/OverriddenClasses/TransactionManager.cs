//#region Referenceing

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Runtime;

//#endregion

//namespace Pyrrha.OverriddenClasses
//{
//    [Wrapper("Autodesk.AutoCAD.DatabaseServices.TransactionManager")]
//    internal sealed class TransactionManager : Autodesk.AutoCAD.DatabaseServices.TransactionManager ,
//        IEnumerable<Transaction>
//    {
//        public override Autodesk.AutoCAD.DatabaseServices.Transaction TopTransaction
//        {
//            get { return _transactions[0]; }
//        }

//        /// <summary>
//        ///     Calls - this.AddNewlyCreatedDBObject(obj , true)
//        ///     Adds DBObject to the Database and ModelSpace.
//        /// </summary>
//        /// <param name="obj"></param>
//        public void AddNewlyCreatedDBObject(DBObject obj)
//        {
//            AddNewlyCreatedDBObject(obj , true);
//        }

//        /// <summary>
//        ///     Adds DBObject to the Database and ModelSpace.
//        /// </summary>
//        /// <param name="obj"></param>
//        /// <param name="add"></param>
//        public override void AddNewlyCreatedDBObject(DBObject obj , bool add)
//        {
//            using (var trans = new OpenCloseTransaction())
//            {
//                trans.AddNewlyCreatedDBObject(obj , add);
//                ObjectId modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(obj.Database);
//                using (DBObject modelSpace = trans.GetObject(modelSpaceId , OpenMode.ForWrite))
//                    ((BlockTableRecord)modelSpace).AppendEntity((Entity)obj);
//                trans.Commit();
//            }
//        }

//        /// <returns> NULL </returns>
//        [Obsolete("Use Transaction.GetObject()" , false)]
//        public override DBObject GetObject(ObjectId id , OpenMode mode)
//        {
//            return null;
//        }

//        /// <returns> NULL </returns>
//        [Obsolete("Use Transaction.GetObject()" , false)]
//        public override DBObject GetObject(ObjectId id , OpenMode mode , bool openErased)
//        {
//            return null;
//        }

//        /// <returns> NULL </returns>
//        [Obsolete("Use Transaction.GetObject()" , false)]
//        public override DBObject GetObject(ObjectId id , OpenMode mode , bool openErased , bool forceOpenOnLockedLayer)
//        {
//            return null;
//        }

//        /// <summary>
//        ///     Useable - calls this.StartTransaction();
//        /// </summary>
//        /// <returns></returns>
//        [Obsolete("Use StartTransaction()" , false)]
//        public new Transaction StartOpenCloseTransaction()
//        {
//            return StartTransaction();
//        }

//        /// <summary>
//        ///     Starts and loggs new Transaction
//        /// </summary>
//        /// <returns> Transaction </returns>
//        public new Transaction StartTransaction()
//        {
//            return StartTransaction( false );
//        }

//        /// <summary>
//        ///     Starts and loggs new Transaction
//        /// </summary>
//        /// <returns> Transaction </returns>
//        public Transaction StartTransaction(bool keepOpen)
//        {
//            return new Transaction(this);
//        }

//        internal bool HasOpenObject(ObjectId id)
//        {
//            return GetAllObjects().Cast<DBObject>().Any( obj => obj.ObjectId.Equals( id ) );
//            //    _transactions.Any(trans => trans.OpenObjects.Any(obj => obj.ObjectId.Equals(id)));
//        }

//        internal DBObject GetOpenObject(ObjectId id)
//        {
//            return GetAllObjects().Cast<DBObject>().FirstOrDefault( obj => obj.ObjectId.Equals( id ) );
//            //return
//            //    this._transactions.SelectMany( trans => trans.OpenObjects ).FirstOrDefault(
//            //        obj => obj.ObjectId.Equals( id ) );
//        }

//        internal void CommitAllTransactions()
//        {
//            foreach (var trans in _transactions)
//                trans.Commit();
//        }

//        #region Constructor

//        protected internal TransactionManager(Autodesk.AutoCAD.DatabaseServices.TransactionManager manager)
//            : base(manager.UnmanagedObject , false)
//        {
//            _transIds = new List<TransId>();
//            _transactions = new List<Transaction>();
//        }

//        #endregion

//        #region IEnumerable Implementation

//        private readonly IList<TransId> _transIds;
//        private readonly IList<Transaction> _transactions;
//        private Autodesk.AutoCAD.DatabaseServices.Transaction _topTransaction;

//        public Transaction this[TransId id]
//        {
//            get { return _transactions[_transIds.IndexOf(id)]; }
//        }

//        public IEnumerator<Transaction> GetEnumerator()
//        {
//            return _transactions.GetEnumerator();
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        internal void AddToTransactionList(TransId id , Transaction trans)
//        {
//            _transIds.Add(id);
//            _transactions.Add(trans);
//        }

//        #endregion
//    }
//}
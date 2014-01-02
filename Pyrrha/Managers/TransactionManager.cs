using Autodesk.AutoCAD.DatabaseServices;

namespace Pyrrha.Managers
{
    internal static class TransactionManager
    {
        internal static Transaction MasterTransaction;
        internal static bool IsActive = MasterTransaction != null;
        internal static int CommitCount;
        internal static bool CommitOnClose;

        public static void Commit()
        {
            if ( IsActive )
            {
                MasterTransaction.Commit();
            }           
        }
    }
}

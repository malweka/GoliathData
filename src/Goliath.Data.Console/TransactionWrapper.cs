using System.Data.Common;

namespace Goliath.Data.CodeGen
{
    class TransactionWrapper : ITransaction
    {
        DbTransaction transaction;

        public TransactionWrapper(DbTransaction transaction)
        {
            this.transaction = transaction;
        }
        #region ITransaction Members

        public DbTransaction InnerTransaction => transaction;
        public bool IsStarted { get; private set; }
        public bool WasCommitted { get; private set; }
        public bool WasRolledBack { get; private set; }

        public void Begin()
        {
            
        }

        public void Begin(System.Data.IsolationLevel isolatedLevel)
        {
            //throw new NotImplementedException();
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public void Rollback()
        {
            transaction.Rollback();
        }

        public void Enlist(System.Data.IDbCommand command)
        {
            command.Transaction = transaction;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            transaction.Dispose();
        }

        #endregion
    }
}

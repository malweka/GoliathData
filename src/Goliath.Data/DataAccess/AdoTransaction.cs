
namespace Goliath.Data.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data;
    using System.Data.Common;
    using Diagnostics;

    [Serializable]
    class AdoTransaction : ITransaction
    {

        IDbTransaction transaction;
        ISession session;
        static ILogger logger;

        static AdoTransaction()
        {
            logger = Logger.GetLogger(typeof(AdoTransaction));
        }

        public AdoTransaction(ISession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            this.session = session;
        }

        #region ITransaction Members

        public bool IsStarted { get; private set; }

        public bool WasCommitted { get; private set; }

        public bool WasRolledBack { get; private set; }

        public void Begin()
        {
            Begin(IsolationLevel.Unspecified);
        }

        public void Begin(System.Data.IsolationLevel isolatedLevel)
        {
            try
            {
                if (isolatedLevel == IsolationLevel.Unspecified)
                    transaction = session.Connection.BeginTransaction();
                else
                    transaction = session.Connection.BeginTransaction(isolatedLevel);
            }
            catch (Exception ex)
            {
                logger.Log(session.Id, "could not begin session", ex);
                throw new DataAccessException("could not begin session", ex);
            }

            IsStarted = true;
        }

        public void Commit()
        {
            if (!IsStarted)
            {
                throw new DataAccessException("Transaction not started");
            }

            try
            {
                transaction.Commit();
                WasCommitted = true;
                WasRolledBack = false;
            }
            catch (Exception ex)
            {
                logger.Log(session.Id, "Commit failed", ex);
                throw new DataAccessException("Commit failed", ex);
            }

        }

        public void Rollback()
        {
            if (!IsStarted)
            {
                throw new DataAccessException("Transaction not started");
            }

            try
            {
                transaction.Rollback();
                WasCommitted = false;
                WasRolledBack = true;
            }
            catch (Exception ex)
            {
                logger.Log(session.Id, "Rollback failed", ex);
                throw new DataAccessException("Rollback failed", ex);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion
    }
}

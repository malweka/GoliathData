using System;
using System.Data;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.DataAccess
{
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

        /// <summary>
        /// begin transaction with isolation level READ COMMiTTED
        /// </summary>
        public void Begin()
        {
            Begin(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Begins the specified isolated level.
        /// </summary>
        /// <param name="isolatedLevel">The isolated level.</param>
        public void Begin(System.Data.IsolationLevel isolatedLevel)
        {
            try
            {
                var currConnection = session.ConnectionManager.CurrentConnection;

                if (currConnection.State != ConnectionState.Open)
                    currConnection.Open();
                
                if (isolatedLevel == IsolationLevel.Unspecified)
                    transaction = currConnection.BeginTransaction();
                else
                    transaction = currConnection.BeginTransaction(isolatedLevel);
            }
            catch (Exception ex)
            {
                logger.LogException(session.Id, "could not begin session", ex);
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
                logger.LogException(session.Id, "Commit failed", ex);
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
                logger.LogException(session.Id, "Rollback failed", ex);
                throw new DataAccessException("Rollback failed", ex);
            }
        }

        public void Enlist(IDbCommand command)
        {
            if (!IsStarted)
            {
                throw new DataAccessException("Transaction not started");
            }

            command.Transaction = transaction;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (transaction != null)
            {
                transaction.Dispose();
            }
        }

        #endregion
    }
}

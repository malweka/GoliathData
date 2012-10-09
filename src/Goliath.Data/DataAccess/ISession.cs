using Goliath.Data.DataAccess;

namespace Goliath.Data
{
    

    /// <summary>
    /// 
    /// </summary>
    public interface ISession : ISqlInterface
    {
        #region Properties 

        /// <summary>
        /// Gets the session factory.
        /// </summary>
        /// <value>The session factory.</value>
        ISessionFactory SessionFactory { get; }

        /// <summary>
        /// Gets the connection manager.
        /// </summary>
        /// <value>The connection manager.</value>
        ConnectionManager ConnectionManager { get; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        string Id { get; }

        ///// <summary>
        ///// Gets the connection.
        ///// </summary>
        ///// <value>The connection.</value>
        //DbConnection Connection { get; }

        /// <summary>
        /// Gets the data access.
        /// </summary>
        /// <value>The data access.</value>
        IDbAccess DataAccess { get; }


        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        /// <value>The current transaction.</value>
        ITransaction CurrentTransaction { get; }

        #endregion

        #region Data Access


        /// <summary>
        /// Creates the data access adapter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDataAccessAdapter<T> CreateDataAccessAdapter<T>();

        #endregion

        #region Transactions

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <returns></returns>
        ITransaction BeginTransaction();

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <param name="isolationLevel">The isolation level.</param>
        /// <returns></returns>
        ITransaction BeginTransaction(System.Data.IsolationLevel isolationLevel);

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <returns></returns>
        ITransaction CommitTransaction();

        /// <summary>
        /// Rollbacks the transaction.
        /// </summary>
        /// <returns></returns>
        ITransaction RollbackTransaction();

        #endregion
    }
}

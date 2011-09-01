

namespace Goliath.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data.Common;
    using DataAccess;
    using Providers;

    /// <summary>
    /// 
    /// </summary>
    public interface ISession
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
        /// create a query object 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQuery<T> Query<T>();

        /// <summary>
        /// Creates the data access adapter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDataAccessAdapter<T> CreateDataAccessAdapter<T>();

        #endregion

        #region Transactions

        ITransaction BeginTransaction();

        ITransaction BeginTransaction(System.Data.IsolationLevel isolationLevel);

        ITransaction CommitTransaction();

        ITransaction RollbackTransaction();

        #endregion
    }
}

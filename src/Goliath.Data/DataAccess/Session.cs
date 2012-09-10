using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace Goliath.Data.DataAccess
{
    using Diagnostics;
    using Sql;

    [Serializable]
    class Session : ISession
    {
        static ILogger logger;
        string id;
        ITransaction currentTransaction;
        public ConnectionManager ConnectionManager { get; private set; }
        //IConnectionProvider connectionProvider;

        #region .Ctor

        static Session()
        {
            logger = Logger.GetLogger(typeof(Session));
        }

        public Session(ISessionFactory sessionFactory, IConnectionProvider connectionProvider)
        {
            id = Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower();
            SessionFactory = sessionFactory;
            ConnectionManager = new ConnectionManager(connectionProvider, !sessionFactory.DbSettings.Connector.AllowMultipleConnections);
        }

        void DisposeOfConnection(IDbConnection connection)
        {
            if (connection.State == ConnectionState.Broken)
                connection.Close();

            connection.Dispose();
        }

        #endregion

        #region Properties

        public string Id
        {
            get { return id; }
        }

        public IDbAccess DataAccess
        {
            get { return SessionFactory.DbSettings.DbAccess; }
        }

        public ITransaction CurrentTransaction
        {
            get
            {
                //if (currentTransaction == null)
                //    currentTransaction = new AdoTransaction(this);
                return currentTransaction;
            }
        }
        #endregion

        #region Data Access

        public IQueryBuilder<T> SelectAll<T>()
        {
            QueryBuilder<T> queryBuilder = new QueryBuilder<T>(this);
            return queryBuilder;
        }

        public ITableNameBuilder SelectAll()
        {
            List<string> columnNames = new List<string>();
            return new QueryBuilder(this, columnNames);
        }

        public IQueryBuilder<T> Select<T>(string propertyName, params string[] propertyNames)
        {
            throw new NotImplementedException();
        }

        public ITableNameBuilder Select(string column, params string[] columns)
        {
            List<string> columnNames = new List<string>();
            columnNames.Add(column);
            if ((columns != null) && (columns.Length > 0))
            {
                columnNames.AddRange(columns);
            }

            return new QueryBuilder(this, columnNames);

        }

        public IDataAccessAdapter<T> CreateDataAccessAdapter<T>()
        {
            var adapterFactory = SessionFactory.AdapterFactory;
            return adapterFactory.Create<T>(SessionFactory.DbSettings.DbAccess, this);
        }

        public int Update<T>(T entity)
        {
            var adapter = CreateDataAccessAdapter<T>();
            return adapter.Update(entity);
        }

        public int Insert<T>(T entity, bool recursive = false)
        {
            var adapter = CreateDataAccessAdapter<T>();
            return adapter.Insert(entity, recursive);
        }

        public int Delete<T>(T entity, bool cascade = false)
        {
            var adapter = CreateDataAccessAdapter<T>();
            return adapter.Delete(entity, cascade);
        }

        public ISessionFactory SessionFactory { get; private set; }

        #endregion

        #region Transactions

        public ITransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.Unspecified);
        }

        public ITransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            if ((currentTransaction == null) || (!currentTransaction.IsStarted))
            {
                currentTransaction = new AdoTransaction(this);
            }
            else if (currentTransaction.IsStarted)
            {
                //TODO: throw exception here?
            }

            currentTransaction.Begin(isolationLevel);
            //TODO: set transaction lock on connection manager??
            return currentTransaction;
        }

        public ITransaction CommitTransaction()
        {
            if ((currentTransaction == null) || (!currentTransaction.IsStarted))
                throw new GoliathDataException("no transaction or not started yet");

            currentTransaction.Commit();
            currentTransaction.Dispose();

            ITransaction transRef = currentTransaction;
            currentTransaction = null;
            return transRef;
        }

        public ITransaction RollbackTransaction()
        {
            if ((currentTransaction == null) || (!currentTransaction.IsStarted))
                throw new GoliathDataException("no transaction or not started yet");

            currentTransaction.Rollback();
            currentTransaction.Dispose();

            ITransaction transRef = currentTransaction;
            currentTransaction = null;
            return transRef;
        }

        #endregion

    }
}

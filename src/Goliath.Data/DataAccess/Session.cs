using System;
using System.Data;

namespace Goliath.Data.DataAccess
{
    using Diagnostics;

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
            //if (connection != null)
            //{
            //    this.connection = connection;
            //    weOwnConnection = false;
            //}
            //this.adapterFactory = adapterFactory;
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

        public IQuery<T> Query<T>()
        {
            throw new NotImplementedException();
        }

        public IDataAccessAdapter<T> CreateDataAccessAdapter<T>()
        {
            var adapterFactory = SessionFactory.AdapterFactory;
            return adapterFactory.Create<T>(SessionFactory.DbSettings.DbAccess, this);
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
                //TODO: throw exception here
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

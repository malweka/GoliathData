
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
    class SessionImpl : ISession
    {
        static ILogger logger;
        string id;
        DbConnection connection;
        ITransaction currentTransaction;
        public ConnectionManager ConnectionManager { get; private set; }

        static SessionImpl()
        {
            logger = Logger.GetLogger(typeof(SessionImpl));
        }

        public SessionImpl(ISessionFactory sessionFactory, IConnectionProvider connectionProvider)
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

        #region ISession Members

        public string Id
        {
            get { return id; }
        }

        public System.Data.Common.DbConnection Connection
        {
            get
            {
                //if (!weOwnConnection)
                //    return connection;

                //if (connection == null)
                //    connection = dbAccess.CreateNewConnection();

                //else if ((connection.State == ConnectionState.Broken) || (connection.State == ConnectionState.Closed))
                //{
                //    DisposeOfConnection(connection);
                //    connection = dbAccess.CreateNewConnection();
                //}

                return connection;
            }
        }

        public IDbAccess DataAccess
        {
            get { return SessionFactory.DbSettings.DbAccess; }
        }

        public ITransaction Transaction
        {
            get
            {
                if (currentTransaction == null)
                    currentTransaction = new AdoTransaction(this);

                return currentTransaction;
            }
        }

        public IQuery<T> Query<T>()
        {
            throw new NotImplementedException();
        }

        public IDataAccessAdapter<T> CreateDataAccessAdapter<T>()
        {
            var adapterFactory = SessionFactory.AdapterFactory;
            return adapterFactory.Create<T>(SessionFactory.DbSettings.DbAccess, connection);
        }

        public ISessionFactory SessionFactory { get; private set; }

        #endregion
    }
}

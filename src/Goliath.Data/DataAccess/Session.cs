
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
    class Session : ISession
    {
        static ILogger logger;
        string id;
        DbConnection connection;
        ITransaction currentTransaction;
        public ConnectionManager ConnectionManager { get; private set; }
        IConnectionProvider connectionProvider;

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

        #region ISession Members

        public string Id
        {
            get { return id; }
        }

        //public System.Data.Common.DbConnection Connection
        //{
        //    get
        //    {
        //        if (connection == null)
        //        {
        //            connection = ConnectionManager.
        //        }

        //        return connection;
        //    }
        //}

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
            return adapterFactory.Create<T>(SessionFactory.DbSettings.DbAccess, this);
        }

        public ISessionFactory SessionFactory { get; private set; }

        #endregion
    }
}

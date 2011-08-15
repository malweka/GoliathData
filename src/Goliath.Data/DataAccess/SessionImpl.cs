
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
        IDbAccess dbAccess;
        DbConnection connection;
        ITransaction currentTransaction;
        bool weOwnConnection = true;
        IDataAccessAdapterFactory adapterFactory;

        static SessionImpl()
        {
            logger = Logger.GetLogger(typeof(SessionImpl));
        }

        public SessionImpl(IDbAccess dbAccess, IDataAccessAdapterFactory adapterFactory, DbConnection connection)
        {
            this.dbAccess = dbAccess;
            id = Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower();
            if (connection != null)
            {
                this.connection = connection;
                weOwnConnection = false;
            }
            this.adapterFactory = adapterFactory;
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

        public IDbAccess DbAccess
        {
            get { return dbAccess; }
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
            return adapterFactory.Create<T>(dbAccess, connection);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

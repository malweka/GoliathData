using System;
using Goliath.Data.Mapping;

namespace Goliath.Data.DataAccess
{
    [Serializable]
    class SessionFactory : ISessionFactory
    {
        readonly Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> adapterFactoryFactoryMethod;
        IDataAccessAdapterFactory adapterFactory;
        readonly IEntitySerializer serializer;


        public IEntitySerializer DataSerializer
        {
            get { return serializer; }
        }

        public IDataAccessAdapterFactory AdapterFactory
        {
            get
            {
                if (adapterFactory == null)
                {
                    adapterFactory = adapterFactoryFactoryMethod.Invoke(DbSettings.Map, serializer);
                }
                return adapterFactory;
            }
        }

        public IDatabaseSettings DbSettings { get; private set; }

        private IConnectionManager connectionManager;
        public SessionFactory(IDatabaseSettings settings, Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> adapterFactoryFactoryMethod, IEntitySerializer serializer)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (adapterFactoryFactoryMethod == null)
                throw new ArgumentNullException("adapterFactoryFactoryMethod");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            this.serializer = serializer;
            this.adapterFactoryFactoryMethod = adapterFactoryFactoryMethod;
            DbSettings = settings;

            connectionManager = new ConnectionManager(DbSettings.Connector, DbSettings.Connector.AllowMultipleConnections);
        }

        //ISession CreateSession(IConnectionProvider connProvider)
        //{
        //    return new Session(this, connProvider);
        //}

        #region ISessionFactory Members

        public ISession OpenSession(System.Data.Common.DbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            var sess = new Session(this, new ConnectionManager(DbSettings.Connector, connection, DbSettings.Connector.AllowMultipleConnections));
            return sess;
        }

        public ISession OpenSession()
        {
            var sess = new Session(this, connectionManager);
            return sess;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion
    }
}

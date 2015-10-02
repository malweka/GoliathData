using System;
using Goliath.Data.Mapping;

namespace Goliath.Data.DataAccess
{
    [Serializable]
    class SessionFactory : ISessionFactory
    {
        readonly Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> adapterFactoryFactoryMethod;
        IDataAccessAdapterFactory adapterFactory;
        IEntitySerializer serializer;


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

        private readonly IConnectionManager connectionManager;

        public SessionFactory(IDatabaseSettings settings, Func<MapConfig, IEntitySerializer, IDataAccessAdapterFactory> adapterFactoryFactoryMethod, IEntitySerializer serializer)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (adapterFactoryFactoryMethod == null)
                throw new ArgumentNullException("adapterFactoryFactoryMethod");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            this.serializer = serializer;

            EntitySerializer entitySerializer = serializer as EntitySerializer;

            if (entitySerializer != null)
            {
                entitySerializer.SessionCreator = OpenSession;
            }

            this.adapterFactoryFactoryMethod = adapterFactoryFactoryMethod;
            DbSettings = settings;

            connectionManager = new ConnectionManager(DbSettings.Connector, !DbSettings.Connector.AllowMultipleConnections);
        }

        #region ISessionFactory Members

        public ISession OpenSession(System.Data.Common.DbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            var sess = new Session(this, new ConnectionManager(DbSettings.Connector, connection, !DbSettings.Connector.AllowMultipleConnections));
            return sess;
        }

        public ISession OpenSession()
        {
            var sess = new Session(this, new ConnectionManager(DbSettings.Connector, !DbSettings.Connector.AllowMultipleConnections));
            return sess;
        }

        /// <summary>
        /// Resets the connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public void ResetConnectionString(string connectionString)
        {
            DbSettings.ResetConnection(connectionString);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            EntitySerializer entitySerializer = serializer as EntitySerializer;

            if (entitySerializer != null)
            {
                entitySerializer.SessionCreator = null;
            }

            serializer = null;
        }

        #endregion
    }
}

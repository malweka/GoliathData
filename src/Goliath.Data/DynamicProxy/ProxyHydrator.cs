using System;
using System.Linq;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using Goliath.Data.Sql;

namespace Goliath.Data.DynamicProxy
{
    class ProxyHydrator : IProxyHydrator
    {
        SqlOperationInfo query;
        Type type;
        EntityMap entityMap;
        IEntitySerializer serializer;
        static ILogger logger;
        IDatabaseSettings settings;

        static ProxyHydrator()
        {
            logger = Logger.GetLogger(typeof(ProxyHydrator));
        }

        public ProxyHydrator(SqlOperationInfo query, Type type, EntityMap entityMap, IEntitySerializer serializer, IDatabaseSettings settings)
        {
            this.query = query;
            this.type = type;
            this.entityMap = entityMap;
            this.serializer = serializer;
            this.settings = settings;
        }

        public void Hydrate(object instance, Type type)
        {
            logger.Log(LogLevel.Debug, "opening connection for proxy query");
            var dbAccess = settings.CreateAccessor();
            using (ConnectionManager connManager = new ConnectionManager(new ConnectionProvider(settings.Connector), !settings.Connector.AllowMultipleConnections))
            {
                try
                {
                    QueryParam[] parameters;
                    if (query.Parameters == null)
                    {
                        parameters = new QueryParam[] { };
                    }
                    else
                        parameters = query.Parameters.ToArray();

                    logger.Log(LogLevel.Debug, string.Format("executing query {0}", query.SqlText));
                    var dataReader = dbAccess.ExecuteReader(connManager.OpenConnection(), query.SqlText, parameters);
                    //logger.Log(LogLevel.Debug, string.Format("datareader has row? {0}", dataReader.HasRows));
                    serializer.Hydrate(instance, type, entityMap, dataReader);
                    dataReader.Dispose();

                    connManager.CloseConnection();
                }
                catch (Exception ex)
                {
                    logger.LogException("Hydrate failed", ex);
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            logger.Log(LogLevel.Debug, "Disposing of proxy");
            EntityMap entMapRef = entityMap;
            entityMap = null;
            IEntitySerializer serializerRef = serializer;
            serializer = null;
            IDatabaseSettings settingsRef = settings;
            settings = null;
        }

        #endregion
    }
}

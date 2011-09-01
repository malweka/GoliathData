using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Goliath.Data.Mapping;
using System.Data.Common;
using Goliath.Data.DataAccess;
using Goliath.Data.Providers;
using Goliath.Data.Diagnostics;
using Goliath.Data.Sql;

namespace Goliath.Data.DynamicProxy
{
    class ProxySerializer : IProxyHydrator
    {
        SqlOperationInfo query;
        Type type;
        EntityMap entityMap;
        IEntitySerializer serialFactory;
        static ILogger logger;
        IDatabaseSettings settings;

        static ProxySerializer()
        {
            logger = Logger.GetLogger(typeof(ProxySerializer));
        }

        public ProxySerializer(SqlOperationInfo query, Type type, EntityMap entityMap, IEntitySerializer factory, IDatabaseSettings settings)
        {
            this.query = query;
            this.type = type;
            this.entityMap = entityMap;
            serialFactory = factory;
            this.settings = settings;
        }

        public void Hydrate(object instance, Type type)
        {
            logger.Log(LogType.Debug, "opening connection for proxy query");
            var dbAccess = settings.CreateAccessor();
            using (ConnectionManager connManager = new ConnectionManager(new ConnectionProvider(settings.Connector), !settings.Connector.AllowMultipleConnections))
            {
                try
                {
                    DbParameter[] parameters;
                    if (query.Parameters == null)
                    {
                        parameters = new DbParameter[] { };
                    }
                    else
                        parameters = dbAccess.CreateParameters(query.Parameters).ToArray();

                    logger.Log(LogType.Debug, string.Format("executing query {0}", query.SqlText));
                    var dataReader = dbAccess.ExecuteReader(connManager.OpenConnection(), query.SqlText, parameters);
                    //logger.Log(LogType.Debug, string.Format("datareader has row? {0}", dataReader.HasRows));
                    serialFactory.Hydrate(instance, type, entityMap, dataReader);
                    dataReader.Dispose();

                    connManager.CloseConnection();
                }
                catch (Exception ex)
                {
                    logger.Log("Hydrate failed", ex);
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            logger.Log(LogType.Debug, "Disposing of proxy");
            EntityMap entMapRef = entityMap;
            entityMap = null;
            IEntitySerializer serializerRef = serialFactory;
            serialFactory = null;
            IDatabaseSettings settingsRef = settings;
            settings = null;
        }

        #endregion
    }
}

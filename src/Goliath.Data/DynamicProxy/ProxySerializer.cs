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
        ConnectionManager connManager;
        IDabaseSettings settings;

        static ProxySerializer()
        {
            logger = Logger.GetLogger(typeof(ProxySerializer));
        }

        public ProxySerializer(SqlOperationInfo query, Type type, EntityMap entityMap, IEntitySerializer factory, IDabaseSettings settings)
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
            ConnectionManager connManager = new ConnectionManager(new ConnectionProvider(settings.Connector), !settings.Connector.AllowMultipleConnections);
            using (var conn = dbAccess.CreateConnection())
            {
                conn.Open();
                DbParameter[] parameters;
                if (query.Parameters == null)
                {
                    parameters = new DbParameter[] { };
                }
                else
                    parameters = dbAccess.CreateParameters(query.Parameters).ToArray();

                logger.Log(LogType.Debug, string.Format("executing query {0}", query.SqlText));
                var dataReader = dbAccess.ExecuteReader(conn, query.SqlText, parameters);
                //logger.Log(LogType.Debug, string.Format("datareader has row? {0}", dataReader.HasRows));
                serialFactory.Hydrate(instance, type, entityMap, dataReader);
                dataReader.Dispose();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion
    }
}

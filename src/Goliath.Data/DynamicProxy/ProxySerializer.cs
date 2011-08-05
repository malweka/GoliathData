using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Goliath.Data.Mapping;
using System.Data.Common;
using Goliath.Data.DataAccess;
using Goliath.Data.Providers;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.DynamicProxy
{
    class ProxySerializer : IProxyHydrator
    {
        QueryInfo query;
        Type type;
        EntityMap entityMap;
        IEntitySerializerFactory serialFactory;
        static ILogger logger;

        static ProxySerializer()
        {
            logger = Logger.GetLogger(typeof(ProxySerializer));
        }

        public ProxySerializer(QueryInfo query, Type type, EntityMap entityMap, IEntitySerializerFactory factory)
        {
            this.query = query;
            this.type = type;
            this.entityMap = entityMap;
            serialFactory = factory;
        }

        public void Hydrate(object instance, Type type)
        {
            logger.Log(LogType.Debug, "opening connection for proxy query");
            var dbAccess = Config.ConfigManager.CurrentSettings.CreateAccessor();
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

                logger.Log(LogType.Debug, string.Format("executing query {0}", query.QuerySqlText));
                var dataReader = dbAccess.ExecuteReader(conn, query.QuerySqlText, parameters);
                //logger.Log(LogType.Debug, string.Format("datareader has row? {0}", dataReader.HasRows));
                serialFactory.Hydrate(instance, type, entityMap, dataReader);
                dataReader.Dispose();
            }
        }
    }
}

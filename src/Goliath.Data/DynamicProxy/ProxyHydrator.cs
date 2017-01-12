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

        Type type;
        EntityMap entityMap;
        IEntitySerializer serializer;
        static ILogger logger;
        private ISession session;
        private QueryBuilder queryBuilder;

        /// <summary>
        /// Initializes the <see cref="ProxyHydrator"/> class.
        /// </summary>
        static ProxyHydrator()
        {
            logger = Logger.GetLogger(typeof(ProxyHydrator));
        }


        public ProxyHydrator(QueryBuilder queryBuilder, Type type, EntityMap entityMap, IEntitySerializer serializer, ISession session)
        {
            this.type = type;
            this.entityMap = entityMap;
            this.serializer = serializer;
            this.session = session;
            this.queryBuilder = queryBuilder;
        }

        public void Hydrate(object instance, Type type)
        {
            logger.Log(LogLevel.Debug, "opening connection for proxy query");

            var sqlbody = queryBuilder.Build();
            var dbConn = session.ConnectionManager.OpenConnection();
            using (
                var dataReader = session.DataAccess.ExecuteReader(dbConn, session.CurrentTransaction, sqlbody.ToString(),
                    queryBuilder.Parameters.ToArray()))
            {
                try { serializer.Hydrate(instance, type, entityMap, sqlbody.QueryMap, dataReader); }
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

            if (session != null)
                session.Dispose();
        }

        #endregion
    }
}

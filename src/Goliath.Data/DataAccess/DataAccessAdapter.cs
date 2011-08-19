using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Goliath.Data.DataAccess;
using Goliath.Data.Config;
using Goliath.Data.Mapping;
using Goliath.Data.Diagnostics;
using Goliath.Data.Sql;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class DataAccessAdapter<TEntity> : IDataAccessAdapter<TEntity>
    {
        /// <summary>
        /// data access o
        /// </summary>
        protected IDbAccess dataAccess;
        DbConnection dbConnection;
        IEntitySerializer serializer;
        Type entityType;
        static ILogger logger;
        EntityMap entityMap;

        static DataAccessAdapter()
        {
            logger = Logger.GetLogger(typeof(DataAccessAdapter<>));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessAdapter&lt;TEntity&gt;"/> class.
        /// </summary>
        /// <param name="dataAccess">The data access.</param>
        public DataAccessAdapter(IEntitySerializer serializerFactory, IDbAccess dataAccess, DbConnection dbConnection)
        {
            if (dataAccess == null)
                throw new ArgumentNullException("dataAccess");
            this.serializer = serializerFactory;
            this.dataAccess = dataAccess;
            this.dbConnection = dbConnection;
            entityType = typeof(TEntity);

            if (entityMap == null)
            {
                ConfigManager.CurrentSettings.Map.EntityConfigs.TryGetValue(entityType.FullName, out entityMap);

                if (entityMap == null)
                    throw new ArgumentException(string.Format("Typ {0} is not map entity", entityType.FullName));
            }
        }

        

        void CheckConnection(DbConnection dbConnection)
        {
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();
        }

        #region IDataAccessAdapter<TEntity> Members

        #region Updates 

        public int Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int Update(TEntity entity, QueryParam[] filters)
        {
            throw new NotImplementedException();
        }

        public int UpdateBatch(IEnumerable<TEntity> entityList)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Inserts

        public int InsertBatch(IEnumerable<TEntity> batch)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public int Insert(TEntity entity)
        {
            Type type = typeof(TEntity);
            var map = ConfigManager.CurrentSettings.Map;
            var entityMap = map.EntityConfigs[type.FullName];
            if (entityMap == null)
                throw new DataAccessException("entity {0} not mapped.", type.FullName);

            var qInfo = serializer.BuildInsertSql(entityMap, entity);

            CheckConnection(dbConnection);

            logger.Log(LogType.Debug, qInfo.QuerySqlText);

            var parameters = dataAccess.CreateParameters(qInfo.Parameters).ToArray();
            try
            {
                int qResult = dataAccess.ExecuteNonQuery(dbConnection, qInfo.QuerySqlText, parameters);
                return qResult;
            }
            catch (GoliathDataException ex)
            {
                Console.WriteLine("goliath exception {0}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                throw new GoliathDataException(string.Format("Exception while inserting: {0}", qInfo.QuerySqlText), ex);
            }
        }

        #endregion

        #region Queries

        public IList<TEntity> FindAll(string sqlQuery)
        {
            throw new NotImplementedException();
        }

        public IList<TEntity> FindAll(params PropertyQueryParam[] filters)
        {
            SelectSqlBuilder queryBuilder = new SelectSqlBuilder(serializer.SqlMapper, entityMap);
            
            throw new NotImplementedException();
        }

        public IList<TEntity> FindAll(int pageIndex, int pageSize, out int totalRecords, params PropertyQueryParam[] filters)
        {
            throw new NotImplementedException();
        }

        public TEntity FindOne(PropertyQueryParam filter, params PropertyQueryParam[] filters)
        {
            throw new NotImplementedException();
        }

        public int Delete(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int Delete(QueryParam[] filters)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}

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
        /// <param name="serializer">The serializer.</param>
        /// <param name="dataAccess">The data access.</param>
        /// <param name="dbConnection">The db connection.</param>
        public DataAccessAdapter(IEntitySerializer serializer, IDbAccess dataAccess, DbConnection dbConnection)
        {
            if (dataAccess == null)
                throw new ArgumentNullException("dataAccess");
            this.serializer = serializer;
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
            entityMap = map.EntityConfigs[type.FullName];
            if (entityMap == null)
                throw new DataAccessException("entity {0} not mapped.", type.FullName);

            var qInfo = serializer.BuildInsertSql(entityMap, entity);
            

            logger.Log(LogType.Debug, qInfo.QuerySqlText);

            var parameters = dataAccess.CreateParameters(qInfo.Parameters).ToArray();
            try
            {
                CheckConnection(dbConnection);
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
            DbDataReader dataReader;
            if ((filters != null) && (filters.Length > 0))
            {
                for (int i = 0; i < filters.Length; i++)
                {
                    var prop = entityMap[filters[i].PropertyName];
                    if (prop == null)
                        throw new MappingException(string.Format("Property {0} not found in mapped entity {1}", filters[i].PropertyName, entityMap.FullName));

                    filters[i].SetParameterName(prop.ColumnName, entityMap.TableAlias);
                    WhereStatement w = new WhereStatement(prop.ColumnName)
                    {
                        Operator = filters[i].ComparisonOperator,
                        PostOperator = filters[i].PostOperator,
                        RightOperand = new StringOperand(serializer.SqlMapper.CreateParameterName(ParameterNameBuilderHelper.ColumnQueryName(prop.ColumnName, entityMap.TableAlias)))
                    };
                    queryBuilder.Where(w);
                }
                DbParameter[] parameters = dataAccess.CreateParameters(filters).ToArray();
                var query = queryBuilder.ToSqlString();
                logger.Log(LogType.Debug, query);
                CheckConnection(dbConnection);
                dataReader = dataAccess.ExecuteReader(dbConnection, query, parameters);
            }
            else
            {
                var query = queryBuilder.ToSqlString();
                logger.Log(LogType.Debug, query);
                CheckConnection(dbConnection);
                dataReader = dataAccess.ExecuteReader(dbConnection, query);
            }

            var entities = serializer.SerializeAll<TEntity>(dataReader, entityMap);
            return entities;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IList<TEntity> FindAll(int pageIndex, int pageSize, out int totalRecords, params PropertyQueryParam[] filters)
        {
            if (pageIndex < 1)
                throw new ArgumentException(" cannot have a pageIndex of less than or equal to 0");
            if(pageSize <1)
                throw new ArgumentException(" cannot have a pageSize of less than or equal to 0");

            throw new NotImplementedException();
        }

        public TEntity FindOne(PropertyQueryParam filter, params PropertyQueryParam[] filters)
        {
            throw new NotImplementedException();
        }

        #endregion

        public int Delete(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public int Delete(QueryParam[] filters)
        {
            throw new NotImplementedException();
        }

        

        #endregion
    }
}

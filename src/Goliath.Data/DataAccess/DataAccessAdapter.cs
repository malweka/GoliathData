using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Goliath.Data.Config;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
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
        public int Insert(TEntity entity, bool recursive = false)
        {
            Type type = typeof(TEntity);
            var map = ConfigManager.CurrentSettings.Map;

            var qInfo = serializer.BuildInsertSql(entityMap, entity, recursive);            
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

        SelectSqlBuilder BuildSelectSql(PropertyQueryParam[] filters, out ICollection<DbParameter> dbParams)
        {
            SelectSqlBuilder queryBuilder = new SelectSqlBuilder(serializer.SqlMapper, entityMap);
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

                dbParams = dataAccess.CreateParameters(filters);
            }
            else
                dbParams = new DbParameter[] { };

            return queryBuilder;
        }

        #region Queries

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <returns></returns>
        public IList<TEntity> FindAll(string sqlQuery, params DbParameter[] parameters)
        {
            logger.Log(LogType.Debug, sqlQuery);
            try
            {
                DbDataReader dataReader;
                CheckConnection(dbConnection);
                dataReader = dataAccess.ExecuteReader(dbConnection, sqlQuery, parameters);
                var entities = serializer.SerializeAll<TEntity>(dataReader, entityMap);
                return entities;
            }
            catch (GoliathDataException ex)
            {
                Console.WriteLine("Encounter GoliathException. Exception rethrown. {0}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(string.Format("Error while trying to fetch all {0}", entityMap.FullName), ex);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IList<TEntity> FindAll(params PropertyQueryParam[] filters)
        {
            ICollection<DbParameter> dbParams;
            SelectSqlBuilder queryBuilder = BuildSelectSql(filters, out dbParams);
            DbDataReader dataReader;

            DbParameter[] parameters = dbParams.ToArray();
            var query = queryBuilder.ToSqlString();
            logger.Log(LogType.Debug, query);            
            try
            {
                CheckConnection(dbConnection);
                dataReader = dataAccess.ExecuteReader(dbConnection, query, parameters);
                var entities = serializer.SerializeAll<TEntity>(dataReader, entityMap);
                return entities;
            }
            catch (GoliathDataException ex)
            {
                Console.WriteLine("Encounter GoliathException. Exception rethrown. {0}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(string.Format("Error while trying to fetch all {0}", entityMap.FullName), ex);
            }
                      
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IList<TEntity> FindAll(int limit, int offset, out long totalRecords, params PropertyQueryParam[] filters)
        {
            if (limit < 1)
                throw new ArgumentException(" cannot have a pageIndex of less than or equal to 0");
            if(offset <0 )
                throw new ArgumentException(" cannot have a pageSize of less than or equal to 0");

            ICollection<DbParameter> dbParams;
            SelectSqlBuilder queryBuilder = BuildSelectSql(filters, out dbParams);
            string selectCount = queryBuilder.SelectCount();
            totalRecords = 0;
            queryBuilder = queryBuilder.WithPaging(limit, offset);
            string query = string.Format("{0};\n{1};", selectCount.Trim(), queryBuilder.ToSqlString().Trim());
            logger.Log(LogType.Debug, query);

            try
            {
                DbParameter[] parameters = dbParams.ToArray();
                DbDataReader dataReader;
                CheckConnection(dbConnection);
                dataReader = dataAccess.ExecuteReader(dbConnection, query, parameters);
                while (dataReader.Read())
                {
                    var type = dataReader.GetFieldType(0);
                    if (typeof(long).Equals(type))
                        totalRecords = (long)dataReader[0];
                    else
                        totalRecords = Convert.ToInt64(dataReader[0]);
                    break;
                }
                dataReader.NextResult();
                var entities = serializer.SerializeAll<TEntity>(dataReader, entityMap);
                return entities;
            }
            catch (GoliathDataException ex)
            {
                Console.WriteLine("Encounter GoliathException. Exception rethrown. {0}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(string.Format("Error while trying to fetch all {0}", entityMap.FullName), ex);
            }
        }

        /// <summary>
        /// Finds the one.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public TEntity FindOne(PropertyQueryParam filter, params PropertyQueryParam[] filters)
        {
            List<PropertyQueryParam> parameters = new List<PropertyQueryParam>();
            parameters.Add(filter);
            parameters.AddRange(filters);

            long total;
            var all = FindAll(1, 0, out total, parameters.ToArray());
            return all.FirstOrDefault();
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

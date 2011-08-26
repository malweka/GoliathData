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
            var sqlWorker = serializer.CreateSqlWorker();
            using (var trans = dbConnection.BeginTransaction())
            {
                Dictionary<string, QueryParam> neededParams = new Dictionary<string, QueryParam>();
                List<string> inserts = new List<string>();

                using (var batchOp = sqlWorker.BuildInsertSql(entityMap, entity, recursive))
                {
                    BuildOrExecuteInsertBatchOperation(batchOp, neededParams, inserts);
                }
                var cur = System.Transactions.Transaction.Current;
                if (cur != null)
                    Console.WriteLine(cur.TransactionInformation.LocalIdentifier);

            }
            //logger.Log(LogType.Debug, qInfo.SqlText);
            //var parameters = dataAccess.CreateParameters(qInfo.Parameters).ToArray();
            //try
            //{
            //    CheckConnection(dbConnection);
            //    int qResult = dataAccess.ExecuteNonQuery(dbConnection, qInfo.SqlText, parameters);
            //    return qResult;
            //}
            //catch (GoliathDataException ex)
            //{
            //    Console.WriteLine("goliath exception {0}", ex.Message);
            //    throw;
            //}
            //catch (Exception ex)
            //{
            //    throw new GoliathDataException(string.Format("Exception while inserting: {0}", qInfo.SqlText), ex);
            //}

            return 0;
        }

        #endregion

        void BuildOrExecuteInsertBatchOperation(BatchSqlOperation batchOp, Dictionary<string, QueryParam> neededParams, List<string> batchInserts)
        {
            if (batchOp == null)
                throw new ArgumentNullException("batchOp");

           
            if (batchOp.KeyGenerationOperations.Count > 0)
            {
                var hasGreaterPriority = batchOp.KeyGenerationOperations.Where(op => op.Value.Priority > batchOp.Priority).ToList();
                //execute these first
                for (int i = 0; i < hasGreaterPriority.Count; i++)
                {
                    //we expects that this queries will be for generating ideas and that the ID will be the first column
                    //returned and only 1 row of data. Therefore, we will reader column 1 row 1 ignore rest.
                    ReadGeneratedData(hasGreaterPriority[i], neededParams);
                    batchOp.KeyGenerationOperations.Remove(hasGreaterPriority[i].Key);
                }
            }

            List<string> inserts = new List<string>();

            List<QueryParam> insertParams = new List<QueryParam>();
            insertParams.AddRange(neededParams.Values);

            for (int i = 0; i < batchOp.Operations.Count; i++)
            {
                inserts.Add(batchOp.Operations[i].SqlText);
                
                insertParams.AddRange(batchOp.Operations[i].Parameters);
            }

            //read if we have post insert get id sql
            if (batchOp.KeyGenerationOperations.Count > 0)
            {
                foreach(var kop in batchOp.KeyGenerationOperations)
                {
                    inserts.Add(kop.Value.Operation.SqlText);
                }
            }

            if (batchOp.Priority < SqlOperationPriority.High)
            {
                foreach (var param in insertParams)
                {
                    if (!neededParams.ContainsKey(param.Name))
                        neededParams.Add(param.Name, param);
                }
                batchInserts.AddRange(inserts);

                for (int i = 0; i < batchOp.SubOperations.Count;i++ )
                {
                    BuildOrExecuteInsertBatchOperation(batchOp.SubOperations[i], neededParams, batchInserts);
                }
            }

            else
            {
                //execute operations
                DbDataReader dataReader;
                CheckConnection(dbConnection);
                var paramList = dataAccess.CreateParameters(neededParams.Values);
                var sql = string.Join(";\n", inserts);
                //dataReader = dataAccess.ExecuteReader(dbConnection, sql, paramList.ToArray());

                if (batchOp.KeyGenerationOperations.Count > 0) // read resulting id that were created
                {
                    Console.Write("bo");
                }
            }
        }

        void ReadGeneratedData(KeyValuePair<string, KeyGenOperationInfo> kpair, Dictionary<string, QueryParam> neededParams)
        {
            var kgInfo = kpair.Value;
            var paramName = kpair.Key;

            DbDataReader dataReader;
            CheckConnection(dbConnection);
            dataReader = dataAccess.ExecuteReader(dbConnection, kgInfo.Operation.SqlText);
            if (dataReader.HasRows)
            {
                dataReader.Read();
                object id = serializer.ReadFieldData(kgInfo.PropertyType, 0, dataReader);
                serializer.SetPropertyValue(kgInfo.Entity, kgInfo.PropertyName, id);

                if (!neededParams.ContainsKey(paramName))
                    neededParams.Add(paramName, new QueryParam(paramName, id));
            }

            dataReader.Dispose();
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
                dataReader.Dispose();
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
            SelectSqlBuilder queryBuilder = SqlWorker.BuildSelectSql(entityMap, serializer.SqlMapper, dataAccess, filters, out dbParams);
            DbDataReader dataReader;

            DbParameter[] parameters = dbParams.ToArray();
            var query = queryBuilder.ToSqlString();
            logger.Log(LogType.Debug, query);
            try
            {
                CheckConnection(dbConnection);
                dataReader = dataAccess.ExecuteReader(dbConnection, query, parameters);
                var entities = serializer.SerializeAll<TEntity>(dataReader, entityMap);
                dataReader.Dispose();
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

            if (offset < 0)
                throw new ArgumentException(" cannot have a pageSize of less than or equal to 0");

            ICollection<DbParameter> dbParams;
            SelectSqlBuilder queryBuilder = SqlWorker.BuildSelectSql(entityMap, serializer.SqlMapper, dataAccess, filters, out dbParams);
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

                //First resultset contains the count
                while (dataReader.Read())
                {
                    totalRecords = serializer.ReadFieldData<long>(0, dataReader);
                    //we only expect 1 row of data to be returned, so let's break out of the loop.
                    break;
                }

                //move to the next result set which contains the entities
                dataReader.NextResult();
                var entities = serializer.SerializeAll<TEntity>(dataReader, entityMap);
                dataReader.Dispose();
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

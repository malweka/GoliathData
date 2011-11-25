using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    using Config;
    using DataAccess;
    using Diagnostics;
    using Mapping;
    using Sql;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    [Serializable]
    public class DataAccessAdapter<TEntity> : IDataAccessAdapter<TEntity>, IDisposable
    {
        /// <summary>
        /// current session
        /// </summary>
        protected ISession session;
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
        /// <param name="session">The session.</param>
        public DataAccessAdapter(IEntitySerializer serializer, ISession session)
            : this(null, serializer, session)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessAdapter&lt;TEntity&gt;"/> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="dataAccess">The data access.</param>
        /// <param name="dbConnection">The db connection.</param>
        public DataAccessAdapter(EntityMap entityMap, IEntitySerializer serializer, ISession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            this.serializer = serializer;
            this.session = session;

            entityType = typeof(TEntity);

            if (entityMap == null)
            {
                var map = session.SessionFactory.DbSettings.Map;
                entityMap = map.GetEntityMap(entityType.FullName);
            }

            this.entityMap = entityMap;
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

        /// <summary>
        /// Inserts the batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <returns></returns>
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

            //var map = session.SessionFactory.DbSettings.Map;
            var sqlWorker = serializer.CreateSqlWorker();

            bool ownTransaction = false;
            int result = 0;

            if ((session.CurrentTransaction == null) || !session.CurrentTransaction.IsStarted)
            {
                session.BeginTransaction();
                ownTransaction = true;
            }

            Dictionary<string, QueryParam> neededParams = new Dictionary<string, QueryParam>();
            List<SqlOperationInfo> inserts = new List<SqlOperationInfo>();

            using (var batchOp = sqlWorker.BuildInsertSql(entityMap, entity, recursive))
            {
                BuildOrExecuteInsertBatchOperation(session.CurrentTransaction, batchOp, neededParams, inserts);
            }

            var paramList = session.DataAccess.CreateParameters(neededParams.Values);
            StringBuilder sqlInserts = new StringBuilder();
            for (int i = 0; i < inserts.Count; i++)
            {
                sqlInserts.Append(inserts[i].SqlText);
                sqlInserts.Append(";\n");
            }
            string sql = sqlInserts.ToString();
            logger.Log(LogType.Debug, sql);
            try
            {
                result = session.DataAccess.ExecuteNonQuery(session.ConnectionManager.OpenConnection(), session.CurrentTransaction, sql, paramList.ToArray());
                if (ownTransaction)
                    session.CommitTransaction();
            }
            catch (GoliathDataException ex)
            {
                Console.WriteLine("goliath exception: {0}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                throw new GoliathDataException(string.Format("Exception while inserting: {0}", sql), ex);
            }
            finally
            {
                neededParams.Clear();
            }

            return result;
        }

        #endregion

        void BuildOrExecuteInsertBatchOperation(ITransaction transaction, BatchSqlOperation batchOp, Dictionary<string, QueryParam> neededParams, List<SqlOperationInfo> batchOperations)
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
                    ReadGeneratedId(hasGreaterPriority[i], neededParams);
                    batchOp.KeyGenerationOperations.Remove(hasGreaterPriority[i].Key);
                }
            }

            List<SqlOperationInfo> inserts = new List<SqlOperationInfo>();

            List<QueryParam> insertParams = new List<QueryParam>();
            insertParams.AddRange(neededParams.Values);

            for (int i = 0; i < batchOp.Operations.Count; i++)
            {
                inserts.Add(batchOp.Operations[i]);
                insertParams.AddRange(batchOp.Operations[i].Parameters);
            }

            ////read if we have post insert get id sql
            //if (batchOp.KeyGenerationOperations.Count > 0)
            //{
            //    foreach (var kop in batchOp.KeyGenerationOperations)
            //    {
            //       inserts.Add(kop.Value.Operation.SqlText);
            //        //ReadGeneratedId(kop, neededParams);
            //    }

            //    //batchOp.KeyGenerationOperations.Clear();
            //}

            foreach (var param in insertParams)
            {
                if (!neededParams.ContainsKey(param.Name))
                    neededParams.Add(param.Name, param);
            }

            if (batchOp.Priority < SqlOperationPriority.High)
            {
                batchOperations.AddRange(inserts);
            }
            else
            {
                var paramList = session.DataAccess.CreateParameters(neededParams.Values);

                for (int i = 0; i < inserts.Count; i++)
                {
                    var sql = inserts[i].SqlText;
                    var xt = session.DataAccess.ExecuteNonQuery(session.ConnectionManager.OpenConnection(), transaction, sql, paramList.ToArray());
                    logger.Log(LogType.Debug, sql);
                    if (batchOp.KeyGenerationOperations.Count > 0 && i == 0) // read resulting id that were created
                    {
                        //TODO: generated keys from database
                        foreach (var kop in batchOp.KeyGenerationOperations)
                        {
                            //inserts.Add(kop.Value.Operation.SqlText);
                            ReadGeneratedId(kop, neededParams);
                        }
                    }
                }

            }

            for (int i = 0; i < batchOp.SubOperations.Count; i++)
            {
                BuildOrExecuteInsertBatchOperation(transaction, batchOp.SubOperations[i], neededParams, batchOperations);
            }
        }

        void ReadGeneratedId(KeyValuePair<string, KeyGenOperationInfo> kpair, Dictionary<string, QueryParam> neededParams)
        {
            var kgInfo = kpair.Value;
            var paramName = kpair.Key;


            var val = session.DataAccess.ExecuteScalar(session.ConnectionManager.OpenConnection(), kgInfo.Operation.SqlText);
            if (val != null)
            {
                object id = serializer.ReadFieldData(kgInfo.PropertyType, val);
                serializer.SetPropertyValue(kgInfo.Entity, kgInfo.PropertyName, id);

                if (!neededParams.ContainsKey(paramName))
                    neededParams.Add(paramName, new QueryParam(paramName, id));
            }

            //dataReader.Dispose();
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
                dataReader = session.DataAccess.ExecuteReader(session.ConnectionManager.OpenConnection(), sqlQuery, parameters);
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
            SelectSqlBuilder queryBuilder = SqlWorker.BuildSelectSql(entityMap, serializer.SqlMapper, session.DataAccess, filters, out dbParams);
            DbDataReader dataReader;

            DbParameter[] parameters = dbParams.ToArray();
            var query = queryBuilder.ToSqlString();
            logger.Log(LogType.Debug, query);
            try
            {
                dataReader = session.DataAccess.ExecuteReader(session.ConnectionManager.OpenConnection(), query, parameters);
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
            SelectSqlBuilder queryBuilder = SqlWorker.BuildSelectSql(entityMap, serializer.SqlMapper, session.DataAccess, filters, out dbParams);
            string selectCount = queryBuilder.SelectCount();

            totalRecords = 0;
            queryBuilder = queryBuilder.WithPaging(limit, offset);
            string query = string.Format("{0};\n{1};", selectCount.Trim(), queryBuilder.ToSqlString().Trim());
            logger.Log(LogType.Debug, query);

            try
            {
                DbParameter[] parameters = dbParams.ToArray();
                DbDataReader dataReader;
                dataReader = session.DataAccess.ExecuteReader(session.ConnectionManager.OpenConnection(), query, parameters);

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

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ////clean up to avoid leaks
            //IEntitySerializer serializerRef = this.serializer;
            //this.serializer = null;

            //EntityMap entMapRef = this.entityMap;
            //this.entityMap = null;
        }

        #endregion
    }
}

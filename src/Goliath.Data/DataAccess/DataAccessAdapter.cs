using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
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

        #region ctors

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
        /// <param name="entityMap">The entity map.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="session">The session.</param>
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

        #endregion

        #region Updates

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public int Update(TEntity entity)
        {
            var sqlWorker = serializer.CreateSqlWorker();
            bool ownTransaction = false;
            int result = 0;

            var dbConn = session.ConnectionManager.OpenConnection();

            if ((session.CurrentTransaction == null) || !session.CurrentTransaction.IsStarted)
            {
                session.BeginTransaction();
                ownTransaction = true;
            }

            Dictionary<string, QueryParam> neededParams = new Dictionary<string, QueryParam>();
            List<SqlOperationInfo> operations = new List<SqlOperationInfo>();

            using (var batchOp = sqlWorker.BuildUpdateSql<TEntity>(entityMap, entity, true))
            {
                BuildUpdateOperations(batchOp, neededParams, operations);
            }
            var paramList = neededParams.Values.ToArray();
            StringBuilder sqlUpdates = new StringBuilder();

            for (int i = 0; i < operations.Count; i++)
            {
                sqlUpdates.Append(operations[i].SqlText);
                sqlUpdates.Append(";\n");
            }

            string sql = sqlUpdates.ToString();

            try
            {
                result = session.DataAccess.ExecuteNonQuery(dbConn, session.CurrentTransaction, sql, paramList);
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
                throw new GoliathDataException(string.Format("Exception while updating: {0}", sql), ex);
            }
            finally
            {
                neededParams.Clear();
            }

            return result;
        }

        void BuildUpdateOperations(BatchSqlOperation batchOp, Dictionary<string, QueryParam> parameters, List<SqlOperationInfo> operations)
        {
            List<SqlOperationInfo> updates = new List<SqlOperationInfo>();


            for (int i = 0; i < batchOp.Operations.Count; i++)
            {
                updates.Add(batchOp.Operations[i]);
                foreach (var paramet in batchOp.Operations[i].Parameters)
                {
                    if (!parameters.ContainsKey(paramet.Name))
                        parameters.Add(paramet.Name, paramet);
                }
            }

            operations.AddRange(updates);
            for (int i = 0; i < batchOp.SubOperations.Count; i++)
            {
                BuildUpdateOperations(batchOp.SubOperations[i], parameters, operations);
            }
        }

        #endregion

        #region Inserts

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <returns></returns>
        public int Insert(TEntity entity, bool recursive = false)
        {
            var sqlWorker = serializer.CreateSqlWorker();

            bool ownTransaction = false;
            int result = 0;

            var dbConn = session.ConnectionManager.OpenConnection();

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

            var paramList = neededParams.Values.ToArray();

            StringBuilder sqlInserts = new StringBuilder();
            for (int i = 0; i < inserts.Count; i++)
            {
                sqlInserts.Append(inserts[i].SqlText);
                sqlInserts.Append(";\n");
            }

            string sql = sqlInserts.ToString();
            try
            {
                result = session.DataAccess.ExecuteNonQuery(dbConn, session.CurrentTransaction, sql, paramList);
                if (ownTransaction)
                    session.CommitTransaction();
            }
            catch (GoliathDataException ex)
            {
                Console.WriteLine("Goliath exception: {0}", ex.Message);
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
                foreach (var paramet in batchOp.Operations[i].Parameters)
                {
                    insertParams.Add(paramet);
                }
            }

            foreach (var param in insertParams)
            {
                if (!neededParams.ContainsKey(param.Name))
                {
                    neededParams.Add(param.Name, param);
                }
            }

            if (batchOp.Priority < SqlOperationPriority.High)
            {
                batchOperations.AddRange(inserts);
            }
            else
            {
                var paramList = neededParams.Values.ToArray();

                for (int i = 0; i < inserts.Count; i++)
                {
                    var sql = inserts[i].SqlText;
                    var xt = session.DataAccess.ExecuteNonQuery(session.ConnectionManager.OpenConnection(), transaction, sql, paramList);
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
        }

        #endregion

        #region Queries

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IList<TEntity> FindAll(string sqlQuery, params QueryParam[] parameters)
        {
            try
            {
                SqlCommandRunner runner = new SqlCommandRunner();
                var entities = runner.RunList<TEntity>(session, sqlQuery, parameters);
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
            SelectSqlBuilder queryBuilder = SqlWorker.BuildSelectSql(entityMap, serializer.SqlDialect, session.DataAccess, filters);

            var query = queryBuilder.ToSqlString();
            try
            {
                SqlCommandRunner runner = new SqlCommandRunner();
                var entities = runner.RunList<TEntity>(session, query, filters);
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

            SelectSqlBuilder queryBuilder = SqlWorker.BuildSelectSql(entityMap, serializer.SqlDialect, session.DataAccess, filters);
            SqlCommandRunner runner = new SqlCommandRunner();
            totalRecords = 0;

            try
            {
                var entities = runner.RunList<TEntity>(session, queryBuilder.Build(), limit, offset, out totalRecords, filters);
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

            var paramArray = parameters.ToArray();
            SqlCommandRunner runner = new SqlCommandRunner();
            SelectSqlBuilder queryBuilder = SqlWorker.BuildSelectSql(entityMap, serializer.SqlDialect, session.DataAccess, paramArray);

            var entity = runner.Run<TEntity>(session, queryBuilder.Build(), paramArray);
            return entity;
        }

        #endregion

        #region Deletes

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public int Delete(TEntity entity)
        {
            return Delete(entity, false);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cascade">if set to <c>true</c> [cascade].</param>
        /// <returns></returns>
        public int Delete(TEntity entity, bool cascade)
        {
            var sqlWorker = serializer.CreateSqlWorker();
            bool ownTransaction = false;
            int result = 0;

            var dbConn = session.ConnectionManager.OpenConnection();

            if ((session.CurrentTransaction == null) || !session.CurrentTransaction.IsStarted)
            {
                session.BeginTransaction();
                ownTransaction = true;
            }

            Dictionary<string, QueryParam> neededParams = new Dictionary<string, QueryParam>();
            List<SqlOperationInfo> operations = new List<SqlOperationInfo>();

            using (var batchOp = sqlWorker.BuildDeleteSql<TEntity>(entityMap, entity, cascade))
            {
                BuildDeleteOperations(batchOp, neededParams, operations);
            }

            var paramList = neededParams.Values.ToArray();
            StringBuilder sqlDeletes = new StringBuilder();

            for (int i = 0; i < operations.Count; i++)
            {
                sqlDeletes.Append(operations[i].SqlText);
                sqlDeletes.Append(";\n");
            }

            string sql = sqlDeletes.ToString();

            try
            {
                result = session.DataAccess.ExecuteNonQuery(dbConn, session.CurrentTransaction, sql, paramList);
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
                throw new GoliathDataException(string.Format("Exception while deleting: {0}", sql), ex);
            }
            finally
            {
                neededParams.Clear();
            }

            return result;
        }

        void BuildDeleteOperations(BatchSqlOperation batchOp, Dictionary<string, QueryParam> parameters, List<SqlOperationInfo> operations)
        {
            List<SqlOperationInfo> deletes = new List<SqlOperationInfo>();

            for (int i = 0; i < batchOp.Operations.Count; i++)
            {
                deletes.Add(batchOp.Operations[i]);
                foreach (var paramet in batchOp.Operations[i].Parameters)
                {
                    if (!parameters.ContainsKey(paramet.Name))
                        parameters.Add(paramet.Name, paramet);
                }
            }

            operations.AddRange(deletes);

            for (int i = 0; i < batchOp.SubOperations.Count; i++)
            {
                BuildDeleteOperations(batchOp.SubOperations[i], parameters, operations);
            }
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

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
using Goliath.Data.Utils;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    [Serializable]
    public class DataAccessAdapter<TEntity> : IDataAccessAdapter<TEntity>
    {
        /// <summary>
        /// current session
        /// </summary>
        protected ISession session;
        readonly IEntitySerializer serializer;
        readonly Type entityType;
        static ILogger logger;
        readonly EntityMap entityMap;
        readonly SqlCommandRunner commandRunner = new SqlCommandRunner();
        readonly EntityAccessorStore entityAccessorStore = new EntityAccessorStore();
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

        #endregion

        #region Queries

        public IQueryBuilder<TEntity> Select()
        {
            var queryBuilder = new QueryBuilder<TEntity>(session);
            return queryBuilder;
        }

        public ICollection<TEntity> FetchAll()
        {
            return Select().FetchAll();
        }

        public ICollection<TEntity> FetchAll(int limit, int offset)
        {
            return Select().Take(limit, offset).FetchAll();
        }

        public ICollection<TEntity> FetchAll(int limit, int offset, out long total)
        {
            return Select().Take(limit, offset).FetchAll(out total);
        }

        public int Update(TEntity entity)
        {
            if (entityMap.PrimaryKey == null)
                throw new GoliathDataException(string.Format("Cannot update entity {0} because no primary key has been defined for table {1}", entityMap.FullName, entityMap.TableName));
            try
            {
                INonQuerySqlBuilder<TEntity> updateBuilder = new UpdateSqlBuilder<TEntity>(session, entity);
                var pk = entityMap.PrimaryKey;
                var entAccessor = entityAccessorStore.GetEntityAccessor(entityType, entityMap);
                var firstKeyPropertyName = pk.Keys[0].Key.PropertyName;
                var firstKey = entAccessor.GetPropertyAccessor(firstKeyPropertyName);

                var filterBuilder = updateBuilder.Where(firstKeyPropertyName).EqualToValue(firstKey.GetMethod(entity));
                if (pk.Keys.Count > 1)
                {
                    for (int i = 0; i < pk.Keys.Count; i++)
                    {
                        var propName = pk.Keys[i].Key.PropertyName;
                        var propAccessor = entAccessor.GetPropertyAccessor(propName);
                        filterBuilder.And(pk.Keys[i].Key.PropertyName).EqualToValue(propAccessor.GetMethod(entity));
                    }
                }

                return filterBuilder.Execute();
            }
            catch (GoliathDataException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new GoliathDataException(string.Format("Error while trying to update mapped entity {0} of type {1}", entityMap.FullName, entity.GetType()), exception);
            }
           
        }

        public int Insert(TEntity entity, bool recursive = false)  
        {
            try
            {
                var insertBuilder = new InsertSqlBuilder();
                var execList = insertBuilder.Build(entity, entityMap, session);
                return execList.Execute(session);
            }
            catch (GoliathDataException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new GoliathDataException(string.Format("Error while trying to insert mapped entity {0} of type {1}", entityMap.FullName, entity.GetType()), exception);
            }
            
        }

        #endregion

        #region Deletes

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cascade">if set to <c>true</c> [cascade].</param>
        /// <returns></returns>
        public int Delete(TEntity entity, bool cascade = false)
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

using System;
using System.Collections.Generic;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Entity;
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

        #region CRUD operations

        /// <summary>
        /// Selects this instance.
        /// </summary>
        /// <returns></returns>
        public IQueryBuilder<TEntity> Select()
        {
            var queryBuilder = new QueryBuilder<TEntity>(session);
            return queryBuilder;
        }

        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <returns></returns>
        public ICollection<TEntity> FetchAll()
        {
            return Select().FetchAll();
        }

        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public ICollection<TEntity> FetchAll(int limit, int offset)
        {
            return Select().Take(limit, offset).FetchAll();
        }

        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="total">The total.</param>
        /// <returns></returns>
        public ICollection<TEntity> FetchAll(int limit, int offset, out long total)
        {
            return Select().Take(limit, offset).FetchAll(out total);
        }

        int ExecuteUpdateOrDeleteEntity(INonQuerySqlBuilder<TEntity> sqlBuilder, TEntity entity)
        {
            var pk = entityMap.PrimaryKey;
            var entAccessor = entityAccessorStore.GetEntityAccessor(entityType, entityMap);
            var firstKeyPropertyName = pk.Keys[0].Key.PropertyName;
            var firstKey = entAccessor.GetPropertyAccessor(firstKeyPropertyName);

            var filterBuilder = sqlBuilder.Where(firstKeyPropertyName).EqualToValue(firstKey.GetMethod(entity));
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

        void ResetTrackableEntity(ITrackable trackable)
        {
            if (trackable == null || !trackable.IsDirty) return;
            trackable.ChangeTracker.CommitChanges();
            trackable.Version = trackable.ChangeTracker.Version;
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="GoliathDataException"></exception>
        public int Update(TEntity entity)
        {
            if (entityMap.PrimaryKey == null)
                throw new GoliathDataException(string.Format("Cannot update entity {0} because no primary key has been defined for table {1}", entityMap.FullName, entityMap.TableName));
            try
            {
                INonQuerySqlBuilder<TEntity> updateBuilder = new UpdateSqlBuilder<TEntity>(session, entityMap, entity);
                var result = ExecuteUpdateOrDeleteEntity(updateBuilder, entity);

                //if it's a trackable entity let's reset it 
                ResetTrackableEntity(entity as ITrackable);

                return result;
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

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public int Delete(TEntity entity)
        {
            if (entityMap.PrimaryKey == null)
                throw new GoliathDataException(string.Format("Cannot delete entity {0} because no primary key has been defined for table {1}", entityMap.FullName, entityMap.TableName));
            try
            {
                INonQuerySqlBuilder<TEntity> deleteBuilder = new DeleteSqlBuilder<TEntity>(session, entityMap, entity);
                return ExecuteUpdateOrDeleteEntity(deleteBuilder, entity);
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

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="GoliathDataException"></exception>
        public int Insert(TEntity entity)
        {
            try
            {
                var insertBuilder = new InsertSqlBuilder();
                var execList = insertBuilder.Build(entity, entityMap, session);
                var result = execList.Execute(session);

                //if it's a trackable entity let's reset it 
                var trackable = entity as ITrackable;
                //ResetTrackableEntity(trackable);
                if(trackable != null && !trackable.ChangeTracker.IsTracking)
                {
                    trackable.ChangeTracker.Init();
                   //TODO: write method to initialize values;
                    trackable.ChangeTracker.Start();
                }
                return result;
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

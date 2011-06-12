using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessAdapter&lt;TEntity&gt;"/> class.
        /// </summary>
        /// <param name="dataAccess">The data access.</param>
        public DataAccessAdapter(IDbAccess dataAccess)
        {
            if (dataAccess == null)
                throw new ArgumentNullException("dataAccess");

            this.dataAccess = dataAccess;
        }

        protected DbParameter[] GetParameters(QueryParam[] filters)
        {
            if (filters == null)
                return new DbParameter[] { };

            List<DbParameter> parameters = new List<DbParameter>();
            foreach (var f in filters)
            {
                parameters.Add(dataAccess.CreateParameter(f));
            }
            return parameters.ToArray();
        }

        #region Abstract methods

        protected virtual string InsertSql { get; set; }
        protected virtual string SelectSql { get; set; }
        protected string UpdateSql { get; set; }

        public virtual int Update(TEntity entity)
        {
            throw new NotImplementedException();
        }
        public virtual int UpdateBatch(IEnumerable<TEntity> list)
        {
            throw new NotImplementedException();
        }
        protected virtual TEntity CreateEntityFromReader(DbDataReader reader)
        {
            throw new NotImplementedException();
        }
        protected virtual DbParameter[] CreateParameters(TEntity entity)
        {
            throw new NotImplementedException();
        }
        public virtual TEntity SelectById(object id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Insert Methods

        public virtual int ExecuteNonQuery(string sql, params QueryParam[] filters)
        {
            try
            {
                return dataAccess.ExecuteNonQuery(sql, GetParameters(filters));
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Couldn't Execute SQL state", ex);
            }
        }

        public int ExecuteUpdate(TEntity entity, QueryParam[] filters)
        {
            try
            {
                using (DbConnection connection = dataAccess.CreateNewConnection())
                {
                    return ExecuteUpdate(connection, entity, filters);
                }
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Couldn't save data into data store", ex);
            }
        }


        internal int ExecuteUpdate(DbConnection connection, TEntity entity, QueryParam[] filters)
        {
            int executed = 0;
            string update = AppendWhere(UpdateSql, filters);
            List<DbParameter> mixedParams = new List<DbParameter>();

            mixedParams.AddRange(GetParameters(filters));
            mixedParams.AddRange(CreateParameters(entity));
            executed = dataAccess.ExecuteNonQuery(connection, update, mixedParams.ToArray());
            return executed;
        }

        public virtual int Update(TEntity entity, QueryParam[] filters)
        {
            try
            {
                int executed = 0;
                using (DbConnection connection = dataAccess.CreateNewConnection())
                {

                    executed = ExecuteUpdate(connection, entity, filters);

                }
                return executed;
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Couldn't save data into data store", ex);
            }
        }

        public virtual int InsertBatch(IEnumerable<TEntity> batch)
        {
            try
            {
                int executed = 0;
                using (DbConnection connection = dataAccess.CreateNewConnection())
                {

                    using (DbTransaction dbTrans = connection.BeginTransaction())
                    {
                        foreach (var entity in batch)
                        {
                            executed += dataAccess.ExecuteNonQuery(connection, InsertSql, CreateParameters(entity));
                        }
                        dbTrans.Commit();
                    }
                }
                return executed;
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Couldn't save data into data store", ex);
            }
        }

        public virtual int Insert(TEntity entity)
        {
            try
            {
                int executed = 0;
                using (DbConnection connection = dataAccess.CreateNewConnection())
                {

                    executed = dataAccess.ExecuteNonQuery(connection, InsertSql, CreateParameters(entity));
                }
                return executed;
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Couldn't save data into data store", ex);
            }
        }

        #endregion

        #region Query Methods

        string AppendWhere(string queryString, QueryParam[] filters)
        {
            StringBuilder sql = new StringBuilder(queryString);
            if ((filters != null) && (filters.Length > 0))
            {
                sql.Append(" WHERE ");
                List<string> query = new List<string>();
                foreach (var filter in filters)
                {
                    query.Add(string.Format("{0} = @{0} ", filter.Name));
                }
                sql.Append(string.Join(" and ", query.ToArray()));
            }
            return sql.ToString();
        }

        protected virtual IList<TEntity> SelectInternal(string sql, params DbParameter[] parameters)
        {
            List<TEntity> list = new List<TEntity>();
            using (DbConnection conn = dataAccess.CreateNewConnection())
            {
                try
                {
                    using (var dataReader = dataAccess.ExecuteReader(conn, sql, parameters))
                    {
                        while (dataReader.Read())
                        {
                            var ent = CreateEntityFromReader(dataReader);
                            list.Add(ent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DataAccessException("Couldn't read data from data store.", ex);
                }

            }
            return list;
        }

        public virtual IList<TEntity> SelectAll(params QueryParam[] filters)
        {
            var sql = AppendWhere(SelectSql, filters);
            var parameters = GetParameters(filters);
            return SelectInternal(sql.ToString(), parameters);
        }

        public virtual IList<TEntity> FindAll(string sql, params QueryParam[] filters)
        {
            var parameters = GetParameters(filters);
            return SelectInternal(sql, parameters);
        }

        public virtual TEntity FindOne(QueryParam filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            var values = SelectAll(new QueryParam[] { filter });
            return values.FirstOrDefault();
        }

        #endregion

        #region IDataAccessAdapter<TEntity> Members


        public IList<TEntity> SelectAll()
        {
            throw new NotImplementedException();
        }

        public IList<TEntity> SelectAll(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public IList<TEntity> FindAll(params QueryParam[] filters)
        {
            throw new NotImplementedException();
        }

        public IList<TEntity> FindAll(int pageIndex, int pageSize, out int totalRecords, params QueryParam[] filters)
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

        #region IDataAccessAdapter<TEntity> Members


        public IList<TEntity> FindAll(string sqlQuery)
        {
            throw new NotImplementedException();
        }

        public TEntity FindOne(QueryParam filter, params QueryParam[] filters)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

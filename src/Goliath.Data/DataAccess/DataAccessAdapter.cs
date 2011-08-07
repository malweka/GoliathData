using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Goliath.Data.DataAccess;

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
        IDbConnection dbConnection;
        IEntitySerializerFactory serializerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessAdapter&lt;TEntity&gt;"/> class.
        /// </summary>
        /// <param name="dataAccess">The data access.</param>
        public DataAccessAdapter(IEntitySerializerFactory serializerFactory, IDbAccess dataAccess, IDbConnection dbConnection)
        {
            if (dataAccess == null)
                throw new ArgumentNullException("dataAccess");
            this.serializerFactory = serializerFactory;
            this.dataAccess = dataAccess;
            this.dbConnection = dbConnection;
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


        #region IDataAccessAdapter<TEntity> Members

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

        public int InsertBatch(IEnumerable<TEntity> batch)
        {
            throw new NotImplementedException();
        }

        public int Insert(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public IList<TEntity> FindAll(string sqlQuery)
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

        public TEntity FindOne(QueryParam filter, params QueryParam[] filters)
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
    }
}

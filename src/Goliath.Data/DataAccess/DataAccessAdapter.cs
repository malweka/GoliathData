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
        IEntitySerializerFactory serializerFactory;
        static ILogger logger;

        static DataAccessAdapter()
        {
            logger = Logger.GetLogger(typeof(DataAccessAdapter<>));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessAdapter&lt;TEntity&gt;"/> class.
        /// </summary>
        /// <param name="dataAccess">The data access.</param>
        public DataAccessAdapter(IEntitySerializerFactory serializerFactory, IDbAccess dataAccess, DbConnection dbConnection)
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
            Type type = typeof(TEntity);
            var map = ConfigManager.CurrentSettings.Map;
            var entityMap = map.EntityConfigs[type.FullName];
            if (entityMap == null)
                throw new DataAccessException("entity {0} not mapped.", type.FullName);

            var qInfo = serializerFactory.Deserialize(entityMap, entity);
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();

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

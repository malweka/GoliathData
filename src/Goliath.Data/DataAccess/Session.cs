using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Goliath.Data.Diagnostics;
using Goliath.Data.Sql;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.DataAccess
{
    [Serializable]
    class Session : ISession
    {
        static ILogger logger;
        readonly string id;
        ITransaction currentTransaction;
        readonly SqlCommandRunner commandRunner = new SqlCommandRunner();
        readonly IMappedStatementParser statementParser = new StatementMapParser();
        public IConnectionManager ConnectionManager { get; private set; }
        public ISessionFactory SessionFactory { get; private set; }

        #region .Ctor

        static Session()
        {
            logger = Logger.GetLogger(typeof(Session));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        /// <param name="connectionManager">The connection manager.</param>
        public Session(ISessionFactory sessionFactory, IConnectionManager connectionManager)
        {
            if (sessionFactory == null) throw new ArgumentNullException("sessionFactory");
            if (connectionManager == null) throw new ArgumentNullException("connectionManager");

            id = Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower();
            SessionFactory = sessionFactory;
            ConnectionManager = connectionManager;
        }

        void DisposeOfConnection(IDbConnection connection)
        {
            if (connection.State == ConnectionState.Broken)
                connection.Close();

            connection.Dispose();
        }

        #endregion

        #region Properties

        public string Id
        {
            get { return id; }
        }

        public IDbAccess DataAccess
        {
            get { return SessionFactory.DbSettings.DbAccess; }
        }

        public ITransaction CurrentTransaction
        {
            get
            {
                //if (currentTransaction == null)
                //    currentTransaction = new AdoTransaction(this);
                return currentTransaction;
            }
        }
        #endregion

        #region Data Access

        public IQueryBuilder<T> SelectAll<T>()
        {
            var queryBuilder = new QueryBuilder<T>(this);
            return queryBuilder;
        }

        public ITableNameBuilder SelectAll()
        {
            var columnNames = new List<string>();
            return new QueryBuilder(this, columnNames);
        }

        public IQueryBuilder<T> Select<T>(string propertyName, params string[] propertyNames)
        {
            throw new NotImplementedException();
        }

        public ITableNameBuilder Select(string column, params string[] columns)
        {
            var columnNames = new List<string>();
            columnNames.Add(column);
            if ((columns != null) && (columns.Length > 0))
            {
                columnNames.AddRange(columns);
            }

            return new QueryBuilder(this, columnNames);
        }

        #endregion

        #region Run command implementation

        public IList<T> RunList<T>(SqlQueryBody sql, int limit, int offset, params QueryParam[] paramArray)
        {
            return commandRunner.RunList<T>(this, sql, limit, offset, paramArray);
        }

        public IList<T> RunList<T>(SqlQueryBody sql, int limit, int offset, out long total, params QueryParam[] paramArray)
        {
            return commandRunner.RunList<T>(this, sql, limit, offset, out total, paramArray);
        }

        public IList<T> RunList<T>(string sql, params QueryParam[] paramArray)
        {
            return commandRunner.RunList<T>(this, sql, paramArray);
        }
        public IList<T> RunList<T>(SqlQueryBody sql, params QueryParam[] paramArray)
        {
            return commandRunner.RunList<T>(this, sql, paramArray);
        }

        public T Run<T>(string sql, params QueryParam[] paramArray)
        {
            return commandRunner.Run<T>(this, sql, paramArray);
        }

        public T Run<T>(SqlQueryBody sql, params QueryParam[] paramArray)
        {
            return commandRunner.Run<T>(this, sql, paramArray);
        }

        public int RunNonQuery(string sql, params QueryParam[] paramArray)
        {
            return commandRunner.ExecuteNonQuery(this, sql, paramArray);
        }

        #endregion

        #region Run Mapped Statements

        public T RunMappedStatement<T>(string statementName, params QueryParam[] paramArray)
        {
            return RunMappedStatement<T>(statementName, paramArray, new object[] { });
        }

        public T RunMappedStatement<T>(string statementName, QueryParam[] paramArray, params object[] inputParams)
        {
            try
            {
                var statementRunner = new MappedStatementRunner(statementParser);
                T returnValue;
                if (inputParams != null && inputParams.Length > 0)
                {
                    returnValue = statementRunner.RunStatement<T>(this, statementName, paramArray, inputParams);
                }
                else
                {
                    returnValue = statementRunner.RunStatement<T>(this, statementName, paramArray);
                }

                return returnValue;
            }
            catch (GoliathDataException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GoliathDataException(string.Format("Error running mapped statement {0}.", statementName));
            }
        }

        public IList<T> RunListMappedStatement<T>(string statementName, params QueryParam[] paramArray)
        {
            return RunListMappedStatement<T>(statementName, paramArray, new object[] { });
        }

        public IList<T> RunListMappedStatement<T>(string statementName, QueryParam[] paramArray, params object[] inputParams)
        {
            try
            {
                var statementRunner = new MappedStatementRunner(statementParser);
                if (inputParams != null && inputParams.Length > 0)
                {
                    return statementRunner.RunListStatement<T>(this, statementName, paramArray, inputParams);
                }
                else
                {
                    return statementRunner.RunListStatement<T>(this, statementName, paramArray);
                }
            }
            catch (GoliathDataException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GoliathDataException(string.Format("Error running mapped statement {0}.", statementName));
            }
        }

        public int RunNonQueryMappedStatement(string statementName, params QueryParam[] paramArray)
        {
            return RunNonQueryMappedStatement(statementName, paramArray, new object[] { });
        }

        public int RunNonQueryMappedStatement(string statementName, QueryParam[] paramArray, params object[] inputParams)
        {
            try
            {
                var statementRunner = new MappedStatementRunner(statementParser);
                if (inputParams != null && inputParams.Length > 0)
                {
                    return statementRunner.RunNonQueryMappedStatement(this, statementName, paramArray, inputParams);
                }
                else
                {
                    return statementRunner.RunNonQueryMappedStatement(this, statementName, paramArray);
                }
            }
            catch (GoliathDataException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GoliathDataException(string.Format("Error running mapped statement {0}.", statementName));
            }
        }

        #endregion

        #region IDataStore Members

        public IDataAccessAdapter<TEntity> GetEntityDataAdapter<TEntity>()
        {
            var adapterFactory = SessionFactory.AdapterFactory;
            return adapterFactory.Create<TEntity>(SessionFactory.DbSettings.DbAccess, this);
        }

        #endregion

        #region ISqlInterface Members

        public int Insert<T>(T entity)
        {
            var adapter = GetEntityDataAdapter<T>();
            return adapter.Insert(entity);
        }

        public int Insert<T>(string tableName, T entity)
        {
            var entityMap = new DynamicEntityMap(tableName, tableName, typeof(T));
            return Insert(entityMap, entity);
        }

        public int Insert<T>(EntityMap entityMap, T entity)
        {
            var insert = new InsertSqlBuilder();
            var execList = insert.Build(entity, entityMap, this);
            return execList.Execute(this);
        }

        public int Update<T>(T entity)
        {
            var adapter = GetEntityDataAdapter<T>();
            return adapter.Update(entity);
        }

        public UpdateSqlBuilder<T> Update<T>(string tableName, T entity)
        {
            var entityMap = new DynamicEntityMap(tableName, tableName, typeof(T));
            return Update(entityMap, entity);
        }

        public UpdateSqlBuilder<T> Update<T>(EntityMap entityMap, T entity)
        {
            var builder = new UpdateSqlBuilder<T>(this, entityMap, entity);
            return builder;
        }

        public int Delete<T>(T entity)
        {
            var adapter = GetEntityDataAdapter<T>();
            return adapter.Delete(entity);
        }

        public DeleteSqlBuilder<T> Delete<T>(string tableName, T entity)
        {
            var entityMap = new DynamicEntityMap(tableName, tableName, typeof(T));
            return Delete(entityMap, entity);
        }

        public DeleteSqlBuilder<T> Delete<T>(EntityMap entityMap, T entity)
        {
            var builder = new DeleteSqlBuilder<T>(this, entityMap, entity);
            return builder;
        }

        #endregion

        #region Transactions

        public ITransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.Unspecified);
        }

        public ITransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            if ((currentTransaction == null) || (!currentTransaction.IsStarted))
            {
                currentTransaction = new AdoTransaction(this);
            }
            else if (currentTransaction.IsStarted)
            {
                //TODO: throw exception here?
                return currentTransaction;
            }

            currentTransaction.Begin(isolationLevel);
            //TODO: set transaction lock on connection manager??
            return currentTransaction;
        }

        public ITransaction CommitTransaction()
        {
            if ((currentTransaction == null) || (!currentTransaction.IsStarted))
                throw new GoliathDataException("no transaction or not started yet");

            currentTransaction.Commit();
            currentTransaction.Dispose();

            ITransaction transRef = currentTransaction;
            currentTransaction = null;
            return transRef;
        }

        public ITransaction RollbackTransaction()
        {
            if ((currentTransaction == null) || (!currentTransaction.IsStarted))
                throw new GoliathDataException("no transaction or not started yet");

            currentTransaction.Rollback();
            currentTransaction.Dispose();

            ITransaction transRef = currentTransaction;
            currentTransaction = null;
            return transRef;
        }

        #endregion

        public void Close()
        {
            if (ConnectionManager.HasOpenConnection)
                ConnectionManager.CloseConnection();
        }


        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}

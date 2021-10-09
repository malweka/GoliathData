using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using Goliath.Data.Sql;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;
using Goliath.Security;

namespace Goliath.Data.DataAccess
{

    class Session : ISession
    {
        static ILogger logger;
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
            if (sessionFactory == null) throw new ArgumentNullException(nameof(sessionFactory));
            if (connectionManager == null) throw new ArgumentNullException(nameof(connectionManager));
            UniqueLongGenerator uniqueIdGenerator = new UniqueLongGenerator();
            Id = uniqueIdGenerator.GetNextId();
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

        public long Id { get; }

        public IDbAccess DataAccess => SessionFactory.DbSettings.DbAccess;

        public ITransaction CurrentTransaction { get; private set; }

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
            var columnNames = new List<string> {column};

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

        public IList<T> RunList<T>(string sql,TableQueryMap queryMap, params QueryParam[] paramArray)
        {
            return commandRunner.RunList<T>(this, sql,queryMap, paramArray);
        }
        public IList<T> RunList<T>(SqlQueryBody sql, params QueryParam[] paramArray)
        {
            return commandRunner.RunList<T>(this, sql, paramArray);
        }

        public T Run<T>(string sql,TableQueryMap queryMap, params QueryParam[] paramArray)
        {
            return commandRunner.Run<T>(this, sql,queryMap, paramArray);
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

            catch (Exception ex)
            {
                if (ex is GoliathDataException)
                    throw;

                throw new GoliathDataException($"Mapped statement {statementName} execution Failed. {ex.Message}", ex);
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
            catch (Exception ex)
            {
                if (ex is GoliathDataException)
                    throw;

                throw new GoliathDataException($"Mapped statement {statementName} execution Failed. {ex.Message}", ex);
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
            catch (Exception ex)
            {
                if (ex is GoliathDataException)
                    throw;

                throw new GoliathDataException($"Mapped statement {statementName} execution Failed. {ex.Message}", ex);
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
            return UpdateInternal(entityMap, entity);
        }

        UpdateSqlBuilder<T> UpdateInternal<T>(EntityMap entityMap, T entity)
        {
            var builder = new UpdateSqlBuilder<T>(this, entityMap, entity);
            return builder;
        }

        public UpdateSqlBuilder<T> UpdateQuery<T>(T entity)
        {
            var entityMap = SessionFactory.DbSettings.Map.GetEntityMap(typeof(T).FullName);
            return UpdateInternal(entityMap, entity);
        }

        public int Delete<T>(T entity)
        {
            var adapter = GetEntityDataAdapter<T>();
            return adapter.Delete(entity);
        }

        public DeleteSqlBuilder<T> Delete<T>(string tableName, T entity)
        {
            var entityMap = new DynamicEntityMap(tableName, tableName, typeof(T));
            return DeleteInternal(entityMap, entity);
        }

        public DeleteSqlBuilder<T> DeleteQuery<T>(T entity)
        {
            var entityMap = SessionFactory.DbSettings.Map.GetEntityMap(typeof(T).FullName);
            return DeleteInternal(entityMap, entity);
        }

         DeleteSqlBuilder<T> DeleteInternal<T>(EntityMap entityMap, T entity)
        {
            var builder = new DeleteSqlBuilder<T>(this, entityMap, entity);
            return builder;
        }

        #endregion

        #region Transactions

        public ITransaction BeginTransaction()
        {
            return BeginTransaction(SessionFactory.DbSettings.Connector.DefaultIsolationLevel);
        }

        public ITransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            if ((CurrentTransaction == null) || (!CurrentTransaction.IsStarted))
            {
                CurrentTransaction = new AdoTransaction(this);
            }
            else if (CurrentTransaction.IsStarted)
            {
                //TODO: throw exception here?
                return CurrentTransaction;
            }

            CurrentTransaction.Begin(isolationLevel);
            //TODO: set transaction lock on connection manager??
            return CurrentTransaction;
        }

        public ITransaction CommitTransaction()
        {
            if ((CurrentTransaction == null) || (!CurrentTransaction.IsStarted))
                throw new GoliathDataException("No transaction or not started yet.");

            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();

            ITransaction transRef = CurrentTransaction;
            CurrentTransaction = null;
            return transRef;
        }

        public ITransaction RollbackTransaction()
        {
            if ((CurrentTransaction == null) || (!CurrentTransaction.IsStarted))
                return null;

            CurrentTransaction.Rollback();
            CurrentTransaction.Dispose();

            ITransaction transRef = CurrentTransaction;
            CurrentTransaction = null;
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

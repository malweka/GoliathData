using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using Goliath.Data.Diagnostics;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class DbAccess : IDbAccess
    {
        #region properties and variables

        static readonly ILogger logger;
        readonly IDbConnector dbConnector;

        int? CommandTimeout
        {
            get
            {
                return dbConnector.CommandTimeout;
            }
        }

        #endregion

        #region .ctors

        static DbAccess()
        {
            logger = Logger.GetLogger(typeof(DbAccess));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbAccess"/> class.
        /// </summary>
        /// <param name="dbConnector">The db connector.</param>
        public DbAccess(IDbConnector dbConnector)
        {
            if (dbConnector == null) throw new ArgumentNullException(nameof(dbConnector));

            this.dbConnector = dbConnector;
        }

        #endregion

        #region Data access methods

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(DbConnection conn, string sql, params QueryParam[] parameters)
        {
            logger.Log(LogLevel.Debug, sql);
            return ExecuteNonQuery(conn, null, sql, parameters);
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(DbConnection conn, ITransaction transaction, string sql, params QueryParam[] parameters)
        {
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = conn;
                if (CommandTimeout != null)
                    cmd.CommandTimeout = CommandTimeout.Value;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var dbParam = dbConnector.CreateParameter(parameters[i].Name, parameters[i].Value, parameters[i].PropertyDbType);
                        cmd.Parameters.Add(dbParam);
                    }
                }

                transaction?.Enlist(cmd);

                logger.Log(LogLevel.Debug, sql);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public object ExecuteScalar(DbConnection conn, string sql, params QueryParam[] parameters)
        {
            return ExecuteScalar(conn, null, sql, parameters);
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public object ExecuteScalar(DbConnection conn, ITransaction transaction, string sql, params QueryParam[] parameters)
        {
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = conn;
                if (CommandTimeout != null)
                    cmd.CommandTimeout = CommandTimeout.Value;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var dbParam = dbConnector.CreateParameter(parameters[i].Name, parameters[i].Value, parameters[i].PropertyDbType);
                        cmd.Parameters.Add(dbParam);
                    }
                }

                if (transaction != null)
                    transaction.Enlist(cmd);

                logger.Log(LogLevel.Debug, sql);
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DbDataReader ExecuteReader(DbConnection conn, string sql, params QueryParam[] parameters)
        {
            logger.Log(LogLevel.Debug, sql);
            return ExecuteReader(conn, null, sql, parameters);
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DbDataReader ExecuteReader(DbConnection conn, ITransaction transaction, string sql, params QueryParam[] parameters)
        {
            using (DbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = conn;
                if (CommandTimeout != null)
                    cmd.CommandTimeout = CommandTimeout.Value;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var dbParam = dbConnector.CreateParameter(parameters[i].Name, parameters[i].Value, parameters[i].PropertyDbType);
                        cmd.Parameters.Add(dbParam);
                    }
                }

                transaction?.Enlist(cmd);

                logger.Log(LogLevel.Debug, sql);
                return cmd.ExecuteReader();
            }
        }

        /// <summary>
        /// Executes the dynamic.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IList<dynamic> ExecuteDynamic(DbConnection conn, string sql, params QueryParam[] parameters)
        {
            var list = new List<dynamic>();
            using (var dbReader = ExecuteReader(conn, null, sql, parameters))
            {
                if (dbReader.FieldCount <= 0) return list;

                var readerColumnOrder = GetReaderColumnOrder(dbReader);
                while (dbReader.Read())
                {
                    var entity = HydrateDynamic(dbReader, readerColumnOrder);
                    list.Add(entity);
                }
            }

            return list;
        }

        /// <summary>
        /// Executes the dictionary.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IList<IDictionary<string, object>> ExecuteDictionary(DbConnection conn, string sql, params QueryParam[] parameters)
        {
            var list = new List<IDictionary<string, object>>();
            using (var dbReader = ExecuteReader(conn, null, sql, parameters))
            {
                if (dbReader.FieldCount <= 0) return list;

                var readerColumnOrder = GetReaderColumnOrder(dbReader);
                while (dbReader.Read())
                {
                    var entity = HydrateDictionary(dbReader, readerColumnOrder);
                    list.Add(entity);
                }
            }

            return list;
        }

        /// <summary>
        /// Gets the reader column order.
        /// </summary>
        /// <param name="dbReader">The database reader.</param>
        /// <returns></returns>
        public static Dictionary<string, int> GetReaderColumnOrder(DbDataReader dbReader)
        {
            var readerColumnOrder = new Dictionary<string, int>();
            for (int i = 0; i < dbReader.FieldCount; i++)
            {
                readerColumnOrder.Add(dbReader.GetName(i), i);
            }

            return readerColumnOrder;
        }

        /// <summary>
        /// Hydrates from data reader.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="readerColumnOrder">The reader column order.</param>
        /// <returns></returns>
        public static dynamic HydrateDynamic(DbDataReader dataReader, Dictionary<string, int> readerColumnOrder)
        {
            var dynamicObject = new ExpandoObject() as IDictionary<string, object>;

            foreach (var col in readerColumnOrder)
            {
                var val = dataReader[col.Value];
                if (DBNull.Value == val)
                {
                    val = null;
                }

                dynamicObject.Add(col.Key, val);
            }

            return dynamicObject;
        }

        /// <summary>
        /// Hydrates the dictionary.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="readerColumnOrder">The reader column order.</param>
        /// <returns></returns>
        public static IDictionary<string, object> HydrateDictionary(DbDataReader dataReader, Dictionary<string, int> readerColumnOrder)
        {
            var dataBag = new Dictionary<string, object>();

            foreach (var col in readerColumnOrder)
            {
                var val = dataReader[col.Value];
                if (DBNull.Value == val)
                {
                    val = null;
                }
                dataBag.Add(col.Key, val);
            }

            return dataBag;
        }

        #endregion

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    using Diagnostics;
    using Mapping;
    using Sql;

    /// <summary>
    /// 
    /// </summary>
    public class SqlCommandRunner
    {
        static ILogger logger;

        static SqlCommandRunner()
        {
            logger = Logger.GetLogger(typeof(SqlCommandRunner));
        }

        internal T ExecuteScalar<T>(DbConnection dbConn, ISession session, string sql, params QueryParam[] paramArray)
        {
            var result = session.DataAccess.ExecuteScalar(dbConn, session.CurrentTransaction, sql, paramArray);

            if (result != null)
            {
                var converter = session.SessionFactory.DbSettings.ConverterStore.GetConverterFactoryMethod(typeof(T));
                var res = converter.Invoke(result);
                if (res != null)
                    return (T)res;
            }

            return default(T);
        }

        internal IList<T> ExecuteReaderPrimitive<T>(DbConnection dbConn, ISession session, string sql, params QueryParam[] paramArray)
        {
            var reader = session.DataAccess.ExecuteReader(dbConn, session.CurrentTransaction, sql, paramArray);
            List<T> list = new List<T>();

            while (reader.Read())
            {
                T val = session.SessionFactory.DataSerializer.ReadFieldData<T>(0, reader);
                list.Add(val);
            }

            reader.Dispose();
            return list;
        }

        internal IList<T> ExecuteReader<T>(IEntityMap entMap, DbConnection dbConn, ISession session, string sql, params QueryParam[] paramArray)
        {
            var dataReader = session.DataAccess.ExecuteReader(dbConn, session.CurrentTransaction, sql, paramArray);
            var entities = session.SessionFactory.DataSerializer.SerializeAll<T>(dataReader, entMap);

            dataReader.Dispose();
            return entities;
        }

        internal IList<T> ExecuteReader<T>(IEntityMap entMap, DbConnection dbConn, ISession session, SqlQueryBody sql, int limit, int offset, params QueryParam[] paramArray)
        {
            var dataReader = session.DataAccess.ExecuteReader(dbConn, session.CurrentTransaction, sql.ToString(session.SessionFactory.DbSettings.SqlDialect, new PagingInfo() { Limit = limit, Offset = offset }), paramArray);
            var entities = session.SessionFactory.DataSerializer.SerializeAll<T>(dataReader, entMap);

            dataReader.Dispose();
            return entities;
        }

        string BuildCountSql(ISession session, SqlQueryBody sql, int limit, int offset)
        {
            var dialect = session.SessionFactory.DbSettings.SqlDialect;

            var countFunction = dialect.GetFunction(FunctionNames.Count);
            if (countFunction == null)
                throw new GoliathDataException(string.Format("Count function is either not registerd for provider {0} or is not supported",
                    dialect.DatabaseProviderName));

            StringBuilder countSql = new StringBuilder(string.Format("SELECT {0} AS ctn_TOTAL ", countFunction.ToSqlStatement()));
            countSql.AppendFormat("FROM {0} ", sql.From);
            if (!string.IsNullOrWhiteSpace(sql.JoinEnumeration))
                countSql.Append(sql.JoinEnumeration);
            if (!string.IsNullOrWhiteSpace(sql.WhereExpression))
                countSql.AppendFormat(" WHERE {0}", sql.WhereExpression);

            return countSql.ToString();
        }

        internal IList<T> ExecuteReaderPrimitive<T>(DbConnection dbConn, ISession session, SqlQueryBody sql, int limit, int offset, out long total, params QueryParam[] paramArray)
        {
            total = 0;
            var dialect = session.SessionFactory.DbSettings.SqlDialect;
            var serializer = session.SessionFactory.DataSerializer;

            string countSql = BuildCountSql(session, sql, limit, offset);

            string sqlWithPaging = sql.ToString(dialect, new PagingInfo() { Limit = limit, Offset = offset });

            string sqlToRun = string.Format("{0};\n\r{1};", countSql, sqlWithPaging);

            var dataReader = session.DataAccess.ExecuteReader(dbConn, session.CurrentTransaction, sqlToRun, paramArray);
            //First resultset contains the count
            while (dataReader.Read())
            {
                total = serializer.ReadFieldData<long>(0, dataReader);
                //we only expect 1 row of data to be returned, so let's break out of the loop.
                break;
            }

            //move to the next result set which contains the entities
            dataReader.NextResult();
            List<T> list = new List<T>();
            while (dataReader.Read())
            {
                T val = serializer.ReadFieldData<T>(0, dataReader);
                list.Add(val);
            }

            dataReader.Dispose();
            return list;
        }

        internal IList<T> ExecuteReader<T>(IEntityMap entMap, DbConnection dbConn, ISession session, SqlQueryBody sql, int limit, int offset, out long total, params QueryParam[] paramArray)
        {
            total = 0;
            var dialect = session.SessionFactory.DbSettings.SqlDialect;
            var serializer = session.SessionFactory.DataSerializer;

            string countSql = BuildCountSql(session, sql, limit, offset);

            string sqlWithPaging = sql.ToString(dialect, new PagingInfo() { Limit = limit, Offset = offset });

            string sqlToRun = string.Format("{0};\n\r{1};", countSql, sqlWithPaging);

            var dataReader = session.DataAccess.ExecuteReader(dbConn, session.CurrentTransaction, sqlToRun, paramArray);
            //First resultset contains the count
            while (dataReader.Read())
            {
                total = serializer.ReadFieldData<long>(0, dataReader);
                //we only expect 1 row of data to be returned, so let's break out of the loop.
                break;
            }

            //move to the next result set which contains the entities
            dataReader.NextResult();
            var entities = serializer.SerializeAll<T>(dataReader, entMap);

            dataReader.Dispose();
            return entities;
        }

        /// <summary>
        /// Runs the statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="paramArray">The param array.</param>
        /// <returns></returns>
        public T Run<T>(ISession session, SqlQueryBody sql, params QueryParam[] paramArray)
        {
            return Run<T>(session, sql.ToString(), paramArray);
        }

        /// <summary>
        /// Runs the statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="paramArray">The param array.</param>
        /// <returns></returns>
        public T Run<T>(ISession session, string sql, params QueryParam[] paramArray)
        {
            Type instanceType = typeof(T);
            bool ownTransaction = false;
            var dbConn = session.ConnectionManager.OpenConnection();
            T value = default(T);

            if ((session.CurrentTransaction == null) || !session.CurrentTransaction.IsStarted)
            {
                session.BeginTransaction();
                ownTransaction = true;
            }

            try
            {
                if (instanceType.IsPrimitive)
                {
                    value = ExecuteScalar<T>(dbConn, session, sql, paramArray);
                }
                else
                {
                    MapConfig map = session.SessionFactory.DbSettings.Map;
                    EntityMap entMap;
                    ComplexType complexType;

                    if (map.EntityConfigs.TryGetValue(instanceType.FullName, out entMap))
                    {
                        value = ExecuteReader<T>(entMap, dbConn, session, sql, paramArray).FirstOrDefault();
                    }
                    else if (map.ComplexTypes.TryGetValue(instanceType.FullName, out complexType))
                    {
                        value = ExecuteReader<T>(complexType, dbConn, session, sql, paramArray).FirstOrDefault();
                    }
                    else
                    {
                        //Build a dynamic entity
                        DynamicEntityMap dynEntMap = new DynamicEntityMap(instanceType);
                        value = ExecuteReader<T>(dynEntMap, dbConn, session, sql, paramArray).FirstOrDefault();
                    }
                }

                if (ownTransaction)
                    session.CommitTransaction();

                return value;
            }
            catch (GoliathDataException ex)
            {
                logger.Log(LogLevel.Debug, string.Format("Goliath Exception found {0} ", ex.Message));
                throw;
            }
            catch (Exception ex)
            {
                throw new GoliathDataException(string.Format("Exception while inserting: {0}", sql), ex);
            }
        }

        /// <summary>
        /// Runs the statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="paramArray">The param array.</param>
        /// <returns></returns>
        public IList<T> RunList<T>(ISession session, SqlQueryBody sql, int limit, int offset, params QueryParam[] paramArray)
        {
            Type instanceType = typeof(T);
            bool ownTransaction = false;
            var dbConn = session.ConnectionManager.OpenConnection();
            IList<T> list = new List<T>();
            var dialect = session.SessionFactory.DbSettings.SqlDialect;
            var serializer = session.SessionFactory.DataSerializer;

            if ((session.CurrentTransaction == null) || !session.CurrentTransaction.IsStarted)
            {
                session.BeginTransaction();
                ownTransaction = true;
            }

            try
            {
                if (instanceType.IsPrimitive)
                {
                    list = ExecuteReaderPrimitive<T>(dbConn, session, sql.ToString(dialect, new PagingInfo() { Limit = limit, Offset = offset }), paramArray);
                }
                else
                {
                    MapConfig map = session.SessionFactory.DbSettings.Map;
                    EntityMap entMap;
                    ComplexType complexType;

                    if (map.EntityConfigs.TryGetValue(instanceType.FullName, out entMap))
                    {
                        list = ExecuteReader<T>(entMap, dbConn, session, sql, limit, offset, paramArray);
                    }
                    else if (map.ComplexTypes.TryGetValue(instanceType.FullName, out complexType))
                    {
                        list = ExecuteReader<T>(complexType, dbConn, session, sql, limit, offset, paramArray);
                    }
                    else
                    {
                        //Build a dynamic entity
                        DynamicEntityMap dynEntMap = new DynamicEntityMap(instanceType);
                        list = ExecuteReader<T>(dynEntMap, dbConn, session, sql, limit, offset, paramArray);
                    }
                }

                if (ownTransaction)
                    session.CommitTransaction();

                return list;
            }
            catch (GoliathDataException ex)
            {
                logger.Log(LogLevel.Debug, string.Format("Goliath Exception found {0} ", ex.Message));
                throw;
            }
            catch (Exception ex)
            {
                throw new GoliathDataException(string.Format("Exception while inserting: {0}", sql), ex);
            }
        }


        /// <summary>
        /// Runs the statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="total">The total.</param>
        /// <param name="paramArray">The param array.</param>
        /// <returns></returns>
        public IList<T> RunList<T>(ISession session, SqlQueryBody sql, int limit, int offset, out long total, params QueryParam[] paramArray)
        {
            total = 0;
            Type instanceType = typeof(T);
            bool ownTransaction = false;
            var dbConn = session.ConnectionManager.OpenConnection();
            IList<T> list = new List<T>();
            var dialect = session.SessionFactory.DbSettings.SqlDialect;
            var serializer = session.SessionFactory.DataSerializer;

            if ((session.CurrentTransaction == null) || !session.CurrentTransaction.IsStarted)
            {
                session.BeginTransaction();
                ownTransaction = true;
            }

            try
            {
                if (instanceType.IsPrimitive)
                {
                    list = ExecuteReaderPrimitive<T>(dbConn, session, sql, limit, offset, out total, paramArray);
                }
                else
                {
                    MapConfig map = session.SessionFactory.DbSettings.Map;
                    EntityMap entMap;
                    ComplexType complexType;

                    if (map.EntityConfigs.TryGetValue(instanceType.FullName, out entMap))
                    {
                        list = ExecuteReader<T>(entMap, dbConn, session, sql, limit, offset, out total, paramArray);
                    }
                    else if (map.ComplexTypes.TryGetValue(instanceType.FullName, out complexType))
                    {
                        list = ExecuteReader<T>(complexType, dbConn, session, sql, limit, offset, out total, paramArray);
                    }
                    else
                    {
                        //Build a dynamic entity
                        DynamicEntityMap dynEntMap = new DynamicEntityMap(instanceType);
                        list = ExecuteReader<T>(dynEntMap, dbConn, session, sql, limit, offset, out total, paramArray);
                    }
                }

                if (ownTransaction)
                    session.CommitTransaction();

                return list;
            }
            catch (GoliathDataException ex)
            {
                logger.Log(LogLevel.Debug, string.Format("Goliath Exception found {0} ", ex.Message));
                throw;
            }
            catch (Exception ex)
            {
                throw new GoliathDataException(string.Format("Exception while inserting: {0}", sql), ex);
            }
        }

        public IList<T> RunList<T>(ISession session, SqlQueryBody sql, params QueryParam[] paramArray)
        {
            return RunList<T>(session, sql.ToString(), paramArray);
        }

        /// <summary>
        /// Runs the statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="paramArray">The param array.</param>
        /// <returns></returns>
        public IList<T> RunList<T>(ISession session, string sql, params QueryParam[] paramArray)
        {
            Type instanceType = typeof(T);
            bool ownTransaction = false;
            var dbConn = session.ConnectionManager.OpenConnection();
            IList<T> list = new List<T>();

            if ((session.CurrentTransaction == null) || !session.CurrentTransaction.IsStarted)
            {
                session.BeginTransaction();
                ownTransaction = true;
            }

            try
            {
                if (instanceType.IsPrimitive)
                {
                    list = ExecuteReaderPrimitive<T>(dbConn, session, sql, paramArray);
                }
                else
                {
                    MapConfig map = session.SessionFactory.DbSettings.Map;
                    EntityMap entMap;
                    ComplexType complexType;

                    if (map.EntityConfigs.TryGetValue(instanceType.FullName, out entMap))
                    {
                        list = ExecuteReader<T>(entMap, dbConn, session, sql, paramArray);
                    }
                    else if (map.ComplexTypes.TryGetValue(instanceType.FullName, out complexType))
                    {
                        list = ExecuteReader<T>(complexType, dbConn, session, sql, paramArray);
                    }
                    else
                    {
                        //Build a dynamic entity
                        DynamicEntityMap dynEntMap = new DynamicEntityMap(instanceType);
                        list = ExecuteReader<T>(dynEntMap, dbConn, session, sql, paramArray);
                    }
                }

                if (ownTransaction)
                    session.CommitTransaction();

                return list;
            }
            catch (GoliathDataException ex)
            {
                logger.Log(LogLevel.Debug, string.Format("Goliath Exception found {0} ", ex.Message));
                throw;
            }
            catch (Exception ex)
            {
                throw new GoliathDataException(string.Format("Exception while inserting: {0}", sql), ex);
            }
        }
    }
}

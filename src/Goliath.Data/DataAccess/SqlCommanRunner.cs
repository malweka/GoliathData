using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    using Diagnostics;
    using Mapping;

    /// <summary>
    /// 
    /// </summary>
    public class SqlCommanRunner
    {
        static ILogger logger;

        static SqlCommanRunner()
        {
            logger = Logger.GetLogger(typeof(SqlCommanRunner));
        }

        internal T ExecuteScalar<T>(Type instanceType, DbConnection dbConn, ISession session,  string sql, params QueryParam[] paramArray)
        {
            var result = session.DataAccess.ExecuteScalar(dbConn, session.CurrentTransaction, sql, paramArray);
            if (result != null)
            {
                var converter = session.SessionFactory.DbSettings.ConverterStore.GetConverterFactoryMethod(instanceType);
                var res = converter.Invoke(result);
                if (res != null)
                    return (T)res;
            }

            return default(T);
        }

        internal T ExecuteReader<T>(IEntityMap entMap, Type instanceType, DbConnection dbConn, ISession session,  string sql, params QueryParam[] paramArray)
        {
            var result = session.DataAccess.ExecuteReader(dbConn, session.CurrentTransaction, sql, paramArray);
            var entities = session.SessionFactory.DataSerializer.SerializeAll<T>(result, entMap);
            return entities.FirstOrDefault();
        }

        /// <summary>
        /// Runs the statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="paramArray">The param array.</param>
        /// <returns></returns>
        public T RunStatement<T>(ISession session, string sql, params QueryParam[] paramArray)
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
                    value = ExecuteScalar<T>(instanceType, dbConn, session, sql, paramArray);
                }
                else
                {
                    MapConfig map = session.SessionFactory.DbSettings.Map;
                    EntityMap entMap;
                    ComplexType complexType;

                    if (map.EntityConfigs.TryGetValue(instanceType.FullName, out entMap))
                    {
                        value = ExecuteReader<T>(entMap, instanceType, dbConn, session, sql, paramArray);
                    }
                    else if (map.ComplexTypes.TryGetValue(instanceType.FullName, out complexType))
                    {
                        value = ExecuteReader<T>(complexType, instanceType, dbConn, session, sql, paramArray);
                    }
                    else
                    {
                        //Build a dynamic entity
                        DynamicEntityMap dynEntMap = new DynamicEntityMap(instanceType);
                        value = ExecuteReader<T>(dynEntMap, instanceType, dbConn, session, sql, paramArray);
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
    }
}
 
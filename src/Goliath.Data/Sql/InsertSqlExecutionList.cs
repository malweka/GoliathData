using System;
using System.Collections.Generic;
using System.Linq;
using Goliath.Data.DataAccess;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class InsertSqlExecutionList
    {
        readonly List<InsertSqlInfo> statements = new List<InsertSqlInfo>();
        readonly Dictionary<string, QueryParam> generatedKeys = new Dictionary<string, QueryParam>();

        /// <summary>
        /// Gets the statements.
        /// </summary>
        /// <value>
        /// The statements.
        /// </value>
        public List<InsertSqlInfo> Statements { get { return statements; } }

        /// <summary>
        /// Gets the generated keys.
        /// </summary>
        /// <value>
        /// The generated keys.
        /// </value>
        public Dictionary<string, QueryParam> GeneratedKeys
        {
            get { return generatedKeys; }
        }

        object Execute(ISession session, InsertSqlInfo statement, Type resultType)
        {
            if (resultType == null)
                resultType = typeof(object);

            var sql = statement.ToString(session.SessionFactory.DbSettings.SqlDialect);
            var cmdr = new SqlCommandRunner();
            var parameters = new List<QueryParam>();
            parameters.AddRange(GeneratedKeys.Values);

            foreach (var queryParam in statement.Parameters)
            {
                if (!GeneratedKeys.ContainsKey(queryParam.Key))
                    parameters.Add(queryParam.Value);
            }

            return cmdr.ExecuteScalar(session, sql, resultType, parameters.ToArray());
        }

        int Execute(ISession session, InsertSqlInfo statement)
        {
            var sql = statement.ToString(session.SessionFactory.DbSettings.SqlDialect);
            var cmdr = new SqlCommandRunner();
            var parameters = new List<QueryParam>();
            parameters.AddRange(GeneratedKeys.Values);

            foreach (var queryParam in statement.Parameters)
            {
                if (!GeneratedKeys.ContainsKey(queryParam.Key))
                    parameters.Add(queryParam.Value);
            }

            return cmdr.ExecuteNonQuery(session, sql, parameters.ToArray());
        }

        /// <summary>
        /// Excutes the statement.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="statement">The statement.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <returns></returns>
        internal object ExcuteStatement(ISession session, InsertSqlInfo statement, Type resultType)
        {
            object value = null;

            if (!statement.DelayExecute)
            {
                value = Execute(session, statement, resultType);
                statement.Processed = true;
            }

            Statements.Add(statement);

            return value;
        }

        /// <summary>
        /// Executes all statements
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public int Execute(ISession session)
        {
            var val = 0;

            foreach (var insertSqlInfo in Statements.Where(insertSqlInfo => !insertSqlInfo.Processed))
            {
                val += Execute(session, insertSqlInfo);
                insertSqlInfo.Processed = true;
            }

            return val;
        }
    }
}
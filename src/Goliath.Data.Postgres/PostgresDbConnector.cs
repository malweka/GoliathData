using System;
using System.Data;
using Goliath.Data.Providers;
using Npgsql;

namespace Goliath.Data.Postgres
{
    /// <summary>
    /// 
    /// </summary>
    public class PostgresDbConnector : DbConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDbConnector"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public PostgresDbConnector(string connectionString)
            : base(connectionString, RdbmsBackend.SupportedSystemNames.Postgresql9)
        {
        }

        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
        public override System.Data.Common.DbConnection CreateNewConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            return connection;
        }

        public override bool AllowMultipleConnections => true;

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">parameterName</exception>
        public override System.Data.Common.DbParameter CreateParameter(string parameterName, object value, DbType? dbType)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException(nameof(parameterName));

            if (value == null)
            {
                value = DBNull.Value;
            }
            else
            {
                Type type = value.GetType();
                if (type.IsEnum)
                {
                    if(dbType.HasValue && (dbType == DbType.String || dbType==DbType.StringFixedLength||dbType== DbType.AnsiString || dbType == DbType.AnsiStringFixedLength))
                    {
                        string realValue = value.ToString();
                        return new NpgsqlParameter($"@{parameterName}", realValue);
                    }

                    return new NpgsqlParameter($"@{parameterName}", (int)value);
                }
            }

            var param = new NpgsqlParameter($"@{parameterName}", value);
            return param;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace Goliath.Data.Providers.Sqlite
{
    public class SqliteDbConnector : DbConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDbConnector"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqliteDbConnector(string connectionString) : base(connectionString, Constants.ProviderName)
        {
        }

        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
        public override System.Data.Common.DbConnection CreateNewConnection()
        {
            SQLiteConnection connection;
            connection = new SQLiteConnection(ConnectionString);
            //connection.Open();
            return connection;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override System.Data.Common.DbParameter CreateParameter(string parameterName, object value)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException("parameterName");

            if (value == null)
                value = DBNull.Value;

            SQLiteParameter param = new SQLiteParameter(string.Format("@{0}", parameterName), value);
            return param;
        }
    }
}

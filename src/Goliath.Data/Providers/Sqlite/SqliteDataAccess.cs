using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace Goliath.Data.Providers.Sqlite
{
    public class SqliteDataAccess : DbAccess
    {
        public SqliteDataAccess(string connectionString) : base(Constants.ProviderName)
        {
            ConnectionString = connectionString;
        }

        public override System.Data.Common.DbConnection CreateNewConnection()
        {
            SQLiteConnection connection;
            connection = new SQLiteConnection(ConnectionString);
            //connection.Open();
            return connection;
        }

        public override System.Data.Common.DbParameter CreateParameter(int i, object value)
        {
            return new SQLiteParameter("@" + i, value);
        }

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

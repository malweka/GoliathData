using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Providers.Sqlite
{
    [Serializable]
    public class SqliteProvider : IDbProvider
    {
        #region IDbProvider Members

        public string Name
        {
            get { return Constants.ProviderName; }
        }

        public IDbConnector GetDatabaseConnector(string connectionString)
        {
            return new SqliteDbConnector(connectionString);
        }

        public SqlMapper SqlMapper
        {
            get { return new SqliteSqlMapper(); }
        }

        #endregion
    }
}

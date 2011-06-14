using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Providers.Sqlite
{
    public class SqliteProvider : IDbProvider
    {
        #region IDbProvider Members

        public string Name
        {
            get { return Constants.ProviderName; }
        }

        public IDbAccess GetDataAccess(string connectionString)
        {
            return new SqliteDataAccess(connectionString);
        }

        public SqlMapper SqlMapper
        {
            get { return new SqliteSqlMapper(); }
        }

        #endregion
    }
}

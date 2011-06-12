using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Providers.SqlServer
{
    public class MssqlProvider : IDbProvider
    {
        #region IDbProvider Members

        public string Name
        {
            get { return Constants.ProviderName; }
        }

        public IDbAccess GetDataAccess(string connectionString)
        {
            return new MssqlDataAccess(connectionString);
        }

        public SqlMapper SqlMapper
        {
            get { return new Mssq2008SqlMapper(); }
        }

        #endregion
    }
}

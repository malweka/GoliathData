using System;

namespace Goliath.Data.Providers.SqlServer
{
    [Serializable]
    public class MssqlProvider : IDbProvider
    {
        #region IDbProvider Members

        public string Name
        {
            get { return Constants.ProviderName; }
        }

        public IDbConnector GetDatabaseConnector(string connectionString)
        {
            return new MssqlDbConnector(connectionString);
        }

        public SqlMapper SqlMapper
        {
            get { return new Mssq2008SqlMapper(); }
        }

        #endregion
    }
}

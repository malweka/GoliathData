using System;

namespace Goliath.Data.Providers.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MssqlProvider : IDbProvider
    {
        #region IDbProvider Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return RdbmsBackend.SupportedSystemNames.Mssql2008R2; }
        }

        /// <summary>
        /// Gets the database connector.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public IDbConnector GetDatabaseConnector(string connectionString)
        {
            return new MssqlDbConnector(connectionString);
        }

        /// <summary>
        /// Gets the SQL mapper.
        /// </summary>
        /// <returns></returns>
        public SqlMapper SqlMapper
        {
            get { return new Mssq2008SqlMapper(); }
        }

        #endregion
    }
}

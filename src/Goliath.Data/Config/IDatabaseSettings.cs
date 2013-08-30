using Goliath.Data.DataAccess;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDatabaseSettings
    {
        /// <summary>
        /// Gets the map.
        /// </summary>
        MapConfig Map { get; }

        /// <summary>
        /// Gets the SQL dialect.
        /// </summary>
        /// <value>The SQL dialect.</value>
        SqlDialect SqlDialect { get; }

        /// <summary>
        /// Gets the connector.
        /// </summary>
        /// <value>The connector.</value>
        IDbConnector Connector { get; }

        /// <summary>
        /// Gets the db access.
        /// </summary>
        /// <value>The db access.</value>
        IDbAccess DbAccess { get; }

        /// <summary>
        /// Gets the converter store.
        /// </summary>
        /// <value>The converter store.</value>
        ITypeConverterStore ConverterStore { get; }

        /// <summary>
        /// Creates the accessor.
        /// </summary>
        /// <returns></returns>
        DbAccess CreateAccessor();
    }
}

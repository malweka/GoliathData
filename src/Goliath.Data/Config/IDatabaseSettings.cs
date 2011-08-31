using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDabaseSettings
    {
        Mapping.MapConfig Map { get; }
        /// <summary>
        /// Gets the SQL mapper.
        /// </summary>
        /// <value>The SQL mapper.</value>
        Providers.SqlMapper SqlMapper { get; }
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

        DbAccess CreateAccessor();
    }
}

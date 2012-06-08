﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    using DataAccess;
    using Providers;
    using Mapping;

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
        /// Gets the SQL mapper.
        /// </summary>
        /// <value>The SQL mapper.</value>
        SqlMapper SqlMapper { get; }
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

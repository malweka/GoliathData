using System;
using System.Collections.Generic;
using Goliath.Data.Mapping;

namespace Goliath.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISchemaDescriptor : IDisposable
    {
        /// <summary>
        /// Gets the dialect.
        /// </summary>
        /// <value>
        /// The dialect.
        /// </value>
        SqlDialect Dialect { get; }

        /// <summary>
        /// Gets the name of the database provider.
        /// </summary>
        /// <value>
        /// The name of the database provider.
        /// </value>
        string DatabaseProviderName { get; }

        IList<string> TableWhiteList { get; }

        /// <summary>
        /// Gets the tables.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, EntityMap> GetTables();

        /// <summary>
        /// Gets the views.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, View> GetViews();

        /// <summary>
        /// Gets the stored procs.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, StatementMap> GetStoredProcs();

        /// <summary>
        /// Gets or sets the project settings.
        /// </summary>
        /// <value>
        /// The project settings.
        /// </value>
        ProjectSettings ProjectSettings { get; set; }
    }
}

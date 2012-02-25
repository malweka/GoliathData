using System;
using System.Collections.Generic;

namespace Goliath.Data.Providers
{
    using Mapping;

    /// <summary>
    /// 
    /// </summary>
    public interface ISchemaDescriptor : IDisposable
    {
        string DatabaseProviderName { get; }
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
        IDictionary<string, SqlProcedure> GetStoredProcs();
        /// <summary>
        /// Gets or sets the project settings.
        /// </summary>
        /// <value>
        /// The project settings.
        /// </value>
        ProjectSettings ProjectSettings { get; set; }
    }
}

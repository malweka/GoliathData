using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SchemaDescriptor : ISchemaDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaDescriptor"/> class.
        /// </summary>
        /// <param name="databaseProviderName">Name of the database provider.</param>
        protected SchemaDescriptor(string databaseProviderName)
        {
            DatabaseProviderName = databaseProviderName;
        }

        #region ISchemaDescriptor Members

        /// <summary>
        /// Gets or sets the name of the database provider.
        /// </summary>
        /// <value>The name of the database provider.</value>
        public string DatabaseProviderName { get; private set; }

        /// <summary>
        /// Gets the tables.
        /// </summary>
        /// <returns></returns>
        public abstract IDictionary<string, Mapping.EntityMap> GetTables();

        /// <summary>
        /// Gets the views.
        /// </summary>
        /// <returns></returns>
        public abstract IDictionary<string, Mapping.View> GetViews();

        /// <summary>
        /// Gets the stored procs.
        /// </summary>
        /// <returns></returns>
        public abstract IDictionary<string, Mapping.StoredProcedure> GetStoredProcs();

        /// <summary>
        /// Gets or sets the project settings.
        /// </summary>
        /// <value>The project settings.</value>
        public virtual Mapping.ProjectSettings ProjectSettings { get; set; }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        #endregion
    }
}

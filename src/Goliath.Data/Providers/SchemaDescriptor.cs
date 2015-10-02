using System.Collections.Generic;

namespace Goliath.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SchemaDescriptor : ISchemaDescriptor
    {
        protected string[] excludedTables;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaDescriptor" /> class.
        /// </summary>
        /// <param name="databaseProviderName">Name of the database provider.</param>
        /// <param name="excludedTables">The execluded tables.</param>
        protected SchemaDescriptor(string databaseProviderName, params string[] excludedTables)
        {
            DatabaseProviderName = databaseProviderName;
            this.excludedTables = excludedTables ?? new string[] { };
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
        public abstract IDictionary<string, Mapping.StatementMap> GetStoredProcs();

        /// <summary>
        /// Gets or sets the project settings.
        /// </summary>
        /// <value>The project settings.</value>
        public virtual Mapping.ProjectSettings ProjectSettings { get; set; }

        #endregion

        /// <summary>
        /// Determines whether the specified table name is excluded.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        protected bool IsExcluded(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return false;

            foreach (var xtable in excludedTables)
            {
                var tb = tableName.ToUpper();
                if (tb.Equals(xtable.ToUpper())) return true;

                if (xtable.EndsWith("*"))
                {
                    var startWith = xtable.Substring(0, xtable.Length - 2);
                    if (tb.StartsWith(startWith.ToUpper())) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is excluded entity] [the specified excluded tables].
        /// </summary>
        /// <param name="excludedTables">The excluded tables.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static bool IsExcludedEntity(string[] excludedTables, string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName) || excludedTables == null || excludedTables.Length == 0) return false;

            foreach (var xtable in excludedTables)
            {
                var tb = tableName.ToUpper();
                if (tb.Equals(xtable.ToUpper())) return true;

                if (xtable.EndsWith("*"))
                {
                    var startWith = xtable.Substring(0, xtable.Length - 2);
                    if (tb.StartsWith(startWith.ToUpper())) return true;
                }
            }

            return false;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        #endregion
    }
}

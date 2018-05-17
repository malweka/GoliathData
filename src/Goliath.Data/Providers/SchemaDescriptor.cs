using System.Collections.Generic;
using System.Linq;

namespace Goliath.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SchemaDescriptor : ISchemaDescriptor
    {
        protected string[] TableBlackList { get; }
        public IList<string> TableWhiteList { get; } = new List<string>();

        public abstract string DefaultSchemaName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaDescriptor" /> class.
        /// </summary>
        /// <param name="databaseProviderName">Name of the database provider.</param>
        /// <param name="tableBlackList">The execluded tables.</param>
        protected SchemaDescriptor(string databaseProviderName, string[] tableBlackList)
        {
            DatabaseProviderName = databaseProviderName;
            TableBlackList = tableBlackList ?? new string[] { };
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
        /// <param name="tableSchema">The table schema.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>
        ///   <c>true</c> if the specified table schema is excluded; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsExcluded(string tableSchema, string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return false;
            string tb = tableName.ToUpper();

            if (!"DBO".Equals(tableSchema.ToUpper()))
                tb = $"{tableSchema}.{tableName}".ToUpper();

            if (TableWhiteList.Count > 0)
            {
                Dictionary<string,string> whiteList = new Dictionary<string, string>();
                foreach (var table in TableWhiteList)
                {
                    if(!whiteList.ContainsKey(table.ToUpper()))
                        whiteList.Add(table.ToUpper(), table);
                }

                return !whiteList.ContainsKey(tb);
            }

            foreach (var xtable in TableBlackList)
            {
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

using System.Collections.Generic;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class DeleteSqlExecutionList
    {
        readonly Dictionary<string, DeleteSqlBodyInfo> statements = new Dictionary<string, DeleteSqlBodyInfo>();

        /// <summary>
        /// Gets the statements.
        /// </summary>
        /// <value>
        /// The statements.
        /// </value>
        public Dictionary<string, DeleteSqlBodyInfo> Statements
        {
            get { return statements; }
        }
    }
}
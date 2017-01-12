using System.Collections.Generic;
using System.Text;
using Goliath.Data.Providers;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class InsertSqlInfo
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public Dictionary<string, QueryParam> Parameters { get; private set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public Dictionary<string, string> Columns { get; private set; }

        /// <summary>
        /// Gets or sets the database key generate SQL.
        /// </summary>
        /// <value>
        /// The database key generate SQL.
        /// </value>
        public List<string> DbKeyGenerateSql { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="InsertSqlInfo"/> is processed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processed; otherwise, <c>false</c>.
        /// </value>
        public bool Processed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [delay execute].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [delay execute]; otherwise, <c>false</c>.
        /// </value>
        public bool DelayExecute { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertSqlInfo" /> class.
        /// </summary>
        public InsertSqlInfo()
        {
            Parameters = new Dictionary<string, QueryParam>();
            Columns = new Dictionary<string, string>();
            DbKeyGenerateSql = new List<string>();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(SqlDialect dialect)
        {
            return dialect.BuildInsertStatement(this);
        }
    }
}
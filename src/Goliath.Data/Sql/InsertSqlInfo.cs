using System.Collections.Generic;
using System.Text;
using Goliath.Data.Providers;

namespace Goliath.Data.Sql
{
    public class InsertSqlInfo
    {
        public Dictionary<string, QueryParam> Parameters { get; private set; }
        public Dictionary<string, string> Columns { get; private set; }
        public List<string> DbKeyGenerateSql { get; set; }
        public bool Processed { get; set; }
        public bool DelayExecute { get; set; }
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
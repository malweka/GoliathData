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
            var sb = new StringBuilder("INSERT INTO ");
            sb.AppendFormat("{0} (", dialect.Escape(TableName, EscapeValueType.TableName));
            int countChecker = Columns.Count - 1;
            int counter = 0;

            foreach(var column in Columns.Values)
            {
                sb.Append(dialect.Escape(column, EscapeValueType.Column));

                if (counter < countChecker)
                {
                    sb.Append(", ");
                }
                counter++;
            }
            sb.Append(") VALUES(");

            counter = 0;
            foreach (var param in Parameters.Values)
            {
                sb.Append(dialect.CreateParameterName(param.Name));
                if (counter < countChecker)
                    sb.Append(", ");
                counter++;
            }
            sb.Append(");\n");
            foreach (var keygen in DbKeyGenerateSql)
            {
                sb.AppendFormat("{0};\n", keygen);
            }

            return sb.ToString();
        }
    }
}
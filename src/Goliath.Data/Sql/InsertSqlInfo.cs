using System.Collections.Generic;
using System.Text;
using Goliath.Data.Providers;

namespace Goliath.Data.Sql
{
    public class InsertSqlInfo
    {
        public Dictionary<string, QueryParam> Parameters { get; private set; }
        public List<string> Columns { get; private set; }
        public List<string> DbKeyGenerateSql { get; set; }
        public string TableName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertSqlInfo" /> class.
        /// </summary>
        public InsertSqlInfo()
        {
            Parameters = new Dictionary<string, QueryParam>();
            Columns = new List<string>();
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
            sb.AppendFormat("{0} (", TableName);
            int countChecker = Columns.Count - 1;
            for (int i = 0; i < Columns.Count; i++)
            {
                sb.Append(dialect.Escape(Columns[i]));
                if (i < countChecker)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(") VALUES(");

            int counter = 0;
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
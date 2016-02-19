using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    public class SqlSelectColumnFormatter
    {
        public virtual string Format(List<string> columnNames, string tableAlias)
        {
            if (columnNames == null)
                throw new ArgumentNullException("columnNames");

            StringBuilder sql = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(tableAlias))
            {
                for (int i = 0; i < columnNames.Count; i++)
                {
                    sql.Append(PrintColumnName(columnNames[i], tableAlias));
                    if (i < (columnNames.Count - 1))
                        sql.Append(", ");
                }
            }
            else
            {
                sql.Append(string.Join(", ", columnNames));
            }

            return sql.ToString();
        }

        protected virtual string PrintColumnName(string columnName, string tableAlias)
        {
            return string.Format("{1} AS \"{1}\"", tableAlias, columnName);
        }

    }

}

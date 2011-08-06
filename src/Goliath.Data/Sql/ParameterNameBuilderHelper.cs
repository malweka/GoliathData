using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data 
{
    static class ParameterNameBuilderHelper
    {
        public static string CreateColumnNameForSelectQuery(string tableAlias, string columnName)
        {
            return string.Format("{1}.{0} AS {1}_{0}", columnName, tableAlias);
        }

        public static string CreateTableNameWithAlias(string tableAlias, string tableName)
        {
            return string.Format("{0} {1}", tableName, tableAlias);
        }

        public static string ColumnWithTableAlias(string tableAlias, string columnName)
        {
            return string.Format("{0}.{1}", tableAlias, columnName);
        }

        public static string ColumnQueryName(string propertyName, string tableAlias)
        {
            return string.Format("{0}_{1}", tableAlias, propertyName);
        }

        public  static string GetPropNameFromQueryName(string queryName, string tableAlias)
        {
            if (string.IsNullOrWhiteSpace(tableAlias))
                throw new ArgumentNullException("tableAbbreviation");

            if (string.IsNullOrWhiteSpace(queryName))
                throw new ArgumentNullException("queryName");

            var pname = queryName.Substring(tableAlias.Length + 1);
            return pname;
        }
    }
}

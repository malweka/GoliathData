using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data 
{
    static class ParameterNameBuilderHelper
    {
        /// <summary>
        /// Creates the column name for select query.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static string CreateColumnNameForSelectQuery(string tableAlias, string columnName)
        {
            return string.Format("{1}.{0} AS {1}_{0}", columnName, tableAlias);
        }

        /// <summary>
        /// Creates the table name with alias.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static string CreateTableNameWithAlias(string tableAlias, string tableName)
        {
            return string.Format("{0} {1}", tableName, tableAlias);
        }

        /// <summary>
        /// Columns the with table alias.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static string ColumnWithTableAlias(string tableAlias, string columnName)
        {
            return string.Format("{0}.{1}", tableAlias, columnName);
        }

        /// <summary>
        /// Columns the name of the query.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="tableAlias">The table alias.</param>
        /// <returns></returns>
        public static string ColumnQueryName(string propertyName, string tableAlias)
        {
            return string.Format("{0}_{1}", tableAlias, propertyName);
        }

        /// <summary>
        /// Gets the name of the prop name from query.
        /// </summary>
        /// <param name="queryName">Name of the query.</param>
        /// <param name="tableAlias">The table alias.</param>
        /// <returns></returns>
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

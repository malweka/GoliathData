using System;
using Goliath.Data.Mapping;

namespace Goliath.Data 
{
    static class ParameterNameBuilderHelper
    {
        /// <summary>
        /// Gets the name of the prop name from query.
        /// </summary>
        /// <param name="queryName">Name of the query.</param>
        /// <param name="tableAlias">The table alias.</param>
        /// <returns></returns>
        public  static string GetPropNameFromQueryName(string queryName, string tableAlias)
        {
            if (string.IsNullOrWhiteSpace(tableAlias))
                throw new ArgumentNullException("tableAlias");

            if (string.IsNullOrWhiteSpace(queryName))
                throw new ArgumentNullException("queryName");

            var pname = queryName.Substring(tableAlias.Length + 1);
            return pname;
        }

        public static string QueryParamName(EntityMap entityMap, string columnName)
        {
            return QueryParamName(entityMap.TableAlias ?? entityMap.FullName.Replace(".", "_"), columnName);
        }

        public static string QueryParamName(string prefix, string columnName)
        {
            return prefix + "_" + columnName;
        }
    }
}

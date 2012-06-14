using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Providers;
using Goliath.Data.Mapping;

namespace Goliath.Data.Sql
{
    static class SqlBuilderExtensions
    {

        public static string SelectCount(this SelectSqlBuilder selectBuilder)
        {
            var queryBody = selectBuilder.Build();
            var countFunction = selectBuilder.SqlDialect.GetFunction(FunctionNames.Count);
            if(countFunction == null)
                throw new GoliathDataException(string.Format("Count function is either not registerd for provider {0} or is not supported",
                    selectBuilder.SqlDialect.DatabaseProviderName));

            StringBuilder sb = new StringBuilder(string.Format("SELECT {0} AS Total", countFunction.ToSqlStatement()));
            sb.AppendFormat("\nFROM {0}", queryBody.From);
            if (!string.IsNullOrWhiteSpace(queryBody.JoinEnumeration))
                sb.Append(queryBody.JoinEnumeration);
            if (!string.IsNullOrWhiteSpace(queryBody.WhereExpression))
                sb.AppendFormat("\nWHERE {0}\n", queryBody.WhereExpression);

            return sb.ToString();
            
        }
    }
}

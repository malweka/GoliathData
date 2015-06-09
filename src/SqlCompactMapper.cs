using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Providers.SqlCompact
{
    using Mapping;
    using Providers;
    using Sql;

    public class SqlCompactMapper : SqlMapper
    {
        public SqlCompactMapper():base("SqlCompact"){}
        public override string IdentitySql(int increment, int seed)
        {
            if (increment < 1)
                increment = 1;
            if (seed < 1)
                seed = 1;

            return string.Format("IDENTITY({0},{1})", increment, seed);
        }

        protected override void OnRegisterFunctions()
        {
            base.OnRegisterFunctions();

            RegisterFunctions(new NewGuid());
            RegisterFunctions(new GetDate());
            RegisterFunctions(new GetCurrentUser());
            RegisterFunctions(new GetUtcDate());
            RegisterFunctions(new GetHostName());
            RegisterFunctions(new GetAppName());
            RegisterFunctions(new GetDatabaseName());
        }

        public override string QueryWithPaging(Sql.SqlQueryBody queryBody, Sql.PagingInfo pagingInfo)
        {
            StringBuilder sb = new StringBuilder("SELECT * \nFROM SELECT ROW_NUMBER() OVER");
            sb.AppendFormat(" (ORDER BY {0}) AS __RowNum, {1}", queryBody.SortExpression, queryBody.ColumnEnumeration);
            sb.AppendFormat("\nFROM {0}", queryBody.From);

            if (!string.IsNullOrWhiteSpace(queryBody.JoinEnumeration))
                sb.Append(queryBody.JoinEnumeration);
            if (!string.IsNullOrWhiteSpace(queryBody.WhereExpression))
                sb.AppendFormat("\nWHERE {0}\n", queryBody.WhereExpression);
            sb.Append(") AS RowConstrainedResult");
            sb.AppendFormat("\nWHERE __RowNum >= {0}", pagingInfo.Offset);
            sb.AppendFormat("\nAND __RowNum < {0}", pagingInfo.Offset + pagingInfo.Limit);
            sb.Append("ORDER BY __RowNum)");

            return sb.ToString();
        }

        protected override void OnRegisterTranslationTypes()
        {
            RegisterTranslateType("int", "integer");
            RegisterTranslateType("char", "ntext");
            RegisterTranslateType("bit", "Bit");
            RegisterTranslateType("bigint", "Bigint");
            RegisterTranslateType("binary", "binary");
            RegisterTranslateType("date", "datetime");
            RegisterTranslateType("datetime", "datetime");
            RegisterTranslateType("date", "datetime");
            RegisterTranslateType("decimal", "numeric");
            RegisterTranslateType("double precision", "double precision");
            RegisterTranslateType("float", "float");
            RegisterTranslateType("geometry", "image");
            RegisterTranslateType("real", "Real");
            RegisterTranslateType("smalldatetime", "datetime");
            RegisterTranslateType("smallint", "smallint");
            RegisterTranslateType("smallmoney", "money");
            RegisterTranslateType("timestamp", "bigint");
            RegisterTranslateType("tinyint", "tinyint");
            RegisterTranslateType("uniqueidentifier", "uniqueidentifier");
            RegisterTranslateType("varbinary", "varbinary");

            RegisterTranslateType("nvarchar", "nvarchar");
            RegisterTranslateType("nchar", "ntext");
            RegisterTranslateType("varchar", "nvarchar");
            RegisterTranslateType("text", "ntext");
            RegisterTranslateType("numeric", "numeric");
            RegisterTranslateType("xml", "ntext");
            
        }

        protected override string OnTranslateToSqlTypeString(Property fromType)
        {

            StringBuilder sqlSb = new StringBuilder();
            sqlSb.AppendFormat("[{0}]", fromType.ColumnName);
            string to = null;
            string fType = fromType.SqlType.ToLower();
            if (!string.IsNullOrWhiteSpace(fType))
            {
                translationTypeMap.TryGetValue(fType, out to);
                if ((fromType.Length > 0) && !fType.Equals("text") && !fType.Equals("ntext") && !fType.Equals("image"))
                {
                    if (!string.IsNullOrWhiteSpace(to) && !to.ToUpper().Equals("NTEXT"))
                    {
                        to = string.Format("{0}({1})", to, fromType.Length);
                    }                    
                }
            }

            var sType = to ?? fromType.SqlType;
            sqlSb.AppendFormat(" {0}", sType);

            if (fromType.IsPrimaryKey)
            {
                sqlSb.AppendFormat(" {0}",  PrimarykeySql().ToUpper());
            }
            if (!string.IsNullOrWhiteSpace(fromType.DefaultValue))
            {
                string dVal = fromType.DefaultValue;
                var sfunc = GetFunction(fromType.DefaultValue);
                if (sfunc != null)
                    dVal = sfunc.ToString();
                sqlSb.AppendFormat(" DEFAULT({0})", dVal);
            }
            if (!fromType.IsNullable)
            {
                sqlSb.Append(" NOT NULL");
            }

            return sqlSb.ToString();

            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Goliath.Data.Providers.SqlServer
{
    [Serializable]
    public class Mssq2008SqlMapper : SqlMapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mssq2008SqlMapper"/> class.
        /// </summary>
        public Mssq2008SqlMapper() : base(Constants.ProviderName)
        {            
           
        }

        protected override void OnRegisterTypes()
        {
           base.OnRegisterTypes();

           RegisterType(DbType.Byte, "tinyint");
           RegisterType(DbType.DateTimeOffset, "datetimeoffset");
           RegisterType(DbType.String, "ntext");
           RegisterType(DbType.Int16, "smallint");
           RegisterType(DbType.Binary, 8000, "image");
           RegisterType(DbType.Xml, 8000, "xml");
           RegisterType(DbType.DateTime, "smalldatetime");
           RegisterType(DbType.Int64, "bigint");
           RegisterType(DbType.Double, "double precision");
           RegisterType(DbType.Single, "float"); 
           RegisterType(DbType.DateTime2, "datetime2");
           RegisterType(DbType.Currency, "currency");
           RegisterType(DbType.Xml, "xml");
            
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

        /// <summary>
        /// Identities the SQL.
        /// </summary>
        /// <param name="increment">The increment.</param>
        /// <param name="seed">The seed.</param>
        /// <returns></returns>
        public override string IdentitySql(int increment, int seed)
        {
            if (increment < 1)
                increment = 1;
            if (seed < 1)
                seed = 1;

            return string.Format("identity({0},{1})", increment, seed);
        }
    }
}

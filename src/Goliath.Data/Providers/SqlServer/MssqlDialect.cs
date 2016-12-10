using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Goliath.Data.Providers.SqlServer
{

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Mssq2008Dialect : SqlDialect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mssq2008Dialect"/> class.
        /// </summary>
        public Mssq2008Dialect()
            : base(RdbmsBackend.SupportedSystemNames.Mssql2008R2)
        {
            
        }

        static Mssq2008Dialect()
        {
            var rwords = SqlServerReservedWords.Split(new string[] { " ", "\n", "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rword in rwords)
            {
                reservedWords.Add(rword);
            }
        }

        /// <summary>
        /// The SQL server reserved words
        /// </summary>
        public const string SqlServerReservedWords = "ADD EXTERNAL PROCEDURE ALL FETCH PUBLIC ALTER FILE RAISERROR AND FILLFACTOR READ ANY FOR READTEXT AS FOREIGN RECONFIGURE ASC FREETEXT REFERENCES AUTHORIZATION FREETEXTTABLE REPLICATION BACKUP FROM RESTORE BEGIN FULL RESTRICT BETWEEN FUNCTION RETURN BREAK GOTO REVERT BROWSE GRANT REVOKE BULK GROUP RIGHT BY HAVING ROLLBACK CASCADE HOLDLOCK ROWCOUNT CASE IDENTITY ROWGUIDCOL CHECK IDENTITY_INSERT RULE CHECKPOINT IDENTITYCOL SAVE CLOSE IF SCHEMA CLUSTERED IN SECURITYAUDIT COALESCE INDEX SELECT COLLATE INNER SEMANTICKEYPHRASETABLE COLUMN INSERT SEMANTICSIMILARITYDETAILSTABLE COMMIT INTERSECT SEMANTICSIMILARITYTABLE COMPUTE INTO SESSION_USER CONSTRAINT IS SET CONTAINS JOIN SETUSER CONTAINSTABLE KEY SHUTDOWN CONTINUE KILL SOME CONVERT LEFT STATISTICS CREATE LIKE SYSTEM_USER CROSS LINENO TABLE CURRENT LOAD TABLESAMPLE CURRENT_DATE MERGE TEXTSIZE CURRENT_TIME NATIONAL THEN CURRENT_TIMESTAMP NOCHECK TO CURRENT_USER NONCLUSTERED TOP CURSOR NOT TRAN DATABASE NULL TRANSACTION DBCC NULLIF TRIGGER DEALLOCATE OF TRUNCATE DECLARE OFF TRY_CONVERT DEFAULT OFFSETS TSEQUAL DELETE ON UNION DENY OPEN UNIQUE DESC OPENDATASOURCE UNPIVOT DISK OPENQUERY UPDATE DISTINCT OPENROWSET UPDATETEXT DISTRIBUTED OPENXML USE DOUBLE OPTION USER DROP OR VALUES DUMP ORDER VARYING ELSE OUTER VIEW END OVER WAITFOR ERRLVL PERCENT WHEN ESCAPE PIVOT WHERE EXCEPT PLAN WHILE EXEC PRECISION WITH EXECUTE PRIMARY WITHIN GROUP EXISTS PRINT WRITETEXT EXIT PROC";

        /// <summary>
        /// Called when [register types].
        /// </summary>
        protected override void OnRegisterTypes()
        {
            base.OnRegisterTypes();

            RegisterType(DbType.Int32, "tinyint");
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
            RegisterType(DbType.Currency, "money");
            RegisterType(DbType.Binary, 8000, "binary");


        }

        /// <summary>
        /// Called when [register functions].
        /// </summary>
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

        /// <summary>
        /// Queries the with paging.
        /// </summary>
        /// <param name="queryBody">The query body.</param>
        /// <param name="pagingInfo">The paging info.</param>
        /// <returns></returns>
        public override string QueryWithPaging(SqlQueryBody queryBody, PagingInfo pagingInfo)
        {
            StringBuilder sb = new StringBuilder("SELECT * \nFROM \n\t\t( SELECT ROW_NUMBER() OVER");
            sb.AppendFormat(" (ORDER BY {0}) AS __RowNum, {1}", queryBody.SortExpression, queryBody.ColumnEnumeration);
            sb.AppendFormat("\n\t\tFROM {0} ", queryBody.From);

            if (!string.IsNullOrWhiteSpace(queryBody.JoinEnumeration))
                sb.Append(queryBody.JoinEnumeration);

            if (!string.IsNullOrWhiteSpace(queryBody.WhereExpression))
                sb.AppendFormat("\n\t\tWHERE {0}\n", queryBody.WhereExpression);
            sb.Append(") AS RowConstrainedResult");
            sb.AppendFormat("\nWHERE __RowNum > {0}", pagingInfo.Offset);
            sb.AppendFormat("\nAND __RowNum <= {0}", pagingInfo.Offset +  pagingInfo.Limit);
            sb.Append("\nORDER BY __RowNum");

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

        /// <summary>
        /// Selects the last insert row id SQL.
        /// </summary>
        /// <returns></returns>
        public override string SelectLastInsertRowIdSql()
        {
            //oracle SELECT SEQNAME.CURRVAL FROM DUAL;
            //mysql SELECT LAST_INSERT_ID();
            //postgresql SELECT lastval();
            //sqlite SELECT last_insert_rowid()
            return ";\nSELECT SCOPE_IDENTITY()";
        }

        /// <summary>
        /// Called when [translate to SQL type string].
        /// </summary>
        /// <param name="fromType">From type.</param>
        /// <returns></returns>
        protected override string OnTranslateToSqlTypeString(Mapping.Property fromType)
        {

            StringBuilder sqlSb = new StringBuilder();
            sqlSb.Append(Escape(fromType.ColumnName));
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

            if (fromType.IsIdentity)
            {
                sqlSb.AppendFormat(" IDENTITY(1,1)");
            }

            //if (fromType.IsPrimaryKey)
            //{
            //    sqlSb.AppendFormat(" {0}", PrimarykeySql().ToUpper());
            //}
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

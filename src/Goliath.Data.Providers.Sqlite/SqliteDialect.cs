using System;
using System.Data;
using System.Text;
using Goliath.Data.Sql;

namespace Goliath.Data.Providers.Sqlite
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SqliteDialect : SqlDialect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDialect"/> class.
        /// </summary>
        public SqliteDialect()
            : base(RdbmsBackend.SupportedSystemNames.Sqlite3)
        {

        }

        /// <summary>
        /// Inits the register.
        /// </summary>
        protected override void OnRegisterTypes()
        {
            RegisterType(DbType.Byte, "tinyint");
            RegisterType(DbType.Int16, "smallint");
            RegisterType(DbType.Int64, "bigint");
            RegisterType(DbType.Double, "double");
            RegisterType(DbType.Single, "float");
            RegisterType(DbType.Binary, 8000, "blob");
            base.OnRegisterTypes();
        }

        /// <summary>
        /// Identities the SQL.
        /// </summary>
        /// <param name="increment">The increment.</param>
        /// <param name="seed">The seed.</param>
        /// <returns></returns>
        public override string IdentitySql(int increment, int seed)
        {
            return "autoincrement";
        }

        /// <summary>
        /// Selects the last insert row id SQL.
        /// </summary>
        /// <returns></returns>
        public override string SelectLastInsertRowIdSql()
        {
            return ";\nSELECT last_insert_rowid()";
        }

        /// <summary>
        /// Creates the name of the parameter.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns></returns>
        public override string CreateParameterName(string variableName)
        {
            return string.Format("${0}", variableName);
        }

        public override string Escape(string value, EscapeValueType escapeValueType)
        {
            return $"\"{value}\"";
        }

        protected override void OnRegisterFunctions()
        {
            base.OnRegisterFunctions();


            RegisterFunctions(new GetDate());
            RegisterFunctions(new GetUtcDate());
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
                //sqlSb.AppendFormat(" autoincrement");
            }

            //if (fromType.IsPrimaryKey)
            //{
            //    sqlSb.AppendFormat(" {0}", PrimarykeySql().ToUpper());
            //}
            if (!string.IsNullOrWhiteSpace(fromType.DefaultValue))
            {
                string dVal = fromType.DefaultValue.Replace("N'", "'");
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

    [Serializable]
    class GetDate : SqlFunction
    {
        public GetDate()
            : base(FunctionNames.GetDate, "date")
        {
        }

        public override string ToSqlStatement(params QueryParam[] args)
        {
            return $"{Declaration}('now')";
        }
    }

    [Serializable]
    class GetUtcDate : SqlFunction
    {
        public GetUtcDate()
            : base(FunctionNames.GetUtcDate, "date")
        {

        }

        public override string ToSqlStatement(params QueryParam[] args)
        {
            return $"{Declaration}('now')";
        }
    }

}

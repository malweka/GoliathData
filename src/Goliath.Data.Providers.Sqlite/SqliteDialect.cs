using System;
using System.Data;
using System.Text;
using Goliath.Data.Mapping;
using Goliath.Data.Providers.Sqlite.Functions;

namespace Goliath.Data.Providers.Sqlite
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SqliteDialect : SqlDialect
    {
        public override string DefaultSchemaName => "main";

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
            return $"${variableName}";
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

            var sqlSb = new StringBuilder();
            sqlSb.Append(Escape(fromType.ColumnName));
            string to = null;
            var fType = fromType.SqlType.ToLower();
            if (!string.IsNullOrWhiteSpace(fType))
            {
                translationTypeMap.TryGetValue(fType, out to);
                if ((fromType.Length > 0) && !fType.Equals("text") && !fType.Equals("ntext") && !fType.Equals("image"))
                {
                    if (!string.IsNullOrWhiteSpace(to) && !to.ToUpper().Equals("NTEXT"))
                    {
                        to = $"{to}({fromType.Length})";
                    }
                }
            }

            var sType = to ?? fromType.SqlType;
            sqlSb.AppendFormat(" {0}", sType);

            //if (fromType.IsIdentity)
            //{
            //    //sqlSb.AppendFormat(" autoincrement");
            //}

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

        public override string GetValueAsSqlString(object value, Property prop)
        {
            if (value == null)
                return "NULL";

            switch (prop.DbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Xml:
                    return $"'{value.ToString().Replace("'", "''")}'";
                case DbType.Int32:
                case DbType.Int16:
                case DbType.Int64:
                case DbType.Decimal:
                case DbType.Single:
                case DbType.Currency:
                case DbType.Binary:
                case DbType.Byte:
                case DbType.Double:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.SByte:
                case DbType.VarNumeric:
                    return value.ToString();
                case DbType.Boolean:
                    bool boolVal;
                    bool.TryParse(value.ToString(), out boolVal);
                    return boolVal ? "1" : "0";
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    var datetime = (DateTime)value;
                    if (DateTime.MinValue.Equals(datetime))
                        return "NULL";
                    var dateString = datetime.ToString("yyyy-MM-dd HH:mm:ss");
                    return $"'{dateString}'";
                case DbType.Guid:
                case DbType.Object:
                case DbType.Time:
                    return $"'{value}'";
                default:
                    return $"'{value}'";
            }

        }
    }
}

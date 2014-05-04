using System;
using System.Data;

namespace Goliath.Data.Providers.Postgres
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PostgresDialect : SqlDialect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDialect"/> class.
        /// </summary>
        public PostgresDialect()
            : base(RdbmsBackend.SupportedSystemNames.Postgresql9)
        {

        }

        /// <summary>
        /// Called when [register types].
        /// </summary>
        protected override void OnRegisterTypes()
        {
            base.OnRegisterTypes();
            RegisterType(DbType.Int32, "serial");
            RegisterType(DbType.Int32, "int4");
            RegisterType(DbType.Int64, "bigserial");
            RegisterType(DbType.Int64, "serial8");
            RegisterType(DbType.Int64, "int8");
            RegisterType(DbType.Double, "double precision");
            RegisterType(DbType.Double, "float8");
            RegisterType(DbType.Single, "float");
            RegisterType(DbType.Int16, "smallint");
            RegisterType(DbType.Int16, "int2");
            RegisterType(DbType.Int64, "bigint");
            RegisterType(DbType.Currency, "money");

            RegisterType(DbType.AnsiStringFixedLength, 8000, "character");
            RegisterType(DbType.AnsiString, 8000, "character varying");
            RegisterType(DbType.Binary, "bytea");

            RegisterType(DbType.DateTime, "timestamp");
            RegisterType(DbType.Guid, "uuid");
        }

        /// <summary>
        /// Identities the SQL.
        /// </summary>
        /// <param name="increment">The increment.</param>
        /// <param name="seed">The seed.</param>
        /// <returns></returns>
        public override string IdentitySql(int increment, int seed)
        {
            return "SERIAL";
        }

        /// <summary>
        /// Selects the last insert row id SQL.
        /// </summary>
        /// <returns></returns>
        public override string SelectLastInsertRowIdSql()
        {
            return ";\nSELECT lastval()";
        }

        /// <summary>
        /// Creates the name of the parameter.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns></returns>
        public override string CreateParameterName(string variableName)
        {
            return string.Format("@{0}", variableName);
        }


        /// <summary>
        /// Escapes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="escapeValueType">Type of the escape value.</param>
        /// <returns></returns>
        public override string Escape(string value, EscapeValueType escapeValueType)
        {
            return "\"" + value + "\"";
        }
    }
}

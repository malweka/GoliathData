using System;
using System.Data;

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
            return "SELECT last_insert_rowid()";
        }

        public override string CreateParameterName(string variableName)
        {
            return string.Format("${0}", variableName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Goliath.Data.Providers.Sqlite
{
    /// <summary>
    /// 
    /// </summary>
    public class SqliteSqlMapper : SqlMapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteSqlMapper"/> class.
        /// </summary>
        public SqliteSqlMapper()
            : base(Constants.ProviderName)
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

        public override string CreateParameterName(string variableName)
        {
            return string.Format("${0}", variableName);
        }
    }
}

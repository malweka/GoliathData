using System;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.Generators
{
    class AutoIncrementGenerator : IKeyGenerator
    {
        public const string GeneratorName = "Auto_Increment";
        #region IKeyGenerator Members

        public Type KeyType
        {
            get { return typeof(int); }
        }

        public string Name
        {
            get { return GeneratorName; }
        }

        public object GenerateKey(SqlDialect sqlDialect, EntityMap entityMap, string propertyName, out Sql.SqlOperationPriority priority)
        {
            priority = Sql.SqlOperationPriority.High;
            return sqlDialect.SelectLastInsertRowIdSql();
        }

        public bool IsDatabaseGenerated
        {
            get { return true; }
        }

        #endregion
    }
}

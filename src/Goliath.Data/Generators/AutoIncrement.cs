using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Generators
{
    using Providers;
    using Mapping;

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

        public object GenerateKey(SqlMapper sqlMapper, EntityMap entityMap, string propertyName, out Sql.SqlOperationPriority priority)
        {
            priority = Sql.SqlOperationPriority.High;
            return sqlMapper.SelectLastInsertRowIdSql();
        }

        public bool IsDatabaseGenerated
        {
            get { return true; }
        }

        #endregion
    }
}

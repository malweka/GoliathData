using System.Collections.Generic;
using System.Data.Common;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Utils;

namespace Goliath.Data.DataAccess
{
    abstract class RelationSerializer
    {
        protected SqlDialect sqlDialect;
        protected EntityAccessorStore store;

        protected RelationSerializer(SqlDialect sqlDialect, EntityAccessorStore store)
        {
            this.sqlDialect = sqlDialect;
            this.store = store;
        }

        public abstract void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropertyAccessor pInfo, EntityMap entityMap, EntityAccessor entityAccessor, DbDataReader dbReader);
        
    }
}

using System.Collections.Generic;
using System.Data.Common;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.DataAccess
{
    abstract class RelationSerializer
    {
        protected SqlDialect sqlDialect;
        protected GetSetStore getSetStore;

        protected RelationSerializer(SqlDialect sqlDialect, GetSetStore getSetStore)
        {
            this.sqlDialect = sqlDialect;
            this.getSetStore = getSetStore;
        }

        public abstract void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropInfo pInfo, EntityMap entityMap, EntityGetSetInfo getSetInfo, Dictionary<string, int> columns, DbDataReader dbReader);
        
    }
}

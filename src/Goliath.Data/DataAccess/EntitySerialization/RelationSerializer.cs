using System.Collections.Generic;
using System.Data.Common;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.DataAccess
{
    abstract class RelationSerializer
    {
        protected SqlMapper sqlMapper;
        protected GetSetStore getSetStore;

        protected RelationSerializer(SqlMapper sqlMapper, GetSetStore getSetStore)
        {
            this.sqlMapper = sqlMapper;
            this.getSetStore = getSetStore;
        }

        public abstract void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropInfo pInfo, EntityMap entityMap, EntityGetSetInfo getSetInfo, Dictionary<string, int> columns, DbDataReader dbReader);
        
    }
}

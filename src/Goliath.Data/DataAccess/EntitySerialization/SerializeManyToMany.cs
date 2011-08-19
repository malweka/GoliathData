using System;
using System.Collections.Generic;
using System.Data.Common;
using Goliath.Data.DynamicProxy;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Sql;

namespace Goliath.Data.DataAccess
{
    class SerializeManyToMany : RelationSerializer
    {
        public SerializeManyToMany(SqlMapper sqlMapper, GetSetStore getSetStore)
            : base(sqlMapper, getSetStore)
        {
        }

        public override void Serialize(EntitySerializer serializer, Relation rel, object instanceEntity, PropInfo pInfo, EntityMap entityMap, EntityGetSetInfo getSetInfo, Dictionary<string, int> columns, DbDataReader dbReader)
        {
            
        }
    }
}

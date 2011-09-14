using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    using DynamicProxy;
    using Mapping;
    using Providers;
    using Sql;

    class SerializeManyToMany : RelationSerializer
    {
        public SerializeManyToMany(SqlMapper sqlMapper, GetSetStore getSetStore)
            : base(sqlMapper, getSetStore)
        {
        }

        public override void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropInfo pInfo, EntityMap entityMap, EntityGetSetInfo getSetInfo, Dictionary<string, int> columns, DbDataReader dbReader)
        {
            
        }
    }
}

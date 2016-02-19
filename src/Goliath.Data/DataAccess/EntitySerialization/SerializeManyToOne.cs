using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Goliath.Data.DynamicProxy;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Sql;
using Goliath.Data.Utils;

namespace Goliath.Data.DataAccess
{
    class SerializeManyToOne : RelationSerializer
    {

        public SerializeManyToOne(SqlDialect sqlDialect, EntityAccessorStore store)
            : base(sqlDialect, store)
        {
        }

        public override void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropertyAccessor pInfo, EntityMap entityMap, EntityAccessor entityAccessor, DbDataReader dbReader)
        {
            var relMap = settings.Map.GetEntityMap(rel.ReferenceEntityName);
            var relCols = new List<string>();
            var session = serializer.SessionCreator();

            int iteration = 0;
            int recursion = 0;

            var relQueryMap = new TableQueryMap(relMap.FullName, ref recursion, ref iteration);

            QueryBuilder q = new QueryBuilder(session, relCols);
            relQueryMap.LoadColumns(relMap, session, q, relCols);
            var prop = entityMap.FirstOrDefault(c => c.ColumnName == rel.ColumnName);

            if (prop == null)
                throw new GoliathDataException(string.Format("{0}: Reference {1} does not have matching property.", entityMap.FullName, rel.PropertyName));

            var valAccessor = entityAccessor.GetPropertyAccessor(prop.PropertyName);
            var val = valAccessor.GetMethod(instanceEntity);

            var queryBuilder = q.From(relMap.TableName, relQueryMap.Prefix)
                .Where(rel.ReferenceColumn).EqualToValue(val);

            IProxyHydrator hydrator = new ProxyHydrator(queryBuilder as QueryBuilder, pInfo.PropertyType, relMap, serializer, session);

            ProxyBuilder pbuilder = new ProxyBuilder();
            var proxyType = pbuilder.CreateProxyType(pInfo.PropertyType, relMap);
            object proxyobj = Activator.CreateInstance(proxyType, new object[] { pInfo.PropertyType, hydrator });
            pInfo.SetMethod(instanceEntity, proxyobj);
        }
    }
}

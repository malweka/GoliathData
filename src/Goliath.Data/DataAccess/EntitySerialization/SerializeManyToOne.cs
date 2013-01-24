using System;
using System.Collections.Generic;
using System.Data.Common;
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

        public override void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropertyAccessor pInfo, EntityMap entityMap, EntityAccessor entityAccessor, Dictionary<string, int> columns, DbDataReader dbReader)
        {
            if (!rel.LazyLoad)
            {
                var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                var relColumns = EntitySerializer.GetColumnNames(dbReader, relEntMap.TableAlias + rel.InternalIndex);
                Type relType = pInfo.PropertyType;
                var relEntAccessor = store.GetEntityAccessor(relType, relEntMap);
                object relIstance = serializer.CreateInstance(relType, relEntMap); //Activator.CreateInstance(relType);
                serializer.SerializeSingle(relIstance, relType, relEntMap, relEntAccessor, relColumns, dbReader);
                pInfo.SetMethod(instanceEntity, relIstance);
            }
            else
            {
                ProxyBuilder pbuilder = new ProxyBuilder();
                int ordinal;

                var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                if (columns.TryGetValue(rel.ColumnName, out ordinal))
                {
                    var val = dbReader[ordinal];
                    if (val != null)
                    {
                        QueryParam qp = new QueryParam(ParameterNameBuilderHelper.ColumnQueryName(relEntMap.TableAlias + rel.InternalIndex, rel.ReferenceColumn)) { Value = val };

                        SelectSqlBuilder sqlBuilder = new SelectSqlBuilder(sqlDialect, relEntMap)
                           .Where(new WhereStatement(ParameterNameBuilderHelper.ColumnWithTableAlias(relEntMap.TableAlias + rel.InternalIndex, rel.ReferenceColumn))
                                    .Equals(sqlDialect.CreateParameterName(qp.Name)));

                        SqlOperationInfo qInfo = new SqlOperationInfo() { CommandType = SqlStatementType.Select };
                        qInfo.SqlText = sqlBuilder.ToSqlString();
                        qInfo.Parameters = new QueryParam[] { qp };

                        IProxyHydrator hydrator = new ProxyHydrator(qInfo, pInfo.PropertyType, relEntMap, serializer, settings);
                        var proxyType = pbuilder.CreateProxyType(pInfo.PropertyType, relEntMap);
                        object proxyobj = Activator.CreateInstance(proxyType, new object[] { pInfo.PropertyType, hydrator });
                        pInfo.SetMethod(instanceEntity, proxyobj);

                    }
                }
            }
        }
    }
}

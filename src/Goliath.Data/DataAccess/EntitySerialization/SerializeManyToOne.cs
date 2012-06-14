using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    using DynamicProxy;
    using Mapping;
    using Providers;
    using Sql;

    class SerializeManyToOne : RelationSerializer
    {

        public SerializeManyToOne(SqlDialect sqlDialect, GetSetStore getSetStore)
            : base(sqlDialect, getSetStore)
        {
        }

        public override void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropInfo pInfo, EntityMap entityMap, EntityGetSetInfo getSetInfo, Dictionary<string, int> columns, DbDataReader dbReader)
        {
            if (!rel.LazyLoad)
            {
                var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                var relColumns = EntitySerializer.GetColumnNames(dbReader, relEntMap.TableAlias);
                Type relType = pInfo.PropertType;
                EntityGetSetInfo relGetSetInfo = getSetStore.GetReflectionInfoAddIfMissing(relType, relEntMap);
                object relIstance = Activator.CreateInstance(relType);
                serializer.SerializeSingle(relIstance, relType, relEntMap, relGetSetInfo, relColumns, dbReader);
                pInfo.Setter(instanceEntity, relIstance);
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
                        QueryParam qp = new QueryParam(ParameterNameBuilderHelper.ColumnQueryName(relEntMap.TableAlias, rel.ReferenceColumn)) { Value = val };

                        SelectSqlBuilder sqlBuilder = new SelectSqlBuilder(sqlDialect, relEntMap)
                           .Where(new WhereStatement(ParameterNameBuilderHelper.ColumnWithTableAlias(relEntMap.TableAlias, rel.ReferenceColumn))
                                    .Equals(sqlDialect.CreateParameterName(qp.Name)));

                        SqlOperationInfo qInfo = new SqlOperationInfo() { CommandType = SqlStatementType.Select };
                        qInfo.SqlText = sqlBuilder.ToSqlString();
                        qInfo.Parameters = new QueryParam[] { qp };

                        IProxyHydrator hydrator = new ProxyHydrator(qInfo, pInfo.PropertType, relEntMap, serializer, settings);
                        var proxyType = pbuilder.CreateProxy(pInfo.PropertType, relEntMap);
                        object proxyobj = Activator.CreateInstance(proxyType, new object[] { pInfo.PropertType, hydrator });
                        pInfo.Setter(instanceEntity, proxyobj);
                        
                    }
                }
            }
        }
    }
}

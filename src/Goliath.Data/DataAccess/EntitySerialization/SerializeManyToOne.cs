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

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeManyToOne"/> class.
        /// </summary>
        /// <param name="sqlMapper">The SQL mapper.</param>
        /// <param name="getSetStore">The get set store.</param>
        public SerializeManyToOne(SqlMapper sqlMapper, GetSetStore getSetStore)
            : base(sqlMapper, getSetStore)
        {
        }

        public override void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropInfo pInfo, EntityMap entityMap, EntityGetSetInfo getSetInfo, Dictionary<string, int> columns, DbDataReader dbReader)
        {
            if (!rel.LazyLoad)
            {
                var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                var relColumns = EntitySerializer.GetColumnNames(dbReader, relEntMap.TableAlias);
                EntityGetSetInfo relGetSetInfo;
                Type relType = pInfo.PropertType;
                if (!getSetStore.TryGetValue(relType, out relGetSetInfo))
                {
                    relGetSetInfo = new EntityGetSetInfo(relType);
                    relGetSetInfo.Load(relEntMap);
                    getSetStore.Add(relType, relGetSetInfo);
                }

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

                        SelectSqlBuilder sqlBuilder = new SelectSqlBuilder(sqlMapper, relEntMap)
                           .Where(new WhereStatement(ParameterNameBuilderHelper.ColumnWithTableAlias(relEntMap.TableAlias, rel.ReferenceColumn))
                                    .Equals(sqlMapper.CreateParameterName(qp.Name)));

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

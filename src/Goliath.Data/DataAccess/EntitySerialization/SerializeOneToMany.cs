using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Goliath.Data.DataAccess
{
    using Mapping;
    using Providers;
    using Sql;

    class SerializeOneToMany : RelationSerializer
    {
        public SerializeOneToMany(SqlDialect sqlDialect, GetSetStore getSetStore)
            : base(sqlDialect, getSetStore)
        {
        }

        public override void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropInfo pInfo, EntityMap entityMap, EntityGetSetInfo getSetInfo, Dictionary<string, int> columns, DbDataReader dbReader)
        {
            int ordinal;
            var propType = pInfo.PropertType;
            var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
            Type refEntityType = propType.GetGenericArguments().FirstOrDefault();

            if (refEntityType == null)
            {
                throw new MappingException(string.Format("property type mismatch: {0} should be IList<T>", rel.PropertyName));
            }

            if (propType.Equals(typeof(IList<>).MakeGenericType(new Type[] { refEntityType })))
            {
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

                        var collectionType = typeof(Collections.LazyList<>).MakeGenericType(new Type[] { refEntityType });
                        var lazyCol = Activator.CreateInstance(collectionType, qInfo, relEntMap, serializer, settings);
                        pInfo.Setter(instanceEntity, lazyCol);
                    }
                    else
                    {
                        var collectionType = typeof(List<>).MakeGenericType(new Type[] { refEntityType });
                        pInfo.Setter(instanceEntity, Activator.CreateInstance(collectionType));
                    }
                }

            }
            else
                throw new MappingException(string.Format("property type mismatch: {0} should be IList<T>", rel.PropertyName));
        }
    }
}

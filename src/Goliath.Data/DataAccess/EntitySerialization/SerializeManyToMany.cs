using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Sql;
using Goliath.Data.Utils;

namespace Goliath.Data.DataAccess
{
    class SerializeManyToMany : RelationSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeManyToMany" /> class.
        /// </summary>
        /// <param name="sqlDialect">The SQL dialect.</param>
        /// <param name="store">The store.</param>
        public SerializeManyToMany(SqlDialect sqlDialect, EntityAccessorStore store)
            : base(sqlDialect, store)
        {
        }

        /// <summary>
        /// Serializes the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="rel">The rel.</param>
        /// <param name="instanceEntity">The instance entity.</param>
        /// <param name="pInfo">The p info.</param>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="entityAccessor">The entity accessor.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="dbReader">The db reader.</param>
        /// <exception cref="MappingException"></exception>
        public override void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropertyAccessor pInfo, EntityMap entityMap, EntityAccessor entityAccessor, Dictionary<string, int> columns, DbDataReader dbReader)
        {
            var propType = pInfo.PropertyType;
            var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
            var refEntityType = propType.GetGenericArguments().FirstOrDefault();

            if (refEntityType == null)
            {
                throw new MappingException(string.Format("property type mismatch: {0} should be IList<T>", rel.PropertyName));
            }

            if (propType == typeof(IList<>).MakeGenericType(new Type[] { refEntityType }))
            {
                int ordinal;
                if (columns.TryGetValue(rel.ColumnName, out ordinal))
                {
                    var val = dbReader[ordinal];

                    if (val != null)
                    {
                        var leftColumn1 = new Relation() { ColumnName = rel.MapColumn, PropertyName = rel.MapColumn };
                        var leftcolumn2 = new Relation() { ColumnName = rel.MapReferenceColumn, PropertyName = rel.MapReferenceColumn };

                        var mapTableMap = UnMappedTableMap.Create(rel.MapTableName, leftColumn1, leftcolumn2);
                        mapTableMap.TableAlias = "mX1";

                        var currEntMap = UnMappedTableMap.Create(entityMap.TableName);

                        var qp = new QueryParam(ParameterNameBuilderHelper.ColumnQueryName(rel.MapColumn, mapTableMap.TableAlias)) { Value = val };

                        var sqlBuilder = new SelectSqlBuilder(sqlDialect, mapTableMap)
                            .AddJoin(new SqlJoin(mapTableMap, JoinType.Inner).OnTable(relEntMap).OnLeftColumn(leftcolumn2).OnRightColumn(rel.ReferenceColumn))
                            .AddJoin(new SqlJoin(mapTableMap, JoinType.Inner).OnTable(currEntMap).OnLeftColumn(leftColumn1).OnRightColumn(rel.ColumnName))
                            .Where(new WhereStatement(ParameterNameBuilderHelper.ColumnWithTableAlias(mapTableMap.TableAlias, rel.MapColumn))
                            .Equals(sqlDialect.CreateParameterName(qp.Name)));

                        var qInfo = new SqlOperationInfo
                                        {
                                            CommandType = SqlStatementType.Select,
                                            SqlText = sqlBuilder.ToSqlString(),
                                            Parameters = new QueryParam[] {qp}
                                        };

                        var collectionType = typeof(Collections.LazyList<>).MakeGenericType(new Type[] { refEntityType });
                        var lazyCol = Activator.CreateInstance(collectionType, qInfo, relEntMap, serializer, settings);
                        pInfo.SetMethod(instanceEntity, lazyCol);
                    }
                    else
                    {
                        var collectionType = typeof(List<>).MakeGenericType(new Type[] { refEntityType });
                        pInfo.SetMethod(instanceEntity, Activator.CreateInstance(collectionType));
                    }
                }
            }
            else
                throw new MappingException(string.Format("property type mismatch: {0} should be IList<T>", rel.PropertyName));

        }
    }
}

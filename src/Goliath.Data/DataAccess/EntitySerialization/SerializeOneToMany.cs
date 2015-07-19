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
    class SerializeOneToMany : RelationSerializer
    {
        public SerializeOneToMany(SqlDialect sqlDialect, EntityAccessorStore store)
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
        public override void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropertyAccessor pInfo, EntityMap entityMap, EntityAccessor entityAccessor, DbDataReader dbReader)
        {
            var propType = pInfo.PropertyType;
            var relEntMap = settings.Map.GetEntityMap(rel.ReferenceEntityName);

            Type refEntityType = propType.GetGenericArguments().FirstOrDefault();
            var prop = entityMap.FirstOrDefault(c => rel.ColumnName.Equals(c.ColumnName));

            if (prop == null)
                throw new GoliathDataException(string.Format("{0}: Reference {1} does not have matching property.", entityMap.FullName, rel.PropertyName));

            if (refEntityType == null)
            {
                throw new MappingException(string.Format("property type mismatch: {0} should be IList<T>", rel.PropertyName));
            }

            if (propType == typeof(IList<>).MakeGenericType(new Type[] { refEntityType }))
            {
                var valAccessor = entityAccessor.GetPropertyAccessor(prop.PropertyName);
                var val = valAccessor.GetMethod(instanceEntity);

                if (val != null)
                {
                    var relCols = new List<string>();
                    var session = serializer.SessionCreator();

                    int iteration = 0;
                    int recursion = 0;
                    var relQueryMap = new TableQueryMap(relEntMap.FullName, ref recursion, ref iteration);
                    QueryBuilder q = new QueryBuilder(session, relCols);
                    relQueryMap.LoadColumns(relEntMap, session, q, relCols,true);
                    var queryBuilder = q.From(relEntMap.TableName, relQueryMap.Prefix)
                        .Where(rel.ReferenceColumn).EqualToValue(val) as QueryBuilder;

                    var collectionType = typeof(Collections.LazyList<>).MakeGenericType(new Type[] { refEntityType });
                    var lazyCol = Activator.CreateInstance(collectionType, queryBuilder, relEntMap, serializer, session);
                    pInfo.SetMethod(instanceEntity, lazyCol);
                }
                else
                {
                    var collectionType = typeof(List<>).MakeGenericType(new Type[] { refEntityType });
                    pInfo.SetMethod(instanceEntity, Activator.CreateInstance(collectionType));
                }
            }
            else
                throw new MappingException(string.Format("property type mismatch: {0} should be IList<T>", rel.PropertyName));
        }
    }
}

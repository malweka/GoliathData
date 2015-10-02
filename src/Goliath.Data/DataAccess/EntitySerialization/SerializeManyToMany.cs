using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Sql;
using Goliath.Data.Utils;
using Microsoft.Win32;

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


        public override void Serialize(IDatabaseSettings settings, EntitySerializer serializer, Relation rel, object instanceEntity, PropertyAccessor pInfo, EntityMap entityMap, EntityAccessor entityAccessor, DbDataReader dbReader)
        {
            var propType = pInfo.PropertyType;
            var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
            var refEntityType = propType.GetGenericArguments().FirstOrDefault();

            if (refEntityType == null)
            {
                throw new MappingException(string.Format("property type mismatch: {0} should be IList<T>", rel.PropertyName));
            }

            var prop = entityMap.FirstOrDefault(c => c.ColumnName == rel.ColumnName);

            if (prop == null)
                throw new GoliathDataException(string.Format("{0}: Reference {1} does not have matching property.", entityMap.FullName, rel.PropertyName));


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
                    relQueryMap.LoadColumns(relEntMap, session, q, relCols);
                    q.QueryMap = relQueryMap;

                    var queryBuilder = q.From(relEntMap.TableName, relQueryMap.Prefix)
                        .InnerJoin(rel.MapTableName, "m_t1")
                            .On(relQueryMap.Prefix, rel.MapReferenceColumn)
                            .EqualTo(rel.MapPropertyName)
                        .InnerJoin(entityMap.TableName, "e_t1")
                            .On("m_t1", "m_t1." + rel.MapColumn)
                            .EqualTo("e_t1." + rel.MapPropertyName)
                        .Where("m_t1", rel.MapColumn).EqualToValue(val) as QueryBuilder;



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

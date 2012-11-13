using System.Collections.Generic;
using System.Text;
using Goliath.Data.DataAccess;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    class UpdateSqlBuilderOld : SqlBuilder
    {

        int level;
        int rootLevel;

        public UpdateSqlBuilderOld(SqlDialect dialect, EntityMap entMap)
            : this(dialect, entMap, 0, 0)
        {

        }


        public UpdateSqlBuilderOld(SqlDialect dialect, EntityMap entMap, int level, int rootLevel)
            : base(dialect, entMap)
        {
            this.level = level;
            this.rootLevel = rootLevel;

            foreach (var prop in entMap)
            {
                if (prop.IgnoreOnUpdate || prop.IsPrimaryKey)
                    continue;

                string paramName = null;

                if (prop is Relation)
                {
                    var rel = (Relation)prop;
                    if (rel.RelationType != RelationshipType.ManyToOne)
                        continue;
                }

                paramName = BuildParameterNameWithLevel(prop.ColumnName, entMap.TableAlias, rootLevel);

                if (!Columns.ContainsKey(prop.ColumnName))
                {
                    Columns.Add(prop.ColumnName, dialect.CreateParameterName(paramName));
                }

            }
        }

        public UpdateSqlBuilderOld Where(params WhereStatement[] whereCollection)
        {
            if (whereCollection != null)
            {
                foreach (var w in whereCollection)
                    wheres.Add(w);
            }

            return this;
        }

       
        public override string ToSqlString()
        {
            //if (entMap.PrimaryKey == null)
            //    throw new InvalidOperationException("Cannot build update for a table without primary key");

            StringBuilder sb = new StringBuilder("UPDATE ");
            sb.AppendFormat("{0} SET ", entMap.TableName);

            int count = 0;

            foreach (var col in Columns)
            {
                if (count > 0)
                {
                    sb.Append(", ");
                }

                sb.AppendFormat("{0} = {1}", col.Key, col.Value);
                count++;
            }

            sb.Append("\nWHERE ");
            foreach (var w in wheres)
            {
                sb.AppendFormat("{0} ", w.ToString());
            }

            return sb.ToString();
        }

        public static Dictionary<string, ParamHolder> BuildUpdateQueryParams(object entity, EntityAccessor entityAccessor,
             EntityMap entityMap, EntityAccessorStore EntityAccessorStore, int level, int rootLevel)
        {
            Dictionary<string, ParamHolder> parameters = new Dictionary<string, ParamHolder>();

            foreach (var prop in entityMap)
            {
                if (prop.IgnoreOnUpdate)
                    continue;

                if ((prop is Relation) && !prop.IsPrimaryKey)
                {
                    Relation rel = (Relation)prop;
                    if (rel.RelationType != RelationshipType.ManyToOne)
                        continue;

                    PropertyAccessor pInfo;
                    if (entityAccessor.Properties.TryGetValue(prop.PropertyName, out pInfo))
                    {
                        var relInstance = pInfo.GetMethod(entity);
                        if (relInstance != null)
                        {
                            EntityAccessor relGetSet = EntityAccessorStore.GetEntityAccessor(pInfo.PropertyType, entityMap);
                            PropertyAccessor referenceProp;
                            if (relGetSet.Properties.TryGetValue(rel.ReferenceProperty, out referenceProp))
                            {
                                var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                                string paramName = paramName = BuildParameterNameWithLevel(rel.ReferenceColumn, relEntMap.TableAlias, rootLevel);

                                ParamHolder param = new ParamHolder(paramName, referenceProp.GetMethod, relInstance) { IsNullable = prop.IsNullable };

                                if (!parameters.ContainsKey(prop.ColumnName))
                                {
                                    parameters.Add(prop.ColumnName, param);
                                }

                            }
                            else
                                throw new MappingException(string.Format("Property {0} of entity {1} is referencing property {2} which was not found in {3}", prop.PropertyName, entityMap.FullName, rel.ReferenceProperty, rel.ReferenceEntityName));
                        }
                    }
                    else
                        throw new MappingException(string.Format("Property {0} was not found for entity {1}", prop.PropertyName, entityMap.FullName));
                }

                else
                {
                    PropertyAccessor pInfo;
                    if (entityAccessor.Properties.TryGetValue(prop.PropertyName, out pInfo))
                    {
                        string paramName = null;
                        if ((prop is Relation) && prop.IsPrimaryKey)
                        {
                            //we need to change the parameter namel
                            Relation rel = (Relation)prop;
                            var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                            paramName = BuildParameterNameWithLevel(rel.ReferenceColumn, relEntMap.TableAlias, level);
                        }
                        else
                        {
                            paramName = BuildParameterNameWithLevel(prop.ColumnName, entityMap.TableAlias, level);
                        }

                        ParamHolder param = new ParamHolder(paramName, pInfo.GetMethod, entity) { IsNullable = prop.IsNullable };

                        if (parameters.ContainsKey(prop.ColumnName))
                        {
                            parameters.Remove(prop.ColumnName);
                        }

                        parameters.Add(prop.ColumnName, param);
                    }
                    else
                        throw new MappingException(string.Format("Property {0} was not found for entity {1}", prop.PropertyName, entityMap.FullName));
                }
            }

            return parameters;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using Goliath.Data.DataAccess;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.Sql
{
    class InsertSqlBuilder : SqlBuilder
    {
        //Consolidate with parameter building 
        public InsertSqlBuilder(SqlDialect dialect, EntityMap entMap, int level, int rootLevel)
            : base(dialect, entMap)
        {
            foreach (var prop in entMap)
            {
                string paramName = null;

                if (prop is Relation)
                {
                    Relation rel = (Relation)prop;
                    if (rel.RelationType != RelationshipType.ManyToOne)
                        continue;

                    var relEntMap = entMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                    if (rel.IsPrimaryKey)
                    {
                        paramName = BuildParameterNameWithLevel(rel.ReferenceColumn, relEntMap.TableAlias, level);
                    }
                    else
                        paramName = BuildParameterNameWithLevel(rel.ReferenceColumn, relEntMap.TableAlias, rootLevel);
                }
                else
                {
                    paramName = BuildParameterNameWithLevel(prop.ColumnName, entMap.TableAlias, level);
                }

                if (!prop.IsAutoGenerated)
                {
                    if (Columns.ContainsKey(prop.ColumnName))
                    {
                        if (prop is Relation)
                        {
                            //Columns.Remove(prop.ColumnName);
                            continue;
                        }
                        else
                        {
                            Columns.Remove(prop.ColumnName); 
                        }
                    }
                    Columns.Add(prop.ColumnName, dialect.CreateParameterName(paramName));
                }
            }
        }

        public Dictionary<string, ParamHolder> BuildQueryParams(object entity, GetSetStore getSetStore, int level, int rootLevel)
        {
            Type entityType = entity.GetType();
            EntityGetSetInfo getSetInfo = getSetStore.GetReflectionInfoAddIfMissing(entityType, entMap);
            return BuildInsertQueryParams(entity, getSetInfo, entMap, getSetStore, level, rootLevel);
        }

        public static Dictionary<string, ParamHolder> BuildInsertQueryParams(object entity, EntityGetSetInfo getSetInfo,
            EntityMap entityMap, GetSetStore getSetStore, int level, int rootLevel)
        {
            Dictionary<string, ParamHolder> parameters = new Dictionary<string, ParamHolder>();
            foreach (var prop in entityMap)
            {
                // object val = null;
                if ((prop is Relation) && !prop.IsPrimaryKey)
                {
                    Relation rel = (Relation)prop;
                    if (rel.RelationType != RelationshipType.ManyToOne)
                        continue;

                    PropInfo pInfo;
                    if (getSetInfo.Properties.TryGetValue(prop.PropertyName, out pInfo))
                    {
                        var relInstance = pInfo.Getter(entity);
                        var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                        string paramName = BuildParameterNameWithLevel(rel.ReferenceColumn, relEntMap.TableAlias, rootLevel);
                        ParamHolder param = null;

                        if (relInstance != null)
                        {
                            EntityGetSetInfo relGetSet = getSetStore.GetReflectionInfoAddIfMissing(pInfo.PropertType, entityMap);
                            PropInfo referenceProp;
                            if (relGetSet.Properties.TryGetValue(rel.ReferenceProperty, out referenceProp))
                            {

                                param = new ParamHolder(paramName, referenceProp.Getter, relInstance) { IsNullable = prop.IsNullable };

                               
                            }
                            else
                                throw new MappingException(string.Format("Property {0} of entity {1} is referencing property {2} which was not found in {3}", prop.PropertyName, entityMap.FullName, rel.ReferenceProperty, rel.ReferenceEntityName));
                        }
                        else
                        {
                            param = new ParamHolder(paramName, null, null);
                        }

                        if (!parameters.ContainsKey(prop.ColumnName))
                        {
                            parameters.Add(prop.ColumnName, param);
                        }

                    }
                    else
                        throw new MappingException(string.Format("Property {0} was not found for entity {1}", prop.PropertyName, entityMap.FullName));
                }

                else
                {
                    PropInfo pInfo;
                    if (getSetInfo.Properties.TryGetValue(prop.PropertyName, out pInfo))
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

                        ParamHolder param = new ParamHolder(paramName, pInfo.Getter, entity) { IsNullable = prop.IsNullable };
                        //val = pInfo.Getter(entity);

                        //if ((val == null) && !prop.IsNullable)
                        //    throw new DataAccessException("{0}.{1} is cannot be null.", entityMap.Name, prop.PropertyName);

                        //param.Value = val;
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

        public override string ToSqlString()
        {
            StringBuilder sb = new StringBuilder("INSERT INTO ");
            sb.AppendFormat("{0} ({1})", entMap.TableName, string.Join(",", Columns.Keys));
            sb.AppendFormat("\nVALUES ({0})", string.Join(",", Columns.Values));

            return sb.ToString();
        }
    }
}

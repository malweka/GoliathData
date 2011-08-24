﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Goliath.Data.Diagnostics;
using Goliath.Data.DynamicProxy;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Sql;
using Goliath.Data.DataAccess;

namespace Goliath.Data.Sql
{
    class InsertSqlBuilder : SqlBuilder
    {
        //Consolidate with parameter building 
        public InsertSqlBuilder(SqlMapper sqlMapper, EntityMap entMap)
            : base(sqlMapper, entMap)
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
                    paramName = ParameterNameBuilderHelper.ColumnQueryName(rel.ReferenceColumn, relEntMap.TableAlias);
                }
                else
                {
                    paramName = ParameterNameBuilderHelper.ColumnQueryName(prop.ColumnName, entMap.TableAlias);
                }

                if (!prop.IsAutoGenerated)
                {
                    if (Columns.ContainsKey(prop.ColumnName))
                    {
                        if (prop is Relation)
                        {
                            Columns.Remove(prop.ColumnName);
                        }
                        else
                            continue;
                    }
                    Columns.Add(prop.ColumnName, sqlMapper.CreateParameterName(paramName));
                }
            }
        }

        public Dictionary<string, QueryParam> BuildQueryParams(object entity, GetSetStore getSetStore)
        {
            Type entityType = entity.GetType();
            EntityGetSetInfo getSetInfo;

            if (!getSetStore.TryGetValue(entityType, out getSetInfo))
            {
                getSetInfo = new EntityGetSetInfo(entityType);
                getSetInfo.Load(entMap);
                getSetStore.Add(entityType, getSetInfo);
            }

            return BuildQueryParams(entity, getSetInfo, entMap, getSetStore);
        }

        public static Dictionary<string, QueryParam> BuildQueryParams(object entity, EntityGetSetInfo getSetInfo, EntityMap entityMap, GetSetStore getSetStore)
        {
            Dictionary<string, QueryParam> parameters = new Dictionary<string, QueryParam>();
            foreach (var prop in entityMap)
            {
                object val = null;
                if ((prop is Relation) && !prop.IsPrimaryKey)
                {
                    Relation rel = (Relation)prop;
                    if(rel.RelationType != RelationshipType.ManyToOne)
                        continue;

                    PropInfo pInfo;
                    if (getSetInfo.Properties.TryGetValue(prop.PropertyName, out pInfo))
                    {
                        var relInstance = pInfo.Getter(entity);
                        if (relInstance != null)
                        {
                            EntityGetSetInfo relGetSet;

                            if (!getSetStore.TryGetValue(pInfo.PropertType, out relGetSet))
                            {
                                relGetSet = new EntityGetSetInfo(pInfo.PropertType);
                                relGetSet.Load(entityMap);
                                getSetStore.Add(pInfo.PropertType, relGetSet);
                            }

                            PropInfo referenceProp;
                            if (relGetSet.Properties.TryGetValue(rel.ReferenceProperty, out referenceProp))
                            {
                                var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                                QueryParam param = new QueryParam(ParameterNameBuilderHelper.ColumnQueryName(rel.ReferenceColumn, relEntMap.TableAlias));
                                val = referenceProp.Getter(relInstance);

                                if ((val == null) && !prop.IsNullable)
                                    throw new DataAccessException("{0}.{1} is cannot be null.", entityMap.Name, prop.PropertyName);
                                param.Value = val;
                                if (parameters.ContainsKey(prop.ColumnName))
                                {
                                    parameters.Remove(prop.ColumnName);
                                }

                                parameters.Add(prop.ColumnName, param);

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
                    PropInfo pInfo;
                    if (getSetInfo.Properties.TryGetValue(prop.PropertyName, out pInfo))
                    {
                        string paramName = ParameterNameBuilderHelper.ColumnQueryName(prop.ColumnName, entityMap.TableAlias);
                        if ((prop is Relation) && prop.IsPrimaryKey)
                        {
                            //we need to change the parameter namel
                            Relation rel = (Relation)prop;
                            var relEntMap = entityMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                            paramName = ParameterNameBuilderHelper.ColumnQueryName(rel.ReferenceColumn, relEntMap.TableAlias);
                        }

                        QueryParam param = new QueryParam(paramName);
                        val = pInfo.Getter(entity);

                        if ((val == null) && !prop.IsNullable)
                            throw new DataAccessException("{0}.{1} is cannot be null.", entityMap.Name, prop.PropertyName);

                        param.Value = val;
                        if (parameters.ContainsKey(prop.ColumnName))
                        {
                            //var containedParam = parameters[prop.ColumnName];
                            //if ((containedParam.Value == null) && (val != null))
                            //{
                            //    containedParam.Value = val;
                            //}
                            continue;
                        }
                        else
                        {
                            parameters.Add(prop.ColumnName, param);
                        }
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

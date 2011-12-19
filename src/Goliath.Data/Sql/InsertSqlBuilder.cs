﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Goliath.Data.Sql
{
    using DataAccess;
    using Mapping;
    using Providers;

    class InsertSqlBuilder : SqlBuilder
    {
        //Consolidate with parameter building 
        public InsertSqlBuilder(SqlMapper sqlMapper, EntityMap entMap, int level, int rootLevel)
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
                            Columns.Remove(prop.ColumnName);
                        }
                        else
                            continue;
                    }
                    Columns.Add(prop.ColumnName, sqlMapper.CreateParameterName(paramName));
                }
            }
        }

        public Dictionary<string, ParamHolder> BuildQueryParams(object entity, GetSetStore getSetStore, int level, int rootLevel)
        {
            Type entityType = entity.GetType();
            EntityGetSetInfo getSetInfo;

            if (!getSetStore.TryGetValue(entityType, out getSetInfo))
            {
                getSetInfo = new EntityGetSetInfo(entityType);
                getSetInfo.Load(entMap);
                getSetStore.Add(entityType, getSetInfo);
            }

            return BuildInsertQueryParams(entity, getSetInfo, entMap, getSetStore, level, rootLevel);
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

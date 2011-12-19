using System;
using System.Collections.Generic;

namespace Goliath.Data.Sql
{
    using DataAccess;
    using Mapping;
    using Providers;

    abstract class SqlBuilder
    {
        protected SqlMapper sqlMapper;
        protected EntityMap entMap;

        public SqlMapper SqlMapper
        {
            get { return sqlMapper; }
        }

        readonly Dictionary<string, string> columns = new Dictionary<string, string>();
        internal Dictionary<string, string> Columns
        {
            get { return columns; }
        }

        protected SqlBuilder(SqlMapper sqlMapper, EntityMap entMap)
        {
            if (sqlMapper == null)
                throw new ArgumentNullException("sqlMapper");

            if (entMap == null)
                throw new ArgumentNullException("entMap");

            this.sqlMapper = sqlMapper;
            this.entMap = entMap;
        }


        public static string BuildParameterNameWithLevel(string columnName, string tableAlias, int level)
        {
            return string.Format("{0}_{1}", ParameterNameBuilderHelper.ColumnQueryName(columnName, tableAlias), level);
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
                                string paramName = paramName = BuildParameterNameWithLevel(rel.ReferenceColumn, relEntMap.TableAlias, rootLevel);

                                ParamHolder param = new ParamHolder(paramName, referenceProp.Getter, relInstance) { IsNullable = prop.IsNullable };
                                //val = referenceProp.Getter(relInstance);

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

        public static Dictionary<string, ParamHolder> BuildUpdateQueryParams(object entity, EntityGetSetInfo getSetInfo,
           EntityMap entityMap, GetSetStore getSetStore, int level, int rootLevel)
        {
            Dictionary<string, ParamHolder> parameters = new Dictionary<string, ParamHolder>();
            foreach (var prop in entityMap)
            {
                if (prop.IsPrimaryKey || prop.IgnoreOnUpdate)
                    continue;

                else if (prop is Relation)
                {
                    Relation rel = (Relation)prop;
                    if (rel.RelationType != RelationshipType.ManyToOne)
                        continue;
                }


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

                    if (parameters.ContainsKey(prop.ColumnName))
                    {
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


            return parameters;
        }

        public abstract string ToSqlString();



        public override string ToString()
        {
#if DEBUG
            return base.ToString();
#else 
            return ToSqlString();
#endif
        }
    }
}

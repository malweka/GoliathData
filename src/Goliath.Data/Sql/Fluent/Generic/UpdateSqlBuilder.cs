﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Goliath.Data.Collections;
using Goliath.Data.Entity;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    class UpdateSqlBuilder<T> : INonQuerySqlBuilder<T>, IBinaryNonQueryOperation<T>
    {
        private ISession session;
        private T entity;

        private EntityMap Table { get; set; }
        public List<NonQueryFilterClause<T>> Filters { get; private set; }

        public UpdateSqlBuilder(ISession session, T entity)
        {
            this.session = session;
            this.entity = entity;
            var type = typeof(T);
            Filters = new List<NonQueryFilterClause<T>>();
            Table = session.SessionFactory.DbSettings.Map.GetEntityMap(type.FullName);

        }

        void LoadColumns(UpdateSqlExecutionList execList, EntityMap entityMap, EntityAccessor accessor)
        {
            var updateBodyInfo = new UpdateSqlBodyInfo();

            var trackable = entity as ITrackable;
            if (trackable != null)
            {
                var changes = trackable.ChangeTracker.GetChangedItems();
                foreach (var item in changes)
                {
                    var prop = entityMap.GetProperty(item.ItemName);
                    if (prop == null)
                    {
                        if (entityMap.IsSubClass && !execList.Statements.ContainsKey(entityMap.Extends))
                        {
                            var parentMap = session.SessionFactory.DbSettings.Map.GetEntityMap(entityMap.Extends);
                            LoadColumns(execList, parentMap, accessor);
                        }
                        else
                            continue;
                    }

                    if (prop == null || prop.IgnoreOnUpdate || prop.IsPrimaryKey)
                        continue;

                    var propInfo = accessor.Properties[prop.Name];
                    if (propInfo == null)
                        throw new MappingException("Could not find mapped property " + prop.Name + " inside " + entityMap.FullName);

                    AddColumnAndParameterToUpdateInfo(execList, updateBodyInfo, entityMap, prop, propInfo, accessor);
                }
            }
            else
            {
                foreach (var pInfo in accessor.Properties)
                {
                    var prop = entityMap.GetProperty(pInfo.Value.PropertyName);
                    if (prop == null)
                    {
                        if (entityMap.IsSubClass && !execList.Statements.ContainsKey(entityMap.Extends))
                        {
                            var parentMap = session.SessionFactory.DbSettings.Map.GetEntityMap(entityMap.Extends);
                            LoadColumns(execList, parentMap, accessor);
                        }
                        else
                            continue; //throw new MappingException("Could not find mapped property " + pInfo.Value.PropertyName + " inside " + entityMap.FullName);
                    }

                    if (prop == null || prop.IgnoreOnUpdate || prop.IsPrimaryKey)
                        continue;

                    AddColumnAndParameterToUpdateInfo(execList, updateBodyInfo, entityMap, prop, pInfo.Value, accessor);
                }
            }

            execList.Statements.Add(entityMap.FullName, updateBodyInfo);

        }

        void AddColumnAndParameterToUpdateInfo(UpdateSqlExecutionList execList, UpdateSqlBodyInfo updateBodyInfo, EntityMap entityMap, Property prop, PropertyAccessor propInfo, EntityAccessor accessor)
        {
            object val = propInfo.GetMethod(entity);
            bool isRel = false;
            if (prop is Relation)
            {
                var rel = (Relation)prop;
                if (updateBodyInfo.Columns.ContainsKey(prop.ColumnName))
                    return;

                isRel = true;
                if (val != null)
                {
                    if (rel.RelationType == RelationshipType.ManyToOne)
                    {
                        var store = new EntityAccessorStore();
                        var relMap = session.SessionFactory.DbSettings.Map.GetEntityMap(rel.ReferenceEntityName);
                        var relAccessor = store.GetEntityAccessor(val.GetType(), relMap);

                        var relPinfo = relAccessor.Properties[rel.ReferenceProperty];
                        if (relPinfo == null)
                            throw new MappingException(string.Format("could not find property {0} in mapped entity {1}", rel.ReferenceProperty, relMap.FullName));
                        else
                        {
                            val = relPinfo.GetMethod(val);
                        }
                    }
                    else if (rel.RelationType == RelationshipType.ManyToMany)
                    {
                        var trackableCollection = val as ITrackableCollection;
                        if (trackableCollection != null)
                        {
                            AddInsertManyToManyOperation(execList, trackableCollection.InsertedItems, rel, accessor, true);
                            AddInsertManyToManyOperation(execList, trackableCollection.DeletedItems, rel, accessor, false);
                        }

                        //NOTE: if not trackable collection used mapped statement to add or remove many to many associations
                    }
                    else return;
                }
            }
            else
            {
                Tuple<QueryParam, bool> etuple;
                if (updateBodyInfo.Columns.TryGetValue(prop.ColumnName, out etuple))
                {
                    if (etuple.Item2)
                    {
                        updateBodyInfo.Columns.Remove(prop.ColumnName);
                    }
                    else return;
                }
            }

            Tuple<QueryParam, bool> tuple = Tuple.Create(new QueryParam(ParameterNameBuilderHelper.ColumnWithTableAlias(entityMap.TableAlias, prop.ColumnName)) { Value = val }, isRel);
            updateBodyInfo.Columns.Add(prop.ColumnName, tuple);
        }

        void AddInsertManyToManyOperation(UpdateSqlExecutionList execList, IEnumerable insertedItems, Relation rel, EntityAccessor accessor, bool isInsertStatement)
        {
            if (insertedItems != null)
            {
                var relMap = session.SessionFactory.DbSettings.Map.GetEntityMap(rel.ReferenceEntityName);
                var store = new EntityAccessorStore();

                var propInfo = accessor.Properties[rel.MapPropertyName];
                if(propInfo == null)
                    throw new GoliathDataException(string.Format("Could not retrieve value of property {0} in mapped entity {1} with clr type {2}.",
                                rel.MapPropertyName, Table.FullName, typeof(T).FullName));

                var propValue = propInfo.GetMethod(entity);
                 
                PropertyAccessor relPropInfo = null;
                foreach (var item in insertedItems)
                {
                    if (relPropInfo == null)
                    {
                        var relAccessor = store.GetEntityAccessor(item.GetType(), relMap);
                        if (!relAccessor.Properties.TryGetValue(rel.ReferenceProperty, out relPropInfo))
                            throw new GoliathDataException(string.Format("Could not retrieve value of property {0} in mapped entity {1} with clr type {2}.", 
                                rel.ReferenceProperty, relMap.FullName, item.GetType().FullName));
                    }

                    var relParam = new QueryParam(rel.MapReferenceColumn) {Value = relPropInfo.GetMethod(item)};
                    var propParm = new QueryParam(rel.MapColumn) {Value = propValue};
                    var dialect = session.SessionFactory.DbSettings.SqlDialect;
                    if(isInsertStatement)
                    {
                        var sql = string.Format("INSERT INTO {0} ({1}, {2}) VALUES({3},{4})",
                                                rel.MapTableName, rel.MapColumn, rel.MapReferenceColumn, dialect.CreateParameterName(propParm.Name), dialect.CreateParameterName(relParam.Name));

                        execList.ManyToManyStatements.Add(Tuple.Create(sql, new List<QueryParam>(){propParm, relParam}));
                    }
                    else
                    {
                        var sql = string.Format("DELETE FROM {0} WHERE {1} = {3} AND {2} = {4}",
                                                rel.MapTableName, rel.MapColumn, rel.MapReferenceColumn, dialect.CreateParameterName(propParm.Name), dialect.CreateParameterName(relParam.Name));

                        execList.ManyToManyStatements.Add(Tuple.Create(sql, new List<QueryParam>() { propParm, relParam }));
                    }
                }
            }
        }

        internal UpdateSqlExecutionList Build()
        {
            var execList = new UpdateSqlExecutionList();
            var type = typeof(T);
            var store = new EntityAccessorStore();
            var accessor = store.GetEntityAccessor(type, Table);

            LoadColumns(execList, Table, accessor);

            return execList;
        }

        NonQueryFilterClause<T> CreateFilter(EntityMap map, SqlOperator preOperator, string propertyName)
        {
            var prop = map.GetProperty(propertyName);

            //NOTE: we're not supporting this yet. so we shouldn't really be going down to the parent class yet.
            if (prop == null)
            {
                //if (map.IsSubClass)
                //{
                //    var parent = session.SessionFactory.DbSettings.Map.GetEntityMap(map.Extends);
                //    return CreateFilter(parent, preOperator, propertyName);
                //}
                //else
                //{
                throw new MappingException(string.Format("Could not find property {0} on mapped entity {1}", propertyName, map.FullName));
                //}

            }

            var filter = new NonQueryFilterClause<T>(prop, this) { PreOperator = preOperator };
            Filters.Add(filter);
            return filter;
        }

        #region INonQuerySqlBuilder<T> Members

        public IFilterNonQueryClause<T> Where<TProperty>(string propertyName)
        {
            return CreateFilter(Table, SqlOperator.AND, propertyName);
        }

        public IFilterNonQueryClause<T> Where<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return CreateFilter(Table, SqlOperator.AND, property.GetMemberName());
        }

        #endregion

        #region IBinaryNonQueryOperation<T> Members

        public IFilterNonQueryClause<T> And<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return CreateFilter(Table, SqlOperator.AND, property.GetMemberName());
        }

        public IFilterNonQueryClause<T> And<TProperty>(string propertyName)
        {
            return CreateFilter(Table, SqlOperator.AND, propertyName);
        }

        public IFilterNonQueryClause<T> Or<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return CreateFilter(Table, SqlOperator.OR, property.GetMemberName());
        }

        public IFilterNonQueryClause<T> Or<TProperty>(string propertyName)
        {
            return CreateFilter(Table, SqlOperator.OR, propertyName);
        }

        public int Execute()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    class NonQueryFilterClause<T> : IFilterNonQueryClause<T>
    {
        private readonly IBinaryNonQueryOperation<T> nonQueryBuilder;
        public Property LeftColumn { get; private set; }
        public string RightColumnName { get; private set; }
        public object RightValue { get; private set; }
        public ComparisonOperator BinaryOp { get; private set; }
        public SqlOperator PreOperator { get; set; }

        public NonQueryFilterClause(Property property, IBinaryNonQueryOperation<T> nonQueryBuilder)
        {
            LeftColumn = property;
            this.nonQueryBuilder = nonQueryBuilder;
        }

        #region IFilterNonQueryClause<T,TProperty> Members

        public IBinaryNonQueryOperation<T> EqualToValue<TProperty>(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.Equal;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> EqualTo<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.Equal;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterThanValue<TProperty>(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.GreaterThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterThan<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.GreaterThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterOrEqualToValue<TProperty>(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.GreaterOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterOrEqualTo<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.GreaterOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerOrEqualToValue<TProperty>(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.LowerOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerOrEqualTo<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.LowerOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerThanValue<TProperty>(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.LowerThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerThan<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.LowerThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LikeValue<TProperty>(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.Like;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> Like<TProperty>(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.Like;
            return nonQueryBuilder;
        }

        #endregion
    }
}

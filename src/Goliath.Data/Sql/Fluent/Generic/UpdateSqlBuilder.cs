using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Goliath.Data.Entity;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    class UpdateSqlBuilder<T> : INonQuerySqlBuilder<T>, IBinaryNonQueryOperation<T>
    {
        UpdateSqlExecutionList executionList = new UpdateSqlExecutionList();
        private ISession session;
        private T entity;


        internal UpdateSqlExecutionList ExecutionList { get { return executionList; } }

        public UpdateSqlBuilder(ISession session, T entity)
        {
            this.session = session;
            this.entity = entity;
            var type = typeof(T);
            EntityMap entityMap = session.SessionFactory.DbSettings.Map.GetEntityMap(type.FullName);
            EntityAccessorStore store = new EntityAccessorStore();
            var accessor = store.GetEntityAccessor(type, entityMap);

            LoadColumns(entityMap, accessor);
        }

        void LoadColumns(EntityMap entityMap, EntityAccessor accessor)
        {
            if (entityMap.IsSubClass)
            {
                var parentMap = session.SessionFactory.DbSettings.Map.GetEntityMap(entityMap.Extends);
                LoadColumns(parentMap, accessor);
            }

            //var trackable = entity as ITrackable;
            //if (trackable != null)
            //{
            //    var changes = trackable.ChangeTracker.GetChangedItems();
            //    foreach (var item in changes)
            //    {
            //        var prop = entityMap.GetProperty(item.ItemName);
            //        if(prop.IgnoreOnUpdate)
            //            continue;

            //        var propInfo = accessor.Properties[prop.Name];
            //        if (propInfo == null)
            //            throw new MappingException("Could not find mapped property " + prop.Name + " inside " + entityMap.FullName);

            //        executionList.AddColumn(entityMap.FullName, prop, propInfo.GetMethod(entity));
            //    }
            //}

            foreach (var prop in entityMap.Properties)
            {
                if (prop.IgnoreOnUpdate)
                    continue;

                var propInfo = accessor.Properties[prop.Name];
                if (propInfo == null)
                    throw new MappingException("Could not find mapped property " + prop.Name + " inside " + entityMap.FullName);

                executionList.AddColumn(entityMap.FullName, prop, propInfo.GetMethod(entity));
            }

        }

        #region INonQuerySqlBuilder<T> Members

        public IFilterNonQueryClause<T, TProperty> Where<TProperty>(string propertyName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBinaryNonQueryOperation<T> Members

        public IFilterNonQueryClause<T, TProperty> And<TProperty>(Expression<Func<T, TProperty>> property)
        {
            throw new NotImplementedException();
        }

        public IFilterNonQueryClause<T, TProperty> Or<TProperty>(Expression<Func<T, TProperty>> property)
        {
            throw new NotImplementedException();
        }

        public int Execute()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    class NonQueryFilterClause<T, TProperty> : IFilterNonQueryClause<T, TProperty>
    {
        private readonly IBinaryNonQueryOperation<T> nonQueryBuilder;
        public Property LeftColumn { get; private set; }
        public string RightColumnName { get; private set; }
        public TProperty RightValue { get; private set; }
        public ComparisonOperator BinaryOp { get; private set; }
        public SqlOperator PreOperator { get; set; }

        public NonQueryFilterClause(Property property, IBinaryNonQueryOperation<T> nonQueryBuilder)
        {
            LeftColumn = property;
            this.nonQueryBuilder = nonQueryBuilder;
        }

        #region IFilterNonQueryClause<T,TProperty> Members

        public IBinaryNonQueryOperation<T> EqualToValue(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.Equal;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> EqualTo(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.Equal;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterThanValue(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.GreaterThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterThan(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.GreaterThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterOrEqualToValue(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.GreaterOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> GreaterOrEqualTo(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.GreaterOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerOrEqualToValue(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.LowerOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerOrEqualTo(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.LowerOrEquals;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerThanValue(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.LowerThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LowerThan(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.LowerThan;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> LikeValue(TProperty value)
        {
            RightValue = value;
            BinaryOp = ComparisonOperator.Like;
            return nonQueryBuilder;
        }

        public IBinaryNonQueryOperation<T> Like(Expression<Func<T, TProperty>> property)
        {
            RightColumnName = property.GetMemberName();
            BinaryOp = ComparisonOperator.Like;
            return nonQueryBuilder;
        }

        #endregion
    }
}

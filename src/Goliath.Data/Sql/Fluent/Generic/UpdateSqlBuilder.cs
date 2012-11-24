using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    class UpdateSqlBuilder<T> : INonQuerySqlBuilder<T>, IBinaryNonQueryOperation<T>
    {
        UpdateSqlExecutionList executionList = new UpdateSqlExecutionList();
        private ISession session;
        //private List<string> columns;
        private T entity;


        public UpdateSqlBuilder(ISession session, T entity)
        {
            this.session = session;
            this.entity = entity;
            var type = typeof(T);
            EntityMap entityMap = session.SessionFactory.DbSettings.Map.GetEntityMap(type.FullName);
            EntityAccessorStore store = new EntityAccessorStore();
            var accessor = store.GetEntityAccessor(type, entityMap);
            
            LoadColumns(entityMap, accessor, entity);
        }

        void LoadColumns(EntityMap entityMap, EntityAccessor accessor, T entity)
        {
            if(entityMap.IsSubClass)
            {
                var parentMap = session.SessionFactory.DbSettings.Map.GetEntityMap(entityMap.Extends);
                LoadColumns(parentMap, accessor, entity);
            }

            foreach (var prop in entityMap.Properties)
            {
                var propInfo = accessor.Properties[prop.Name];
                if(propInfo == null)
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

        public IFilterNonQueryClause<T, TProperty> And<TProperty>(
            Expression<Func<T, TProperty>> property)
        {
            throw new NotImplementedException();
        }

        public IFilterNonQueryClause<T, TProperty> Or<TProperty>(
            Expression<Func<T, TProperty>> property)
        {
            throw new NotImplementedException();
        }

        public int Execute()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateSqlExecutionList
    {
        readonly Dictionary<string, UpdateSqlBodyInfo> statements = new Dictionary<string, UpdateSqlBodyInfo>();

        private readonly Dictionary<string, Tuple<string, string, object>> columnsTableMap = new Dictionary<string, Tuple<string, string, object>>();

        public Dictionary<string, UpdateSqlBodyInfo> Statements
        {
            get { return statements; }
        }

        public void AddColumn(string entityMapName, Property property, object value)
        {
            if (columnsTableMap.ContainsKey(property.Name))
                throw new MappingException("Entity " + entityMapName + " contains more than one property named " + property.PropertyName);

            columnsTableMap.Add(property.Name, Tuple.Create(entityMapName, property.ColumnName, value));

        }
    }

}

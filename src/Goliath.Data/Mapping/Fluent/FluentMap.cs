using System;
using System.Linq.Expressions;

namespace Goliath.Data.Mapping.Fluent
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFluentMap
    {
        /// <summary>
        /// Builds the entity map.
        /// </summary>
        /// <returns></returns>
        EntityMap BuildEntityMap();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMap<T>
    {
        /// <summary>
        /// Columns the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        T ColumnName(string name);

        /// <summary>
        /// Nullables the specified is nullable.
        /// </summary>
        /// <param name="isNullable">if set to <c>true</c> [is nullable].</param>
        /// <returns></returns>
        T Nullable(bool isNullable);

        /// <summary>
        /// Withes the constrain.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns></returns>
        T WithConstrain(string constraint);

        /// <summary>
        /// Withes the type of the SQL.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        T WithSqlType(System.Data.SqlDbType dbType);

        /// <summary>
        /// Withes the type of the db.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        T WithDbType(string dbType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class FluentMap<TEntity> : IFluentMap
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMap&lt;TEntity&gt;"/> class.
        /// </summary>
        protected FluentMap()
        {

        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public abstract string TableName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is linked table.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is linked table; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsLinkedTable { get; }

        /// <summary>
        /// Gets the schema.
        /// </summary>
        public virtual string Schema
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the alias.
        /// </summary>
        public virtual string Alias
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the namespace.
        /// </summary>
        public virtual string Namespace
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Builds the entity map.
        /// </summary>
        /// <returns></returns>
        public EntityMap BuildEntityMap()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps the property.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns></returns>
        protected PropertyMap MapProperty(Expression<Func<TEntity, object>> propertyExpression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps the primary key.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns></returns>
        protected PrimaryKeyMap MapPrimaryKey(Expression<Func<TEntity, object>> propertyExpression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps the reference property.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns></returns>
        protected ReferenceMap MapReferenceProperty(Expression<Func<TEntity, object>> propertyExpression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps the list property.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns></returns>
        protected ListMap MapListProperty(Expression<Func<TEntity, object>> propertyExpression)
        {
            throw new NotImplementedException();
        }

        //        public PropertyPart Map(Expression<Func<T, object>> memberExpression, string columnName)
        //        {
        //#pragma warning disable 612,618
        //            return Map(memberExpression.ToMember(), columnName);
        //#pragma warning restore 612,618
        //        }

        //        [Obsolete("Do not call this method. Implementation detail mistakenly made public. Will be made private in next version.")]
        //        protected virtual PropertyPart Map(Member property, string columnName)
        //        {
        //            var propertyMap = new PropertyPart(property, typeof(T));

        //            if (!string.IsNullOrEmpty(columnName))
        //                propertyMap.Column(columnName);

        //            providers.Properties.Add(propertyMap);

        //            return propertyMap;
        //        }

    }

}


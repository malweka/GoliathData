using System;
using System.Linq.Expressions;

namespace Goliath.Data.Mapping.Fluent
{
	public interface IFluentMap
	{
		EntityMap BuildEntityMap ();
	}
	
	public interface IMap<T>
	{
		T ColumnName (string name);

		T Nullable (bool isNullable);

		T WithConstrain (string constraint);

		T WithSqlType (System.Data.SqlDbType dbType);

		T WithDbType (string dbType);
	}

	public abstract class FluentMap< TEntity> : IFluentMap
	{

		protected FluentMap ()
		{

		}

		public abstract string TableName { get; }

		public abstract bool IsLinkedTable { get; }

		public virtual string Schema {
			get { return string. Empty; }
		}

		public virtual string Alias {
			get { return string. Empty; }
		}

		public virtual string Namespace {
			get { return string. Empty; }
		}

		public EntityMap BuildEntityMap ()
		{
			throw new NotImplementedException ();
		}

		protected PropertyMap MapProperty (Expression< Func<TEntity, object>> propertyExpression)
		{
			throw new NotImplementedException ();
		}

		protected PrimaryKeyMap MapPrimaryKey (Expression< Func<TEntity, object>> propertyExpression)
		{
			throw new NotImplementedException ();
		}

		protected ReferenceMap MapReferenceProperty (Expression< Func<TEntity, object>> propertyExpression)
		{
			throw new NotImplementedException ();
		}

		protected ListMap MapListProperty (Expression< Func<TEntity, object>> propertyExpression)
		{
			throw new NotImplementedException ();
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


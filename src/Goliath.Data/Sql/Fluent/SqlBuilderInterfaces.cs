using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITableNameBuilder
    {
        /// <summary>
        /// Froms the specified table name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        IQueryBuilder From(string tableName);
        /// <summary>
        /// Froms the specified table name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        IQueryBuilder From(string tableName, string alias);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Goliath.Data.Sql.IQueryFetchable" />
    /// <seealso cref="Goliath.Data.Sql.IOrderByDirection" />
    public interface IQueryBuilder : IQueryFetchable, IOrderByDirection
    {
        /// <summary>
        /// Inners the join.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        IJoinable InnerJoin(string tableName, string alias);
        /// <summary>
        /// Lefts the join.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        IJoinable LeftJoin(string tableName, string alias);
        /// <summary>
        /// Rights the join.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        IJoinable RightJoin(string tableName, string alias);

        /// <summary>
        /// Wheres the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFilterClause Where(string propertyName);
        /// <summary>
        /// Wheres the specified table alias.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFilterClause Where(string tableAlias, string propertyName);
    }


    /// <summary>
    /// 
    /// </summary>
    public interface IJoinable
    {
        /// <summary>
        /// Ons the specified table alias.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IJoinOperation On(string tableAlias, string propertyName);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IJoinOperation
    {
        /// <summary>
        /// Equals to.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IQueryBuilder EqualTo(string propertyName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Goliath.Data.Sql.IQueryFetchable" />
    /// <seealso cref="Goliath.Data.Sql.IOrderByDirection" />
    public interface IBinaryOperation : IQueryFetchable, IOrderByDirection
    {
        /// <summary>
        /// Ands the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFilterClause And(string propertyName);
        /// <summary>
        /// Ands the specified table alias.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFilterClause And(string tableAlias, string propertyName);
        /// <summary>
        /// Ors the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFilterClause Or(string propertyName);
        /// <summary>
        /// Ors the specified table alias.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFilterClause Or(string tableAlias, string propertyName);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IFilterClause
    {
        /// <summary>
        /// Equals to value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryOperation EqualToValue(object value);
        /// <summary>
        /// Equals to.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation EqualTo(string propertyName);
        /// <summary>
        /// Equals to.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation EqualTo(string tableAlias, string propertyName);

        /// <summary>
        /// Greaters the than value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryOperation GreaterThanValue(object value);
        /// <summary>
        /// Greaters the than.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation GreaterThan(string propertyName);
        /// <summary>
        /// Greaters the than.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation GreaterThan(string tableAlias, string propertyName);

        /// <summary>
        /// Greaters the or equal to value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryOperation GreaterOrEqualToValue(object value);
        /// <summary>
        /// Greaters the or equal to.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation GreaterOrEqualTo(string propertyName);
        /// <summary>
        /// Greaters the or equal to.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation GreaterOrEqualTo(string tableAlias, string propertyName);

        /// <summary>
        /// Lowers the or equal to value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryOperation LowerOrEqualToValue(object value);
        /// <summary>
        /// Lowers the or equal to.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation LowerOrEqualTo(string propertyName);
        /// <summary>
        /// Lowers the or equal to.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation LowerOrEqualTo(string tableAlias, string propertyName);

        /// <summary>
        /// Lowers the than value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryOperation LowerThanValue(object value);
        /// <summary>
        /// Lowers the than.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation LowerThan(string propertyName);
        /// <summary>
        /// Lowers the than.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation LowerThan(string tableAlias, string propertyName);

        /// <summary>
        /// Likes the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryOperation LikeValue(object value);
        /// <summary>
        /// Likes the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation Like(string propertyName);
        /// <summary>
        /// Likes the specified table alias.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation Like(string tableAlias, string propertyName);

        /// <summary>
        /// is the like value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryOperation ILikeValue(object value);
        /// <summary>
        /// is the like.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation ILike(string propertyName);
        /// <summary>
        /// is the like.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryOperation ILike(string tableAlias, string propertyName);

        /// <summary>
        /// Determines whether this instance is null.
        /// </summary>
        /// <returns></returns>
        IBinaryOperation IsNull();
        /// <summary>
        /// Determines whether [is not null].
        /// </summary>
        /// <returns></returns>
        IBinaryOperation IsNotNull();
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ISorterClause
    {
        /// <summary>
        /// Descs this instance.
        /// </summary>
        /// <returns></returns>
        IOrderByDirection Desc();
        /// <summary>
        /// Ascs this instance.
        /// </summary>
        /// <returns></returns>
        IOrderByDirection Asc();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Goliath.Data.Sql.IQueryFetchable" />
    public interface IOrderByDirection : IQueryFetchable
    {
        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        ISorterClause OrderBy(string propertyName);
        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        ISorterClause OrderBy(string tableAlias, string propertyName);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IFetchable
    {
        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ICollection<T> FetchAll<T>();
        /// <summary>
        /// Fetches the one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T FetchOne<T>();
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IFetchableWithOutput
    {
        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="total">The total.</param>
        /// <returns></returns>
        ICollection<T> FetchAll<T>(out long total);
        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ICollection<T> FetchAll<T>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Goliath.Data.Sql.IFetchable" />
    public interface IQueryFetchable : IFetchable
    {
        /// <summary>
        /// Takes the specified limit.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        IFetchableWithOutput Take(int limit, int offset);
    }

}

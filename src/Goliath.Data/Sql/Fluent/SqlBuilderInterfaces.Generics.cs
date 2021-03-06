﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Goliath.Data.Mapping;

namespace Goliath.Data.Sql
{

    public interface IQueryBuilder<T> : IQueryFetchable<T>, IOrderByDirection<T>
    {
        EntityMap Table { get; }
        IJoinable<T, TRelation> InnerJoin<TRelation>();

        IJoinable<T, TRelation> LeftJoin<TRelation>();

        IJoinable<T, TRelation> RightJoin<TRelation>();

        IFilterClause<T, TProperty> Where<TProperty>(Expression<Func<T, TProperty>> property);

        IFilterClause Where(string propertyName);
    }


    public interface IJoinable<T, TRelation>
    {
        IJoinOperation<T> On<TProperty>(Expression<Func<TRelation, TProperty>> property);
    }

    public interface IJoinOperation<T>
    {
        IQueryBuilder<T> EqualTo<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IBinaryOperation<T> : IQueryFetchable<T>, IOrderByDirection<T>
    {
        IFilterClause<T, TProperty> And<TProperty>(Expression<Func<T, TProperty>> property);
        IFilterClause<T, TProperty> Or<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IFilterClause<T, TValue>
    {
        IBinaryOperation<T> EqualToValue(TValue value);
        IBinaryOperation<T> EqualTo(Expression<Func<T, TValue>> property);

        IBinaryOperation<T> GreaterThanValue(TValue value);
        IBinaryOperation<T> GreaterThan(Expression<Func<T, TValue>> property);

        IBinaryOperation<T> GreaterOrEqualToValue(TValue value);
        IBinaryOperation<T> GreaterOrEqualTo(Expression<Func<T, TValue>> property);

        IBinaryOperation<T> LowerOrEqualToValue(TValue value);
        IBinaryOperation<T> LowerOrEqualTo(Expression<Func<T, TValue>> property);

        IBinaryOperation<T> LowerThanValue(TValue value);

        IBinaryOperation<T> LowerThan(Expression<Func<T, TValue>> property);

        IBinaryOperation<T> LikeValue(TValue value);

        IBinaryOperation<T> Like(Expression<Func<T, TValue>> property);

        IBinaryOperation<T> ILikeValue(TValue value);
        IBinaryOperation<T> ILike(Expression<Func<T, TValue>> property);

        IBinaryOperation<T> IsNull();
        IBinaryOperation<T> IsNotNull();
    }

    public interface IJoinFilterClause<T, TRelation, TValue>
    {
        IJoinBinaryOperation<T, TRelation> EqualTo(TValue value);
        IJoinBinaryOperation<T, TRelation> EqualTo(Expression<Func<TRelation, TValue>> property);

        IJoinBinaryOperation<T, TRelation> GreaterThan(TValue value);
        IJoinBinaryOperation<T, TRelation> GreaterThan(Expression<Func<TRelation, TValue>> property);

        IJoinBinaryOperation<T, TRelation> GreaterOrEqualTo(TValue value);
        IJoinBinaryOperation<T, TRelation> GreaterOrEqualTo(Expression<Func<TRelation, TValue>> property);

        IJoinBinaryOperation<T, TRelation> LowerOrEqualTo(TValue value);
        IJoinBinaryOperation<T, TRelation> LowerOrEqualTo(Expression<Func<TRelation, TValue>> property);

        IJoinBinaryOperation<T, TRelation> LowerThan(TValue value);
        IJoinBinaryOperation<T, TRelation> LowerThan(Expression<Func<TRelation, TValue>> property);

        IJoinBinaryOperation<T, TRelation> Like(TValue value);
        IJoinBinaryOperation<T, TRelation> Like(Expression<Func<TRelation, TValue>> property);
    }

    public interface IJoinBinaryOperation<T, TRelation> : IBinaryOperation<T>
    {
        IOnBinaryOperation<T, TTRelation> ForJoin<TTRelation>();
    }

    public interface IOnBinaryOperation<T, TRelation>
    {
        IJoinFilterClause<T, TRelation, TProperty> And<TProperty>(Expression<Func<TRelation, TProperty>> property);
        IJoinFilterClause<T, TRelation, TProperty> Or<TProperty>(Expression<Func<TRelation, TProperty>> property);
        IJoinFilterClause<T, TRelation, TProperty> OrderBy<TProperty>(Expression<Func<TRelation, TProperty>> property);
    }

    public interface ISorterClause<T>
    {
        IOrderByDirection<T> Desc();
        IOrderByDirection<T> Asc();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Goliath.Data.Sql.IQueryFetchable{T}" />
    public interface IOrderByDirection<T> : IQueryFetchable<T>
    {
        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        ISorterClause<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> property);
        /// <summary>
        /// Orders the by.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        ISorterClause<T> OrderBy(string columnName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFetchable<T>
    {
        /// <summary>
        /// Fetches all.
        /// </summary>
        /// <returns></returns>
        ICollection<T> FetchAll();

        T FetchOne();

        int Count();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFetchableWithOutput<T>
    {
        ICollection<T> FetchAll(out long total);

        ICollection<T> FetchAll();

    }

    public interface IQueryFetchable<T> : IFetchable<T>
    {
        IFetchableWithOutput<T> Take(int limit, int offset);
    }
}

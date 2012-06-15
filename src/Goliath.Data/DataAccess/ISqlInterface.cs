using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Goliath.Data 
{
    public interface ISqlInterface
    {
        IQueryBuilder<T> From<T>();
    }

    public interface IQueryBuilder<T>
    {
        IFilterClause<T> Where<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IUpdateBuilder
    {

    }

    public interface IDeleteBuilder
    {

    }

    public interface IBinaryOperation<T> : IQueryFetchable<T>
    {
        IFilterClause<T> And<TProperty>(Expression<Func<T, TProperty>> property);
        IFilterClause<T> Or<TProperty>(Expression<Func<T, TProperty>> property);
        ISorterClause<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IFilterClause<T>
    {
        IBinaryOperation<T> EqualTo<VType>(VType value);
        IBinaryOperation<T> GreaterThan<VType>(VType value);
        IBinaryOperation<T> GreaterOrEqualTo<VType>(VType value);
        IBinaryOperation<T> LowerOrEqualTo<VType>(VType obj);
        IBinaryOperation<T> LowerThan<VType>(VType obj);
        IBinaryOperation<T> Like<VType>(VType param);
    }

    public interface ISorterClause<T>
    {
        IOrderByDirection<T> Desc();
        IOrderByDirection<T> Asc();
    }

    public interface IOrderByDirection<T> : IQueryFetchable<T>
    {
        ISorterClause<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IQueryFetchable<T>
    {
        ICollection<T> FetchAll();
        T FetchOne();
    }

}

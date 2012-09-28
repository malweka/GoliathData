using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Goliath.Data.Sql
{

    public interface IQueryBuilder<T> : IQueryFetchable<T>, IOrderByDirection<T>
    {
        IJoinable<T, TRelation> InnerJoin<TRelation>();
        IJoinable<T, TRelation> LeftJoin<TRelation>();
        IJoinable<T, TRelation> RightJoin<TRelation>();

        IFilterClause<T, TProperty> Where<TProperty>(Expression<Func<T, TProperty>> property);
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

    public interface IFilterClause<T, VType>
    {
        IBinaryOperation<T> EqualToValue(VType value);
        IBinaryOperation<T> EqualTo(Expression<Func<T, VType>> property);

        IBinaryOperation<T> GreaterThanValue(VType value);
        IBinaryOperation<T> GreaterThan(Expression<Func<T, VType>> property);

        IBinaryOperation<T> GreaterOrEqualToValue(VType value);
        IBinaryOperation<T> GreaterOrEqualTo(Expression<Func<T, VType>> property);

        IBinaryOperation<T> LowerOrEqualToValue(VType value);
        IBinaryOperation<T> LowerOrEqualTo(Expression<Func<T, VType>> property);

        IBinaryOperation<T> LowerThanValue(VType value);
        IBinaryOperation<T> LowerThan(Expression<Func<T, VType>> property);

        IBinaryOperation<T> LikeValue(VType value);
        IBinaryOperation<T> Like(Expression<Func<T, VType>> property);
    }

    public interface IJoinFilterClause<T, TRelation, VType>
    {
        IJoinBinaryOperation<T, TRelation> EqualTo(VType value);
        IJoinBinaryOperation<T, TRelation> EqualTo(Expression<Func<TRelation, VType>> property);

        IJoinBinaryOperation<T, TRelation> GreaterThan(VType value);
        IJoinBinaryOperation<T, TRelation> GreaterThan(Expression<Func<TRelation, VType>> property);

        IJoinBinaryOperation<T, TRelation> GreaterOrEqualTo(VType value);
        IJoinBinaryOperation<T, TRelation> GreaterOrEqualTo(Expression<Func<TRelation, VType>> property);

        IJoinBinaryOperation<T, TRelation> LowerOrEqualTo(VType value);
        IJoinBinaryOperation<T, TRelation> LowerOrEqualTo(Expression<Func<TRelation, VType>> property);

        IJoinBinaryOperation<T, TRelation> LowerThan(VType value);
        IJoinBinaryOperation<T, TRelation> LowerThan(Expression<Func<TRelation, VType>> property);

        IJoinBinaryOperation<T, TRelation> Like(VType value);
        IJoinBinaryOperation<T, TRelation> Like(Expression<Func<TRelation, VType>> property);
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

    public interface IOrderByDirection<T> : IQueryFetchable<T>
    {
        ISorterClause<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IFetchable<T>
    {
        ICollection<T> FetchAll();
        T FetchOne();
    }

    public interface IQueryFetchable<T> : IFetchable<T>
    {
        IQueryFetchable<T> Limit(int i);
        IQueryFetchable<T> Offset(int i);
    }
}

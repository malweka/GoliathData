using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Goliath.Data.Sql
{

    public interface IQueryBuilder<T> : IQueryFetchable<T>
    {
        IJoinQueryBuilder<T> InnerJoinOn<TProperty>(Expression<Func<T, TProperty>> property);
        IJoinQueryBuilder<T> LeftJoinOn<TProperty>(Expression<Func<T, TProperty>> property);
        IJoinQueryBuilder<T> RightJoinOn<TProperty>(Expression<Func<T, TProperty>> property);

        IJoinable<T, TRelation> InnerJoin<TRelation>();
        IJoinable<T, TRelation> LeftJoin<TRelation>();
        IJoinable<T, TRelation> RightJoin<TRelation>();

        IFilterClause<T, TProperty> Where<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IJoinQueryBuilder<T> : IQueryBuilder<T>
    {
        IWhereOnRelation<T, TRelation> ForJoin<TRelation>();
    }

    public interface IWhereOnRelation<T, TRelation>
    {
        IJoinFilterClause<T, TRelation, TProperty> Where<TProperty>(Expression<Func<TRelation, TProperty>> property);
    }

    public interface IJoinable<T, TRelation>
    {
        IJoinOperation<T> On<TProperty>(Expression<Func<TRelation, TProperty>> property);
    }

    public interface IJoinOperation<T>
    {
        IJoinQueryBuilder<T> EqualTo<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IBinaryOperation<T> : IQueryFetchable<T>
    {
        IFilterClause<T, TProperty> And<TProperty>(Expression<Func<T, TProperty>> property);
        IFilterClause<T, TProperty> Or<TProperty>(Expression<Func<T, TProperty>> property);
        ISorterClause<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IFilterClause<T, VType>
    {
        IBinaryOperation<T> EqualTo(VType value);
        IBinaryOperation<T> EqualTo(Expression<Func<T, VType>> propertye);

        IBinaryOperation<T> GreaterThan(VType value);
        IBinaryOperation<T> GreaterThan(Expression<Func<T, VType>> propertye);

        IBinaryOperation<T> GreaterOrEqualTo(VType value);
        IBinaryOperation<T> GreaterOrEqualTo(Expression<Func<T, VType>> propertye);

        IBinaryOperation<T> LowerOrEqualTo(VType obj);
        IBinaryOperation<T> LowerOrEqualTo(Expression<Func<T, VType>> propertye);

        IBinaryOperation<T> LowerThan(VType obj);
        IBinaryOperation<T> LowerThan(Expression<Func<T, VType>> propertye);

        IBinaryOperation<T> Like(VType param);
        IBinaryOperation<T> Like(Expression<Func<T, VType>> propertye);
    }

    public interface IJoinFilterClause<T, TRelation, VType>
    {
        IJoinBinaryOperation<T, TRelation> EqualTo(VType value);
        IJoinBinaryOperation<T, TRelation> EqualTo(Expression<Func<TRelation, VType>> propertye);

        IJoinBinaryOperation<T, TRelation> GreaterThan(VType value);
        IJoinBinaryOperation<T, TRelation> GreaterThan(Expression<Func<TRelation, VType>> propertye);

        IJoinBinaryOperation<T, TRelation> GreaterOrEqualTo(VType value);
        IJoinBinaryOperation<T, TRelation> GreaterOrEqualTo(Expression<Func<TRelation, VType>> propertye);

        IJoinBinaryOperation<T, TRelation> LowerOrEqualTo(VType obj);
        IJoinBinaryOperation<T, TRelation> LowerOrEqualTo(Expression<Func<TRelation, VType>> propertye);

        IJoinBinaryOperation<T, TRelation> LowerThan(VType obj);
        IJoinBinaryOperation<T, TRelation> LowerThan(Expression<Func<TRelation, VType>> propertye);

        IJoinBinaryOperation<T, TRelation> Like(VType param);
        IJoinBinaryOperation<T, TRelation> Like(Expression<Func<TRelation, VType>> propertye);
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
        IFetchable<T> Limit(int i);
        IFetchable<T> Offset(int i);
    }
}

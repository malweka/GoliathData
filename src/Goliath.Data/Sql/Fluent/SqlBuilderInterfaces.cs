using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    public interface ITableNameBuilder
    {
        IQueryBuilder From(string tableName);
        IQueryBuilder From(string tableName, string alias);
    }

    public interface IQueryBuilder : IQueryFetchable
    {
        //IJoinQueryBuilder InnerJoinOn(string propertyName);
        //IJoinQueryBuilder LeftJoinOn(string propertyName);
        //IJoinQueryBuilder RightJoinOn(string propertyName);

        IJoinable InnerJoin(string tableName, string alias);
        IJoinable LeftJoin(string tableName, string alias);
        IJoinable RightJoin(string tableName, string alias);

        IFilterClause Where(string propertyName);
        IFilterClause Where(string tableAlias, string propertyName);
    }

    //public interface IJoinQueryBuilder : IQueryBuilder
    //{
    //    IWhereOnRelation  ForJoin(string alias);
    //}

    //public interface IWhereOnRelation 
    //{
    //    IJoinFilterClause Where(string propertyName);
    //}

    public interface IJoinable
    {
        IJoinOperation On(string propertyName);
    }

    public interface IJoinOperation
    {
        IQueryBuilder EqualTo(string propertyName);
    }

    public interface IBinaryOperation : IQueryFetchable
    {
        IFilterClause And(string propertyName);
        IFilterClause And(string tableAlias, string propertyName);
        IFilterClause Or(string propertyName);
        IFilterClause Or(string tableAlias, string propertyName);
        ISorterClause OrderBy(string propertyName);
        ISorterClause OrderBy(string tableAlias, string propertyName);
    }

    public interface IFilterClause
    {
        IBinaryOperation EqualToValue(object value);
        IBinaryOperation EqualTo(string propertyName);
        IBinaryOperation EqualTo(string tableAlias, string propertyName);

        IBinaryOperation GreaterThanValue(object value);
        IBinaryOperation GreaterThan(string propertyName);
        IBinaryOperation GreaterThan(string tableAlias, string propertyName);

        IBinaryOperation GreaterOrEqualToValue(object value);
        IBinaryOperation GreaterOrEqualTo(string propertyName);
        IBinaryOperation GreaterOrEqualTo(string tableAlias, string propertyName);

        IBinaryOperation LowerOrEqualToValue(object value);
        IBinaryOperation LowerOrEqualTo(string propertyName);
        IBinaryOperation LowerOrEqualTo(string tableAlias, string propertyName);

        IBinaryOperation LowerThanValue(object value);
        IBinaryOperation LowerThan(string propertyName);
        IBinaryOperation LowerThan(string tableAlias, string propertyName);

        IBinaryOperation LikeValue(object value);
        IBinaryOperation Like(string propertyName);
        IBinaryOperation Like(string tableAlias, string propertyName);
    }

    //public interface IJoinFilterClause
    //{
    //    IJoinBinaryOperation EqualTo(object value);
    //    IJoinBinaryOperation EqualTo(string propertyName);

    //    IJoinBinaryOperation GreaterThan(object value);
    //    IJoinBinaryOperation GreaterThan(string propertyName);

    //    IJoinBinaryOperation GreaterOrEqualTo(object value);
    //    IJoinBinaryOperation GreaterOrEqualTo(string propertyName);

    //    IJoinBinaryOperation LowerOrEqualTo(object obj);
    //    IJoinBinaryOperation LowerOrEqualTo(string propertyName);

    //    IJoinBinaryOperation LowerThan(object obj);
    //    IJoinBinaryOperation LowerThan(string propertyName);

    //    IJoinBinaryOperation Like(object param);
    //    IJoinBinaryOperation Like(string propertyName);
    //}

    //public interface IJoinBinaryOperation : IBinaryOperation
    //{
    //    IOnBinaryOperation ForJoin<TTRelation>();
    //}

    //public interface IOnBinaryOperation
    //{
    //    IJoinFilterClause And(string propertyName);
    //    IJoinFilterClause Or(string propertyName);
    //    IJoinFilterClause OrderBy(string propertyName);
    //}

    public interface ISorterClause
    {
        IOrderByDirection Desc();
        IOrderByDirection Asc();
    }

    public interface IOrderByDirection : IQueryFetchable
    {
        ISorterClause OrderBy(string propertyName);
        ISorterClause OrderBy(string tableAlias, string propertyName);
    }

    public interface IFetchable
    {
        ICollection FetchAll();
        object FetchOne();
    }

    public interface IQueryFetchable : IFetchable
    {
        IFetchable Limit(int i);
        IFetchable Offset(int i);
    }

}

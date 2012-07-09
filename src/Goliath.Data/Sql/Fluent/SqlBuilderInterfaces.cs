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
    }

    public interface IJoinQueryBuilder : IQueryBuilder
    {
        IWhereOnRelation  ForJoin(string alias);
    }

    public interface IWhereOnRelation 
    {
        IJoinFilterClause Where(string propertyName);
    }

    public interface IJoinable
    {
        IJoinOperation On(string propertyName);
    }

    public interface IJoinOperation
    {
        IJoinQueryBuilder EqualTo(string propertyName);
    }

    public interface IBinaryOperation : IQueryFetchable
    {
        IFilterClause And(string propertyName);
        IFilterClause Or(string propertyName);
        ISorterClause OrderBy(string propertyName);
    }

    public interface IFilterClause
    {
        IBinaryOperation EqualTo(object value);
        IBinaryOperation EqualTo(string propertyNamee);

        IBinaryOperation GreaterThan(object value);
        IBinaryOperation GreaterThan(string propertyNamee);

        IBinaryOperation GreaterOrEqualTo(object value);
        IBinaryOperation GreaterOrEqualTo(string propertyNamee);

        IBinaryOperation LowerOrEqualTo(object obj);
        IBinaryOperation LowerOrEqualTo(string propertyNamee);

        IBinaryOperation LowerThan(object obj);
        IBinaryOperation LowerThan(string propertyNamee);

        IBinaryOperation Like(object param);
        IBinaryOperation Like(string propertyNamee);
    }

    public interface IJoinFilterClause
    {
        IJoinBinaryOperation EqualTo(object value);
        IJoinBinaryOperation EqualTo(string propertyNamee);

        IJoinBinaryOperation GreaterThan(object value);
        IJoinBinaryOperation GreaterThan(string propertyNamee);

        IJoinBinaryOperation GreaterOrEqualTo(object value);
        IJoinBinaryOperation GreaterOrEqualTo(string propertyNamee);

        IJoinBinaryOperation LowerOrEqualTo(object obj);
        IJoinBinaryOperation LowerOrEqualTo(string propertyNamee);

        IJoinBinaryOperation LowerThan(object obj);
        IJoinBinaryOperation LowerThan(string propertyNamee);

        IJoinBinaryOperation Like(object param);
        IJoinBinaryOperation Like(string propertyNamee);
    }

    public interface IJoinBinaryOperation : IBinaryOperation
    {
        IOnBinaryOperation ForJoin<TTRelation>();
    }

    public interface IOnBinaryOperation
    {
        IJoinFilterClause And(string propertyName);
        IJoinFilterClause Or(string propertyName);
        IJoinFilterClause OrderBy(string propertyName);
    }

    public interface ISorterClause
    {
        IOrderByDirection Desc();
        IOrderByDirection Asc();
    }

    public interface IOrderByDirection : IQueryFetchable
    {
        ISorterClause OrderBy(string propertyName);
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

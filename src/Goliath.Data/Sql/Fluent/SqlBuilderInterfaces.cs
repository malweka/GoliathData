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

    public interface IQueryBuilder : IQueryFetchable, IOrderByDirection
    {
        IJoinable InnerJoin(string tableName, string alias);
        IJoinable LeftJoin(string tableName, string alias);
        IJoinable RightJoin(string tableName, string alias);

        IFilterClause Where(string propertyName);
        IFilterClause Where(string tableAlias, string propertyName);
    }


    public interface IJoinable
    {
        IJoinOperation On(string tableAlias, string propertyName);
    }

    public interface IJoinOperation
    {
        IQueryBuilder EqualTo(string propertyName);
    }

    public interface IBinaryOperation : IQueryFetchable, IOrderByDirection
    {
        IFilterClause And(string propertyName);
        IFilterClause And(string tableAlias, string propertyName);
        IFilterClause Or(string propertyName);
        IFilterClause Or(string tableAlias, string propertyName);
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

        IBinaryOperation ILikeValue(object value);
        IBinaryOperation ILike(string propertyName);
        IBinaryOperation ILike(string tableAlias, string propertyName);

        IBinaryOperation IsNull();
        IBinaryOperation IsNotNull();
    }

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
        ICollection<T> FetchAll<T>();
        T FetchOne<T>();
    }

    public interface IFetchableWithOutput
    {
        ICollection<T> FetchAll<T>(out long total);
        ICollection<T> FetchAll<T>();
    }

    public interface IQueryFetchable : IFetchable
    {
        IFetchableWithOutput Take(int limit, int offset);
    }

}

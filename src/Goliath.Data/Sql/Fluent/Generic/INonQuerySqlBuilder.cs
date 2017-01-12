using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Goliath.Data.Sql
{
    public interface INonQuerySqlBuilder<T>
    {
        IFilterNonQueryClause<T> Where(string propertyName);
        IFilterNonQueryClause<T> Where<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IBinaryNonQueryOperation<T>
    {
        IFilterNonQueryClause<T> And<TProperty>(Expression<Func<T, TProperty>> property);
        IFilterNonQueryClause<T> And(string propertyName);
        IFilterNonQueryClause<T> Or<TProperty>(Expression<Func<T, TProperty>> property);
        IFilterNonQueryClause<T> Or(string propertyName);

        int Execute();
    }

    public interface IFilterNonQueryClause<T>
    {
        IBinaryNonQueryOperation<T> EqualToValue<TValue>(TValue value);
        IBinaryNonQueryOperation<T> EqualTo<TValue>(Expression<Func<T, TValue>> property);

        IBinaryNonQueryOperation<T> GreaterThanValue<TValue>(TValue value);
        IBinaryNonQueryOperation<T> GreaterThan<TValue>(Expression<Func<T, TValue>> property);

        IBinaryNonQueryOperation<T> GreaterOrEqualToValue<TValue>(TValue value);
        IBinaryNonQueryOperation<T> GreaterOrEqualTo<TValue>(Expression<Func<T, TValue>> property);

        IBinaryNonQueryOperation<T> LowerOrEqualToValue<TValue>(TValue value);
        IBinaryNonQueryOperation<T> LowerOrEqualTo<TValue>(Expression<Func<T, TValue>> property);

        IBinaryNonQueryOperation<T> LowerThanValue<TValue>(TValue value);
        IBinaryNonQueryOperation<T> LowerThan<TValue>(Expression<Func<T, TValue>> property);

        IBinaryNonQueryOperation<T> LikeValue<TValue>(TValue value);
        IBinaryNonQueryOperation<T> Like<TValue>(Expression<Func<T, TValue>> property);
    }
}

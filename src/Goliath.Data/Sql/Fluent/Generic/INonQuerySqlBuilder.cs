using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Goliath.Data.Sql
{
    public interface INonQuerySqlBuilder<T>
    {
        IFilterNonQueryClause<T, TProperty> Where<TProperty>(string propertyName);
    }

    public interface IBinaryNonQueryOperation<T>
    {
        IFilterNonQueryClause<T, TProperty> And<TProperty>(Expression<Func<T, TProperty>> property);
        IFilterNonQueryClause<T, TProperty> Or<TProperty>(Expression<Func<T, TProperty>> property);
        
        int Execute();
    }

    public interface IFilterNonQueryClause<T, TValue>
    {
        IBinaryNonQueryOperation<T> EqualToValue(TValue value);
        IBinaryNonQueryOperation<T> EqualTo(Expression<Func<T, TValue>> property);

        IBinaryNonQueryOperation<T> GreaterThanValue(TValue value);
        IBinaryNonQueryOperation<T> GreaterThan(Expression<Func<T, TValue>> property);

        IBinaryNonQueryOperation<T> GreaterOrEqualToValue(TValue value);
        IBinaryNonQueryOperation<T> GreaterOrEqualTo(Expression<Func<T, TValue>> property);

        IBinaryNonQueryOperation<T> LowerOrEqualToValue(TValue value);
        IBinaryNonQueryOperation<T> LowerOrEqualTo(Expression<Func<T, TValue>> property);

        IBinaryNonQueryOperation<T> LowerThanValue(TValue value);
        IBinaryNonQueryOperation<T> LowerThan(Expression<Func<T, TValue>> property);

        IBinaryNonQueryOperation<T> LikeValue(TValue value);
        IBinaryNonQueryOperation<T> Like(Expression<Func<T, TValue>> property);
    }
}

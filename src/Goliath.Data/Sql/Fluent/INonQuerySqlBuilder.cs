using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public interface INonQuerySqlBuilder
    {
        /// <summary>
        /// Wheres the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFilterNonQueryClause Where(string propertyName);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IBinaryNonQueryOperation
    {
        /// <summary>
        /// Ands the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFilterNonQueryClause And(string propertyName);
        /// <summary>
        /// Ors the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IFilterNonQueryClause Or(string propertyName);
        //UpdateSqlBodyInfo Build();
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        int Execute();
    }

    /// <summary>
    /// 
    /// </summary>
    public interface  IFilterNonQueryClause
    {
        /// <summary>
        /// Equals to value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation EqualToValue(object value);
        /// <summary>
        /// Equals to.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation EqualTo(string propertyName);

        /// <summary>
        /// Greaters the than value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation GreaterThanValue(object value);
        /// <summary>
        /// Greaters the than.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation GreaterThan(string propertyName);

        /// <summary>
        /// Greaters the or equal to value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation GreaterOrEqualToValue(object value);
        /// <summary>
        /// Greaters the or equal to.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation GreaterOrEqualTo(string propertyName);

        /// <summary>
        /// Lowers the or equal to value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation LowerOrEqualToValue(object value);
        /// <summary>
        /// Lowers the or equal to.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation LowerOrEqualTo(string propertyName);

        /// <summary>
        /// Lowers the than value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation LowerThanValue(object value);
        /// <summary>
        /// Lowers the than.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation LowerThan(string propertyName);

        /// <summary>
        /// Likes the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation LikeValue(object value);
        /// <summary>
        /// Likes the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IBinaryNonQueryOperation Like(string propertyName);
    }
}

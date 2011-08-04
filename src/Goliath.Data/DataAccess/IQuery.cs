using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQuery<T>
    {
        /// <summary>
        /// Lists this instance.
        /// </summary>
        /// <returns></returns>
        IList<T> List();
        /// <summary>
        /// Lists the specified page index.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns></returns>
        IList<T> List(int pageIndex, int pageSize, out int totalRecords);
        /// <summary>
        /// Wheres the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="andList">The and list.</param>
        /// <returns></returns>
        IQuery<T> Where(Expression<Func<bool>> expression, Expression<Func<bool>>[] andList);
        /// <summary>
        /// Orderbies the specified property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="andProps">The and props.</param>
        /// <returns></returns>
        IQuery<T> Orderby<TProperty>(Expression<Func<TProperty>> property, params Expression<Func<TProperty>>[] andProps);
        /// <summary>
        /// Groups the by.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="andProps">The and props.</param>
        /// <returns></returns>
        IQuery<T> GroupBy<TProperty>(Expression<Func<TProperty>> property, params Expression<Func<TProperty>>[] andProps);
    }
}

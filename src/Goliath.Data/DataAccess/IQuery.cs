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
        /// <returns></returns>
        IQuery<T> Where(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Wheres the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        ISearchable<T> Where(string propertyName);

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

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISearchable<T>
    {
        /// <summary>
        /// Equals to.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        ISearchable<T> EqualTo(object obj);

        /// <summary>
        /// Greaters the than.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        ISearchable<T> GreaterThan(object obj);

        /// <summary>
        /// Greaters the or equal to.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        ISearchable<T> GreaterOrEqualTo(object obj);

        /// <summary>
        /// Lowers the than.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        ISearchable<T> LowerThan(object obj);

        /// <summary>
        /// Lowers the or equal to.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        ISearchable<T> LowerOrEqualTo(object obj);

        /// <summary>
        /// Likes the specified param.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <returns></returns>
        ISearchable<T> Like(string param);

        /// <summary>
        /// Ands the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        ISearchable<T> And(string propertyName);

        /// <summary>
        /// Ors the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        ISearchable<T> Or(string propertyName);

        /// <summary>
        /// Lists this instance.
        /// </summary>
        /// <returns></returns>
        IListable<T> List();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IListable<T> : IList<T>
    {
        /// <summary>
        /// Takes the specified qty.
        /// </summary>
        /// <param name="qty">The qty.</param>
        /// <returns></returns>
        IListable<T> Take(int qty);
        /// <summary>
        /// Skips the specified qty.
        /// </summary>
        /// <param name="qty">The qty.</param>
        /// <returns></returns>
        IListable<T> Skip(int qty);
        /// <summary>
        /// Sorts the by.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <returns></returns>
        IListable<T> SortBy(string propertyName, SortOrder sortOrder);
        //IList<T> ToList();
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Goliath.Data 
{
    using Sql;

    /// <summary>
    /// 
    /// </summary>
    public interface ISqlInterface
    {
        /// <summary>
        /// Selects all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryBuilder<T> SelectAll<T>();

        /// <summary>
        /// Selects all.
        /// </summary>
        /// <returns></returns>
        ITableNameBuilder SelectAll();

        /// <summary>
        /// Selects the specified column.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyNames">The property names.</param>
        /// <returns></returns>
        IQueryBuilder<T> Select<T>(string propertyName, params string[] propertyNames);

        /// <summary>
        /// Selects the specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        ITableNameBuilder Select(string column, params string[] columns);
        
    }
}

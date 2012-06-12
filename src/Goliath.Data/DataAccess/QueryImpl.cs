using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Goliath.Data.DataAccess
{
    class QueryImpl<T> : IQuery<T>
    {
        IDataAccessAdapterFactory dbAccessFactory;

        IDbConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryImpl&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="dbAccessFactory">The db access factory.</param>
        public QueryImpl(IDbConnection connection, IDataAccessAdapterFactory dbAccessFactory)
        {
            this.dbAccessFactory = dbAccessFactory;
            this.connection = connection;
        }

        #region IQuery<T> Members

        /// <summary>
        /// Lists this instance.
        /// </summary>
        /// <returns></returns>
        public IList<T> List()
        {
            throw new NotImplementedException();
        }

        public IList<T> List(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public IQuery<T> Where(Expression<Func<T, bool>> expression)
        {
            return this;
        }

        public ISearchable<T> Where(string propertyName)
        {
            return null;
        }

        public IQuery<T> Orderby<TProperty>(Expression<Func<TProperty>> property, params Expression<Func<TProperty>>[] andProps)
        {
            return this;
        }

        public IQuery<T> GroupBy<TProperty>(Expression<Func<TProperty>> property, params Expression<Func<TProperty>>[] andProps)
        {
            return this;
        }

        #endregion
    }
}

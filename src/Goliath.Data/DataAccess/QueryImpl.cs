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

        public QueryImpl(IDbConnection connection, IDataAccessAdapterFactory dbAccessFactory)
        {
            this.dbAccessFactory = dbAccessFactory;
            this.connection = connection;
        }

        #region IQuery<T> Members

        public IList<T> List()
        {
            throw new NotImplementedException();
        }

        public IList<T> List(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public IQuery<T> Where(Expression<Func<bool>> expression, Expression<Func<bool>>[] andList)
        {
            throw new NotImplementedException();
        }

        public IQuery<T> Orderby<TProperty>(Expression<Func<TProperty>> property, params Expression<Func<TProperty>>[] andProps)
        {
            throw new NotImplementedException();
        }

        public IQuery<T> GroupBy<TProperty>(Expression<Func<TProperty>> property, params Expression<Func<TProperty>>[] andProps)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

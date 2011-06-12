namespace Goliath.Data.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Data;
    using Diagnostics;
    using Sql;

    class QueryImpl<T> : IQuery<T>
    {
        IDataAccessAdaterFactory dbAccessFactory;
        IDbConnection connection;

        public QueryImpl(IDbConnection connection, IDataAccessAdaterFactory dbAccessFactory)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Goliath.Data.Sql
{
    class UpdateSqlBuilder<T> : INonQuerySqlBuilder<T>, IBinaryNonQueryOperation<T>
    {
        #region INonQuerySqlBuilder<T> Members

        public IFilterNonQueryClause<T, TProperty> Where<TProperty>(string propertyName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBinaryNonQueryOperation<T> Members

        public IFilterNonQueryClause<T, TProperty> And<TProperty>(
            Expression<Func<T, TProperty>> property)
        {
            throw new NotImplementedException();
        }

        public IFilterNonQueryClause<T, TProperty> Or<TProperty>(
            Expression<Func<T, TProperty>> property)
        {
            throw new NotImplementedException();
        }

        public int Execute()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class UpdateSqlExecutionList
    {
        readonly List<UpdateSqlBodyInfo> statements = new List<UpdateSqlBodyInfo>();

        public List<UpdateSqlBodyInfo> Statements
        {
            get { return statements; }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    class SortBuilder<T>:ISorterClause<T>
    {
        QueryBuilder<T> builder;
        SortBuilder sortBuilder;

        public SortBuilder(QueryBuilder<T> builder, SortBuilder sortBuilder)
        {
            this.builder = builder;
            this.sortBuilder = sortBuilder;
        }

        #region ISorterClause<T> Members

        public IOrderByDirection<T> Desc()
        {
            sortBuilder.Desc();
            return builder;
        }

        public IOrderByDirection<T> Asc()
        {
            sortBuilder.Asc();
            return builder;
        }

        #endregion
    }
}

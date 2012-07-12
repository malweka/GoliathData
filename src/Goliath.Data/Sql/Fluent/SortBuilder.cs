using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    class SortBuilder : ISorterClause
    {
        QueryBuilder builder;
        public SortType SortDirection { get; private set; }
        public string TableAlias { get; private set; }
        public string ColumnName { get; private set; }

        public SortBuilder(QueryBuilder builder, string columnName):this(builder, string.Empty, columnName){}

        public SortBuilder(QueryBuilder builder, string tableAlias, string columnName)
        {
            this.builder = builder;
            TableAlias = tableAlias;
            ColumnName = columnName;
        }

        #region ISorterClause Members

        public IOrderByDirection Desc()
        {
            SortDirection = SortType.Descinding;
            return builder;
        }

        public IOrderByDirection Asc()
        {
            SortDirection = SortType.Ascending;
            return builder;
        }

        #endregion
    }
}

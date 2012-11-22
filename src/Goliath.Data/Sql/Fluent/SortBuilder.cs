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

        public string BuildSqlString()
        {            
            string col;
            if (!string.IsNullOrWhiteSpace(TableAlias))
                col = string.Format("{0}.{1}", TableAlias, ColumnName);
            else
                col = ColumnName;

            string sql = string.Format("{0} {1}", col, SortTypeToText());
            return sql;
        }

        string SortTypeToText()
        {
            if (SortDirection == SortType.Ascending)
                return "ASC";
            else
                return "DESC";
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

        public override string ToString()
        {
            return BuildSqlString();
        }
    }
}

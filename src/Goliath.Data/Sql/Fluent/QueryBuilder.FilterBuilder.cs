using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    partial class QueryBuilder : IBinaryOperation, IOrderByDirection
    {
        List<WhereClauseBuilder> whereClauses = new List<WhereClauseBuilder>();
        List<SortBuilder> sortClauses = new List<SortBuilder>();

        #region IBinaryOperation Members

        public IFilterClause Where(string columnName)
        {           
            return BuildWhereClause(SqlOperator.AND, columnName);            
        }

        public IFilterClause Where(string tableAlias, string propertyName)
        { 
            return BuildWhereClause(SqlOperator.AND, tableAlias, propertyName); 
        }

        public IFilterClause And(string columnName)
        {
            return BuildWhereClause(SqlOperator.AND, columnName);   
        }

        public IFilterClause And(string tableAlias, string columnName)
        {
            return BuildWhereClause(SqlOperator.AND, tableAlias, columnName);
        }

        public IFilterClause Or(string columnName)
        {
            return BuildWhereClause(SqlOperator.AND, columnName);   
        }

        public IFilterClause Or(string tableAlias, string columnName)
        {
            return BuildWhereClause(SqlOperator.AND, tableAlias, columnName);
        }

        public ISorterClause OrderBy(string columnName)
        {
            return BuildSortClause(string.Empty, columnName);
        }

        public ISorterClause OrderBy(string tableAlias, string columnName)
        {
            return BuildSortClause(tableAlias, columnName);
        }

        #endregion

        WhereClauseBuilder BuildWhereClause(SqlOperator preOperator, string columnName)
        {
            return BuildWhereClause(preOperator, string.Empty, columnName);
        }

        WhereClauseBuilder BuildWhereClause(SqlOperator preOperator, string tableAlias, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("propertyName");

            WhereClauseBuilder whereBuilder = new WhereClauseBuilder(this, columnName);
            whereBuilder.PreOperator = preOperator;
            whereBuilder.TableAlias = tableAlias;
            whereClauses.Add(whereBuilder);
            return whereBuilder;
        }

        SortBuilder BuildSortClause(string tableAlias, string columnName)
        {
            SortBuilder sort = new SortBuilder(this, tableAlias, columnName);
            sortClauses.Add(sort);
            return sort;
        }
    }
}

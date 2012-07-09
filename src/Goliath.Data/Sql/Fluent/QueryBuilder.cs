using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    using Providers;
    using Mapping;

    partial class QueryBuilder : IQueryBuilder, ITableNameBuilder
    {
        List<string> columnNames = new List<string>();
        Dictionary<string, IJoinable> joins = new Dictionary<string, IJoinable>();
        EntityMap Table { get; set; }
        MapConfig mapping;

        SqlDialect dialect;
        string tableName;
        string alias;
        int limit;
        int offset;

        public QueryBuilder(SqlDialect dialect, List<string> columnNames, MapConfig mapping)
        {
            if (columnNames != null)
                this.columnNames = columnNames;

            if (dialect == null)
                throw new ArgumentNullException("dialect");

            if (mapping == null)
                throw new ArgumentNullException("mapping");

            this.dialect = dialect;
            this.mapping = mapping;

        }

        #region ITableNameBuilder Members

        public IQueryBuilder From(string tableName)
        {
            return From(tableName, null);
        }

        public IQueryBuilder From(string tableName, string alias)
        {
            this.tableName = tableName;
            this.alias = alias;
            return this;
        }

        #endregion

        #region IQueryBuilder Members

        //public IJoinQueryBuilder InnerJoinOn(string propertyName)
        //{
        //    if (Table == null)
        //        throw new  GoliathDataException("not join table provided and we do not know what is the mapped table we cannot retrieve all references.");

        //    throw new NotImplementedException();
        //}

        //public IJoinQueryBuilder LeftJoinOn(string propertyName)
        //{
        //    throw new NotImplementedException();
        //}

        //public IJoinQueryBuilder RightJoinOn(string propertyName)
        //{
        //    throw new NotImplementedException();
        //}


        public IFilterClause Where(string propertyName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IQueryFetchable Members

        public IFetchable Limit(int i)
        {
            this.limit = i;
            return this;
        }

        public IFetchable Offset(int i)
        {
            this.offset = i;
            return this;
        }

        #endregion

        #region IFetchable Members

        public ICollection FetchAll()
        {
            throw new NotImplementedException();
        }

        public object FetchOne()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

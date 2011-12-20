using System;
using System.Collections.Generic;

namespace Goliath.Data.Sql
{
    using DataAccess;
    using Mapping;
    using Providers;

    abstract class SqlBuilder
    {
        protected SqlMapper sqlMapper;
        protected EntityMap entMap;

        public SqlMapper SqlMapper
        {
            get { return sqlMapper; }
        }

        readonly Dictionary<string, string> columns = new Dictionary<string, string>();
        internal Dictionary<string, string> Columns
        {
            get { return columns; }
        }

        protected SqlBuilder(SqlMapper sqlMapper, EntityMap entMap)
        {
            if (sqlMapper == null)
                throw new ArgumentNullException("sqlMapper");

            if (entMap == null)
                throw new ArgumentNullException("entMap");

            this.sqlMapper = sqlMapper;
            this.entMap = entMap;
        }


        public static string BuildParameterNameWithLevel(string columnName, string tableAlias, int level)
        {
            return string.Format("{0}_{1}", ParameterNameBuilderHelper.ColumnQueryName(columnName, tableAlias), level);
        }

        public abstract string ToSqlString();

        public override string ToString()
        {
#if DEBUG
            return base.ToString();
#else 
            return ToSqlString();
#endif
        }
    }
}

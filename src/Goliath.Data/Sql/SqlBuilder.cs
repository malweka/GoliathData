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
        protected List<WhereStatement> wheres = new List<WhereStatement>();

        public SqlMapper SqlMapper
        {
            get { return sqlMapper; }
        }

        readonly Dictionary<string, string> columns = new Dictionary<string, string>();
        internal Dictionary<string, string> Columns
        {
            get { return columns; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlBuilder"/> class.
        /// </summary>
        /// <param name="sqlMapper">The SQL mapper.</param>
        /// <param name="entMap">The ent map.</param>
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

        public static WhereStatement[] BuildWhereStatementFromPrimaryKey(EntityMap entMap, SqlMapper sqlMapper, int level)
        {
            if (entMap == null)
                throw new ArgumentNullException("entMap");

            List<WhereStatement> wheres = new List<WhereStatement>();
            if (entMap.PrimaryKey != null)
            {
                for (int i = 0; i < entMap.PrimaryKey.Keys.Count; i++)
                {
                    string colname = entMap.PrimaryKey.Keys[i].Key.ColumnName;
                    var paramName = BuildParameterNameWithLevel(colname, entMap.TableAlias, level);

                    var whs = new WhereStatement(ParameterNameBuilderHelper.ColumnWithTableAlias(entMap.TableName, colname))
                                    .Equals(sqlMapper.CreateParameterName(paramName));


                    wheres.Add(whs);
                }
            }

            return wheres.ToArray();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Providers;
using Goliath.Data.Mapping;

namespace Goliath.Data.Sql
{
    class SelectSqlBuilder
    {
        SqlMapper sqlMapper;
        EntityMap entMap;
        PagingInfo? paging;

        readonly Dictionary<string, string> columns = new Dictionary<string, string>();
        internal Dictionary<string, string> Columns
        {
            get { return columns; }
        }

        readonly List<SqlJoin> joins = new List<SqlJoin>();
        internal List<SqlJoin> Joins
        {
            get { return joins; }
        }

        WhereStatement where;
        List<OrderBy> sortList = new List<OrderBy>();

        //TODO: mechanism to cache select builder
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectSqlBuilder"/> class.
        /// </summary>
        /// <param name="sqlMapper">The SQL mapper.</param>
        /// <param name="entMap">The ent map.</param>
        public SelectSqlBuilder(SqlMapper sqlMapper, EntityMap entMap)
        {
            if (sqlMapper == null)
                throw new ArgumentNullException("sqlMapper");

            if (entMap == null)
                throw new ArgumentNullException("entMap");

            this.sqlMapper = sqlMapper;
            this.entMap = entMap;

            foreach (var col in entMap)
            {
                if (col is Relation)
                {
                    Relation rel = (Relation)col;
                    if (rel.RelationType != RelationshipType.ManyToOne)
                        continue;
                    if (!rel.LazyLoad)
                    {
                        var rightTable = entMap.Parent.EntityConfigs[rel.ReferenceEntityName];
                        //var rightColumn = rightTable[rel.ReferenceColumn];
                        AddJoin(new SqlJoin(entMap, JoinType.Inner).OnTable(rightTable)
                            .OnRightColumn(rel.ReferenceColumn)
                            .OnLeftColumn(rel));
                    }
                }

                string colKey = string.Format("{0}.{1}", entMap.TableAbbreviation, col.ColumnName);
                //var tuple = Tuple.Create<string, string>(sqlMapper.CreateParameterName(col.Name), CreateColumnName(entMap, col));
                if (!columns.ContainsKey(colKey))
                    this.columns.Add(colKey, BuildColumnSelectString(col.ColumnName, entMap.TableAbbreviation));
            }

        }

        internal string BuildColumnSelectString(string columnName, string tableAbbreviation)
        {
            return string.Format("{2}.{0} AS {1}", columnName, Property.PropertyQueryName(columnName, tableAbbreviation), tableAbbreviation);
        }

        internal string BuildTableFromString(string tableName, string tableAbbreviation)
        {
            return string.Format("{0} {1}", tableName, tableAbbreviation);
        }

        public SelectSqlBuilder Where(WhereStatement where)
        {
            this.where = where;
            return this;
        }

        public SelectSqlBuilder OrderBy(OrderBy orderby, params OrderBy[] sorts)
        {
            sortList.Add(orderby);
            if ((sorts != null) && (sorts.Length > 0))
            {
                for (int i = 0; i < sorts.Length; i++)
                    sortList.Add(sorts[i]);
            }
            return this;
        }

        /// <summary>
        /// Adds the join.
        /// </summary>
        /// <param name="joinType">Type of the join.</param>
        /// <returns></returns>
        public SelectSqlBuilder AddJoin(SqlJoin join)
        {
            //SqlJoin join = new SqlJoin(joinType);
            joins.Add(join);
            return this;
        }

        public SelectSqlBuilder WithPaging(int limit, int offset)
        {
            paging = new PagingInfo() { Limit = limit, Offset = offset };
            return this;
        }

        public string Build()
        {
            List<string> printColumns = new List<string>();
            List<SqlJoin> sJoins = new List<SqlJoin>();
            sJoins.AddRange(Joins);
            printColumns.AddRange(Columns.Values);

            SqlQueryBody queryBody = new SqlQueryBody();

            if (Joins.Count > 0)
            {
                for (int i = 0; i < sJoins.Count; i++)
                {
                    BuildColumsAndJoins(sJoins[i].OnEntityMap, printColumns, sJoins);
                }
            }

            queryBody.ColumnEnumeration = string.Join(", ", printColumns);
            queryBody.From = BuildTableFromString(entMap.TableName, entMap.TableAbbreviation);

            if (sJoins.Count > 0)
            {
                queryBody.JoinEnumeration = string.Join(", ", sJoins);
            }

            if (where != null)
            {
                queryBody.WhereExpression = where.ToString();
            }

            if (paging != null)
            {
                return sqlMapper.QueryWithPaging(queryBody, paging.Value);
            }

            return queryBody.ToString();
        }

        void BuildColumsAndJoins(EntityMap entMap, List<string> cols, List<SqlJoin> sjoins)
        {
            SelectSqlBuilder selBuilder = new SelectSqlBuilder(sqlMapper, entMap);
            cols.AddRange(selBuilder.Columns.Values);
            for (int i = 0; i < selBuilder.Joins.Count; i++)
            {
                var jn = selBuilder.Joins[i];
                BuildColumsAndJoins(jn.OnEntityMap, cols, sjoins);
                sjoins.Add(jn);
            }
        }

        public override string ToString()
        {
#if DEBUG
            return base.ToString();
#else 
            return Build();
#endif
        }

        internal static string CreateColumnName(EntityMap entity, Property column)
        {
            return string.Format("{1}.{0} AS {2}", column.ColumnName, entity.TableAbbreviation, column.GetQueryName(entity));
        }

        internal static string CreateTableName(string tableAbbreviation, string tableName)
        {
            return string.Format("{0} {1}", tableName, tableAbbreviation);
        }
    }

    public struct SqlQueryBody
    {
        public string ColumnEnumeration { get; set; }
        public string From { get; set; }
        public string JoinEnumeration { get; set; }
        public string WhereExpression { get; set; }
        public string SortExpression { get; set; }
        public string AggregateExpression { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("SELECT ");
            sb.Append(ColumnEnumeration);
            sb.AppendFormat("\nFROM {0}", From);

            if (!string.IsNullOrWhiteSpace(JoinEnumeration))
                sb.Append(JoinEnumeration);
            if (!string.IsNullOrWhiteSpace(WhereExpression))
                sb.AppendFormat("\nWHERE {0}\n", WhereExpression);

            return sb.ToString();
        }
    }
}

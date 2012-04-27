using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    using Providers;
    using Mapping;

    class SelectSqlBuilder : SqlBuilder
    {
        PagingInfo? paging;
        readonly List<SqlJoin> joins = new List<SqlJoin>();

        internal List<SqlJoin> Joins
        {
            get { return joins; }
        }


        List<OrderBy> sortList = new List<OrderBy>();

        //TODO: mechanism to cache select builder
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectSqlBuilder"/> class.
        /// </summary>
        /// <param name="sqlMapper">The SQL mapper.</param>
        /// <param name="entMap">The ent map.</param>
        public SelectSqlBuilder(SqlMapper sqlMapper, EntityMap entMap)
            : base(sqlMapper, entMap)
        {
            if (entMap is UnMappedTableMap)
            {
                //we should do nothing
            }
            else
            {
                foreach (var col in entMap)
                {
                    if (col is Relation)
                    {
                        Relation rel = (Relation)col;
                        if (rel.RelationType != RelationshipType.ManyToOne)
                            continue;
                        if (!rel.LazyLoad)
                        {
                            var rightTable = entMap.Parent.GetEntityMap(rel.ReferenceEntityName);
                            //var rightColumn = rightTable[rel.ReferenceColumn];
                            AddJoin(new SqlJoin(entMap, JoinType.Inner).OnTable(rightTable)
                                .OnRightColumn(rel.ReferenceColumn)
                                .OnLeftColumn(rel));
                        }
                    }

                    string colKey = ParameterNameBuilderHelper.ColumnWithTableAlias(entMap.TableAlias, col.ColumnName);  //string.Format("{0}.{1}", entMap.TableAlias, col.ColumnName);
                    //var tuple = Tuple.Create<string, string>(sqlMapper.CreateParameterName(col.Name), CreateColumnName(entMap, col));
                    if (!Columns.ContainsKey(colKey))
                        Columns.Add(colKey, BuildColumnSelectString(col.ColumnName, entMap.TableAlias));
                }
            }

        }

        internal string BuildColumnSelectString(string columnName, string tableAbbreviation)
        {
            return string.Format("{2}.{0} AS {1}", columnName, ParameterNameBuilderHelper.ColumnQueryName(columnName, tableAbbreviation), tableAbbreviation);
        }

        internal string BuildTableFromString(string tableName, string tableAbbreviation)
        {
            return string.Format("{0} {1}", tableName, tableAbbreviation);
        }

        public SelectSqlBuilder Where(WhereStatement where)
        {
            if (where != null)
                wheres.Add(where);

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

        public SqlQueryBody Build()
        {
            Dictionary<string, string> printColumns = new Dictionary<string, string>();
            List<SqlJoin> sJoins = new List<SqlJoin>();
            sJoins.AddRange(Joins);

            foreach (var keyPair in Columns)
                printColumns.Add(keyPair.Key, keyPair.Value);

            SqlQueryBody queryBody = new SqlQueryBody();

            if (Joins.Count > 0)
            {
                for (int i = 0; i < sJoins.Count; i++)
                {
                    BuildColumsAndJoins(sJoins[i].OnEntityMap, printColumns, sJoins);
                }
            }

            queryBody.ColumnEnumeration = string.Join(", ", printColumns.Values);
            queryBody.From = BuildTableFromString(entMap.TableName, entMap.TableAlias);

            if (sJoins.Count > 0)
            {
                queryBody.JoinEnumeration = string.Join(" ", sJoins);
            }

            int wheresCount = wheres.Count;
            if (wheresCount > 0)
            {
                StringBuilder whereBuilder = new StringBuilder();
                for (int i = 0; i < wheresCount; i++)
                {
                    whereBuilder.Append(wheres[i].ToString());
                    if (i != (wheresCount - 1))
                        whereBuilder.AppendFormat(" {0} ", wheres[i].PostOperator);
                }
                queryBody.WhereExpression = whereBuilder.ToString();
            }

            return queryBody;
        }

        public override string ToSqlString()
        {
            var queryBody = Build();
            if (paging != null)
            {
                return queryBody.ToString(sqlMapper, paging.Value);
            }

            return queryBody.ToString();
        }

        void BuildColumsAndJoins(EntityMap entMap, Dictionary<string, string> cols, List<SqlJoin> sjoins)
        {
           
            if (entMap is UnMappedTableMap)
                return;

            SelectSqlBuilder selBuilder = new SelectSqlBuilder(sqlMapper, entMap);
            foreach (var kp in selBuilder.Columns)
            {
                if (!cols.ContainsKey(kp.Key))
                    cols.Add(kp.Key, kp.Value);
            }
            
            for (int i = 0; i < selBuilder.Joins.Count; i++)
            {
                var jn = selBuilder.Joins[i];
                BuildColumsAndJoins(jn.OnEntityMap, cols, sjoins);
                sjoins.Add(jn);
            }
        }


    }

}

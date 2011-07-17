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
        OrderBy orderBy;

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
                        AddJoin(new SqlJoin(JoinType.Inner).OnTable(rightTable)
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

        public SelectSqlBuilder OrderBy(OrderBy orderby)
        {
            this.orderBy = orderby;
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

        public override string ToString()
        {
            //TODO use recursive function to get column list
            StringBuilder sb = new StringBuilder("SELECT ");
            List<string> printColumns = new List<string>();
            printColumns.AddRange(Columns.Values);


            if (Joins.Count > 0)
            {
                foreach (var sqlJoin in Joins)
                {
                    SelectSqlBuilder selBuilder = new SelectSqlBuilder(this.sqlMapper, sqlJoin.OnEntityMap);
                    printColumns.AddRange(selBuilder.Columns.Values);
                }
            }

            sb.Append(string.Join(", ", printColumns));
            sb.AppendFormat(" FROM {0}", BuildTableFromString(entMap.TableName, entMap.TableAbbreviation));

            if (where != null)
            {
                sb.Append(" WHERE ");
                sb.Append(where.ToString());
            }

            return sb.ToString();
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
}

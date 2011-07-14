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
        Dictionary<string, Tuple<string, string>> columns = new Dictionary<string, Tuple<string, string>>();
        List<SqlJoin> joins = new List<SqlJoin>();
        WhereStatement where;

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

                var tuple = Tuple.Create<string, string>(sqlMapper.CreateParameterName(col.Name), CreateColumnName(entMap, col));
                this.columns.Add(col.ColumnName, tuple);
            }
           
        }

        //public 

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

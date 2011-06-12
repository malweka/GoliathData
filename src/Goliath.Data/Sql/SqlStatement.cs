using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Providers;
using Goliath.Data.Mapping;

namespace Goliath.Data.Sql
{
    public class SqlStatement
    {
    }

    /// <summary>
    /// 
    /// </summary>
    class SelectSqlStatementBuilder
    {
        SqlMapper sqlMapper;
        EntityMap entMap;
        Dictionary<string, Tuple<string, string>> columns = new Dictionary<string, Tuple<string, string>>();
        List<SqlJoin> joins = new List<SqlJoin>();

        //TODO: mechanism to cache select builder
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectSqlStatementBuilder"/> class.
        /// </summary>
        /// <param name="sqlMapper">The SQL mapper.</param>
        /// <param name="entMap">The ent map.</param>
        public SelectSqlStatementBuilder(SqlMapper sqlMapper, EntityMap entMap)
        {
            if (sqlMapper == null)
                throw new ArgumentNullException("sqlMapper");
            if (entMap == null)
                throw new ArgumentNullException("entMap");

            this.sqlMapper = sqlMapper;
            this.entMap = entMap;

            foreach (var col in entMap.Properties)
            {
                var tuple = Tuple.Create<string, string>(sqlMapper.CreateParameterName(col.Name), CreateColumnName(entMap.TableAbbreviation, col.ColumnName));
                this.columns.Add(col.ColumnName, tuple);
            }

            foreach (var col in entMap.Relations)
            {
                if (col.RelationType == RelationshipType.OneToMany)
                {
                    var tuple = Tuple.Create<string, string>(sqlMapper.CreateParameterName(col.Name), CreateColumnName(entMap.TableAbbreviation, col.ColumnName));
                    this.columns.Add(col.ColumnName, tuple);
                }
            }
        }

        public SelectSqlStatementBuilder AddJoin(JoinType joinType, EntityMap onEntity, string leftPropName, string rightPropName)
        {
            string onTable = CreateTableName(onEntity.TableAbbreviation, onEntity.TableName);
            var propLeft = entMap[leftPropName];
            var propRight = onEntity[rightPropName];

            if (propLeft == null)
                throw new DataAccessException("{0} Add Join Failed. {1} property was not found in {2}", entMap.Name, leftPropName, entMap.Name);
            if(propRight == null)
                throw new DataAccessException("{0} Add Join Failed. {1} property was not found in {2}", onEntity.Name, rightPropName, onEntity.Name);

            return AddJoin(joinType, onTable, CreateColumnName(entMap.TableAbbreviation, propLeft.ColumnName),
                 CreateColumnName(onEntity.TableAbbreviation, propRight.ColumnName));
        }

        public SelectSqlStatementBuilder AddJoin(JoinType joinType, string onTable, string leftCol, string rightCol)
        {
            SqlJoin join = new SqlJoin(joinType, onTable) { LeftColumn = leftCol, RightColumn = rightCol };
            return this;
        }

        internal static string CreateColumnName(string tableAbbreviation, string columnName)
        {
            return string.Format("{1}.{0} AS {0}_{1}", columnName, tableAbbreviation);
        }

        internal static string CreateTableName(string tableAbbreviation, string tableName)
        {
            return string.Format("{0} {1}", tableName, tableAbbreviation);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Mapping;
using Goliath.Data.Sql;

namespace Goliath.Data
{
    class TableQueryMap
    {
        private int iteration;
        private int recursion;

        public TableQueryMap(string tableName, int recursion = 0)
        {
            Columns = new Dictionary<string, string>();
            ReferenceColumns = new Dictionary<string, JoinColumnQueryMap>();
            Table = tableName;
            Prefix = CreatePrefix(iteration, recursion);
            this.recursion = recursion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableQueryMap" /> class.
        /// </summary>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="recursion">The recursion.</param>
        public TableQueryMap(EntityMap entityMap, int recursion = 0) : this(entityMap.TableName, recursion) { }

        public string Table { get; private set; }

        public string Prefix { get; private set; }

        public IDictionary<string, string> Columns { get; private set; }

        public IDictionary<string, JoinColumnQueryMap> ReferenceColumns { get; private set; }

        public void LoadColumns(EntityMap entityMap, ISession session, IQueryBuilder queryBuilder, IList<string> columnSelectList)
        {
            if (entityMap == null)
                throw new ArgumentNullException("entityMap");

            if (session == null)
                throw new ArgumentNullException("session");

            if (columnSelectList == null)
                throw new ArgumentNullException("columnSelectList");

            if (entityMap.Properties != null)
            {
                for (var i = 0; i < entityMap.Properties.Count; i++)
                {
                    var prop = entityMap.Properties[i];
                    var columnKey = PrintColumnKey(Prefix, prop.ColumnName);

                    columnSelectList.Add(columnKey);
                    Columns.Add(columnKey, prop.PropertyName);
                }
            }

            if (entityMap.Relations == null) return;

            var map = session.SessionFactory.DbSettings.Map;
            int counter = 0;
            for (var i = 0; i < entityMap.Relations.Count; i++)
            {
                var rel = entityMap.Relations[i];

                if (rel.LazyLoad) continue;

                var relTable = map.GetEntityMap(rel.ReferenceEntityName);

                var jcolumn = new JoinColumnQueryMap(rel.PropertyName, i + 1, recursion + 1);
                queryBuilder.LeftJoin(relTable.TableName, jcolumn.Prefix)
                    .On(Prefix, rel.ReferenceColumn)
                    .EqualTo(rel.ColumnName);

                jcolumn.LoadColumns(relTable, session, queryBuilder, columnSelectList);
                ReferenceColumns.Add(jcolumn.ColumnName, jcolumn);

                counter++;
            }

            if (entityMap.IsSubClass)
            {
                var extends = session.SessionFactory.DbSettings.Map.GetEntityMap(entityMap.Extends);
                var pk = extends.PrimaryKey.Keys.First();

                var jcolumn = new JoinColumnQueryMap(pk.Key.PropertyName, counter + 1, recursion + 1);
                queryBuilder.LeftJoin(extends.TableName, jcolumn.Prefix)
                    .On(Prefix, pk.Key.ColumnName)
                    .EqualTo(pk.Key.ColumnName);
                jcolumn.LoadColumns(extends, session, queryBuilder, columnSelectList);
                ReferenceColumns.Add(jcolumn.ColumnName, jcolumn);
            }

        }

        public static string PrintColumnKey(string tablePrefix, string columnName)
        {
            return string.Format("{0}.{1}", tablePrefix, columnName);
        }

        /// <summary>
        /// Creates the prefix.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="recursion">The recursion.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// iteration
        /// or
        /// recursion
        /// </exception>
        public static string CreatePrefix(int iteration = 0, int recursion = 0)
        {
            if (iteration < 0)
                iteration = 0;

            const string alphas = "abcdefghijklmnopqrstuvwxyz";

            int index = (iteration / alphas.Length) + 1;


            if (recursion < 0)
                throw new ArgumentOutOfRangeException("recursion");

            StringBuilder sb = new StringBuilder();

            sb.Append(alphas[index]);
            sb.Append(iteration);
            return sb.ToString();
        }
    }
}
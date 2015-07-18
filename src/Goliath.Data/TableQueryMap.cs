using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Mapping;
using Goliath.Data.Sql;

namespace Goliath.Data
{
    public class TableQueryMap
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

        public string Prefix { get; internal set; }

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

            if (entityMap.PrimaryKey != null)
            {
                for (var i = 0; i < entityMap.PrimaryKey.Keys.Count; i++)
                {
                    Property prop = entityMap.PrimaryKey.Keys[i].Key;
                    var columnKey = PrintColumnKey(Prefix, prop.ColumnName);

                    columnSelectList.Add(columnKey);
                    Columns.Add(prop.PropertyName, columnKey);
                }
            }

            if (entityMap.Properties != null)
            {
                for (var i = 0; i < entityMap.Properties.Count; i++)
                {
                    var prop = entityMap.Properties[i];
                    var columnKey = PrintColumnKey(Prefix, prop.ColumnName);

                    columnSelectList.Add(columnKey);
                    Columns.Add(prop.PropertyName, columnKey);
                }
            }

            int counter = 0;

            if (entityMap.IsSubClass)
            {
                counter++;

                var extends = session.SessionFactory.DbSettings.Map.GetEntityMap(entityMap.Extends);
                var pk = extends.PrimaryKey.Keys.First();

                var jcolumn = new JoinColumnQueryMap(pk.Key.PropertyName, counter, recursion + 1);
                queryBuilder.LeftJoin(extends.TableName, jcolumn.Prefix)
                    .On(Prefix, pk.Key.ColumnName)
                    .EqualTo(pk.Key.ColumnName);
                jcolumn.LoadColumns(extends, session, queryBuilder, columnSelectList);
                ReferenceColumns.Add(jcolumn.ColumnName, jcolumn);
            }

            if (entityMap.Relations == null) return;

            var map = session.SessionFactory.DbSettings.Map;
            
            for (var i = 0; i < entityMap.Relations.Count; i++)
            {
                var rel = entityMap.Relations[i];

                if(rel.RelationType != RelationshipType.ManyToOne)
                    continue;

                var relTable = map.GetEntityMap(rel.ReferenceEntityName);

                counter++;
                var jcolumn = new JoinColumnQueryMap(rel.PropertyName, counter, recursion + 1);
                queryBuilder.LeftJoin(relTable.TableName, jcolumn.Prefix)
                    .On(Prefix, rel.ReferenceColumn)
                    .EqualTo(rel.ColumnName);

                jcolumn.LoadColumns(relTable, session, queryBuilder, columnSelectList, !rel.LazyLoad);
                ReferenceColumns.Add(jcolumn.ColumnName, jcolumn);

                
            }

            

        }

        public static string PrintColumnKey(string tablePrefix, string columnName)
        {
            return string.Format("{0}.{1}", tablePrefix, columnName);
        }

        const string Alphas = "abcdefghijklmnopqrstuvwxyz";

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



            int index = (iteration / Alphas.Length) + 1;


            if (recursion < 0)
                throw new ArgumentOutOfRangeException("recursion");

            StringBuilder sb = new StringBuilder();

            sb.Append(Alphas[index]);
            sb.Append(iteration);
            return sb.ToString();
        }

        //static ConcurrentDictionary<string, string> tablePrefixes = new ConcurrentDictionary<string, string>();

        //public static string CreateUniquePrefix(string tableName)
        //{
        //    string prefix;
        //    if(tablePrefixes.TryGetValue(tableName, out prefix))
        //        return prefix;

        //}
    }
}
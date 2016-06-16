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
        private RecursionCounter recInfo;

        private Dictionary<string, string> usedPrefixes;

        public class ColumnInfo
        {
            public string PropertyName;
            public int Index;
        }

        //public TableQueryMap(string tableName )
        //{
        //    Columns = new Dictionary<string, ColumnInfo>();
        //    ReferenceColumns = new Dictionary<string, JoinColumnQueryMap>();
        //    Table = tableName;
        //    this.iteration = 0;
        //    this.recursion = 1;
        //    Prefix = CreatePrefix(iteration, recursion);
        //}

        public TableQueryMap(string tableName, ref int recursion, ref int iteration)
            : this(new Dictionary<string, string>(), tableName, new RecursionCounter{Recursion = recursion, Iteration = iteration})
        {
        }

        internal TableQueryMap(Dictionary<string, string> usedPrefixes, string tableName, RecursionCounter recInfo)
        {
            Columns = new Dictionary<string, ColumnInfo>();
            ReferenceColumns = new Dictionary<string, JoinColumnQueryMap>();
            Table = tableName;

            if(recInfo==null)
                recInfo = new RecursionCounter();

            this.recInfo = recInfo;

            if (usedPrefixes == null)
                usedPrefixes = new Dictionary<string, string>();

            this.usedPrefixes = usedPrefixes;

            Prefix = GetNextPrefix(usedPrefixes, tableName, recInfo);
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="TableQueryMap" /> class.
        ///// </summary>
        ///// <param name="entityMap">The entity map.</param>
        ///// <param name="recursion">The recursion.</param>
        ///// <param name="iteration">The iteration.</param>
        //public TableQueryMap(EntityMap entityMap, int recursion = 0, int iteration=0) : this(entityMap.FullName,ref recursion,ref iteration) { }

        public string Table { get; private set; }

        public string Prefix { get; internal set; }

        public IDictionary<string, ColumnInfo> Columns { get; private set; }

        public IDictionary<string, JoinColumnQueryMap> ReferenceColumns { get; private set; }


        public void LoadColumns(EntityMap entityMap, ISession session, IQueryBuilder queryBuilder, IList<string> columnSelectList, bool lazyLoadManytoOne=false)
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
                    Columns.Add(columnKey, new ColumnInfo { PropertyName = prop.PropertyName, Index = -1 });
                }
            }

            if (entityMap.Properties != null)
            {
                for (var i = 0; i < entityMap.Properties.Count; i++)
                {
                    var prop = entityMap.Properties[i];
                    var columnKey = PrintColumnKey(Prefix, prop.ColumnName);

                    columnSelectList.Add(columnKey);
                    Columns.Add(columnKey, new ColumnInfo { PropertyName = prop.PropertyName, Index = -1 });
                }
            }


            if (entityMap.IsSubClass)
            {
                recInfo.Recursion = recInfo.Iteration + 1;

                var extends = session.SessionFactory.DbSettings.Map.GetEntityMap(entityMap.Extends);
                var pk = extends.PrimaryKey.Keys.First();
                var jcolumn = new JoinColumnQueryMap(usedPrefixes, extends.FullName, pk.Key.PropertyName, recInfo);

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

                if (rel.RelationType != RelationshipType.ManyToOne || rel.LazyLoad || lazyLoadManytoOne)
                    continue;

                 

                var relTable = map.GetEntityMap(rel.ReferenceEntityName);

                recInfo.Recursion = recInfo.Iteration + 1;


                var jcolumn = new JoinColumnQueryMap(usedPrefixes, relTable.FullName, rel.PropertyName, recInfo);

                //var qb = queryBuilder as QueryBuilder;

                //if (qb != null && qb.Joins.ContainsKey(jcolumn.Prefix))
                //{
                //    counter = counter + 26;
                //    jcolumn.Prefix = CreatePrefix(counter, recursion + 1);
                //    jcolumn.JoinTable.Prefix = jcolumn.Prefix;
                //}

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

        internal const string Alphas = "abcdefghijklmnopqrstuvwxyz";

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

            if (recursion > 0)
            {
                sb.Append(Alphas[index]);
            }

            sb.Append(iteration);
            sb.AppendFormat("_{0}", recursion);
            return sb.ToString();
        }

        static string GetNextPrefix(Dictionary<string, string> usedPrefixes, string tableName, RecursionCounter recInfo)
        {
            
            if (recInfo.Iteration>= 650)
            {
                recInfo.Iteration = 0;
                recInfo.Recursion = recInfo.Recursion + 1;
            }
            var pref = CreatePrefix(recInfo.Iteration, recInfo.Recursion);

            while (usedPrefixes.ContainsKey(pref))
            {
                recInfo.Iteration = recInfo.Iteration+1;
                if (recInfo.Iteration >= 650)
                {
                    recInfo.Iteration = 0;
                    recInfo.Recursion = recInfo.Recursion + 1;
                }

                pref = CreatePrefix(recInfo.Iteration, recInfo.Recursion);
            }

            usedPrefixes.Add(pref, tableName);
            return pref;
        }

        
        //static ConcurrentDictionary<string, string> tablePrefixes = new ConcurrentDictionary<string, string>();

        //public static string CreateUniquePrefix(string tableName)
        //{
        //    string prefix;
        //    if(tablePrefixes.TryGetValue(tableName, out prefix))
        //        return prefix;

        //}

     public   class RecursionCounter
        {
            public int Iteration;
            public int Recursion;
        }
    }

   
}
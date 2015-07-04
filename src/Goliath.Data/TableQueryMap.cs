using System;
using System.Collections.Generic;
using Goliath.Data.Mapping;

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
        public TableQueryMap(IEntityMap entityMap, int recursion = 0) : this(entityMap.TableName, recursion) { }

        public string Table { get; private set; }

        public string Prefix { get; private set; }

        public IDictionary<string, string> Columns { get; private set; }

        public IDictionary<string, JoinColumnQueryMap> ReferenceColumns { get; private set; }

        public void LoadColumns(IEntityMap entityMap, ISession session)
        {
            if (entityMap == null)
                throw new ArgumentNullException("entityMap");

            if (session == null)
                throw new ArgumentNullException("session");

            if (entityMap.Properties != null)
            {
                for (var i = 0; i < entityMap.Properties.Count; i++)
                {
                    var prop = entityMap.Properties[i];
                    var columnKey = PrintColumnKey(Prefix, prop.ColumnName);
                    Columns.Add(columnKey, prop.PropertyName);
                }
            }

            if (entityMap.Relations == null) return;

            var map = session.SessionFactory.DbSettings.Map;
            for (var i = 0; i < entityMap.Relations.Count; i++)
            {
                var rel = entityMap.Relations[i];

                var relTAble = map.GetEntityMap(rel.ReferenceEntityName);

                var jcolumn = new JoinColumnQueryMap(rel.PropertyName, i + 1, recursion + 1);
                jcolumn.LoadColumns(relTAble, session);
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
            const string alphas = "abcdefghijklmnopqrstuvwxyz";

            int index = (iteration / alphas.Length);

            if (iteration < 0 || index >= alphas.Length)
                throw new ArgumentOutOfRangeException("iteration");

            if (recursion < 0)
                throw new ArgumentOutOfRangeException("recursion");

            var s = alphas.Substring(0, index + 1) + iteration + recursion;

            return s;

        }
    }
}
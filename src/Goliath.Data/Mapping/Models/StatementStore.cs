using System;
using System.Collections.Generic;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public class StatementStore : IEnumerable<StatementMap>
    {

        /// <summary>
        /// Gets the platform.
        /// </summary>
        public RdbmsBackend Platform { get; private set; }

        internal Dictionary<string, StatementMap> InnerProcedureList { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementStore"/> class.
        /// </summary>
        internal StatementStore()
            : this(RdbmsBackend.SupportedSystemNames.Sqlite3)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementStore"/> class.
        /// </summary>
        /// <param name="platform">The platform.</param>
        public StatementStore(string platform)
        {
            //platformId = platform;
            SetPlatform(platform);
            InnerProcedureList = new Dictionary<string, StatementMap>();
        }

        internal void SetPlatform(string platform)
        {
            Lazy<RdbmsBackend> rdbms;

            if (!RdbmsBackend.TryGetBackend(platform, out rdbms))
                throw new GoliathDataException(string.Format("Database {0} not supported ", platform));

            Platform = rdbms.Value;
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public void Add(StatementMap item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (string.IsNullOrEmpty(item.CanRunOn))
            {
                InnerProcedureList.Add(item.Name, item);
            }
            else
            {
                var supportedDbs = item.CanRunOn.Split(new string[] { ", ", ";", " " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var rdbms in supportedDbs)
                {
                    if (Platform.Id.Equals(rdbms.ToUpper()) || Platform.CompatibilityGroup.Contains(rdbms.ToUpper()))
                    {
                        InnerProcedureList.Add(item.Name, item);
                        break;
                    }
                }
            }            
        }

        /// <summary>
        /// Adds the specified map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="type">The type.</param>
        /// <param name="body">The body.</param>
        public StatementMap Add(EntityMap map, MappedStatementType type, string body)
        {
            string procName = BuildMappedStatementName(map, type);
            return Add(procName, procName, type, body, Platform.Id);
        }

        /// <summary>
        /// Adds the specified map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="type">The type.</param>
        /// <param name="body">The body.</param>
        public StatementMap Add(EntityMap map, string dbName, MappedStatementType type, string body)
        {
            return Add(map, dbName, type, body, Platform.Id);
        }

        /// <summary>
        /// Adds the specified map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="type">The type.</param>
        /// <param name="body">The body.</param>
        /// <param name="supportedRdbms">The supported RDBMS.</param>
        /// <returns></returns>
        public StatementMap Add(EntityMap map, string dbName, MappedStatementType type, string body, string supportedRdbms)
        {
            string procName = BuildMappedStatementName(map, type);
            var proc = new StatementMap(procName, dbName, type) { Body = body, CanRunOn = supportedRdbms, DependsOnEntity = map.FullName };
           Add(proc);
           return proc;
        }

        /// <summary>
        /// Adds the specified map.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="type">The type.</param>
        /// <param name="body">The body.</param>
        /// <param name="supportedRdbms">The supported RDBMS.</param>
        /// <returns></returns>
        public StatementMap Add(string procedureName, string dbName, MappedStatementType type, string body, string supportedRdbms)
        {
            var proc = new StatementMap(procedureName, dbName, type) { Body = body, CanRunOn = supportedRdbms };
            Add(proc);
            return proc;
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="procType">Type of the proc.</param>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public bool TryGetValue(Type type, MappedStatementType procType, out StatementMap val)
        {
            var procName = BuildMappedStatementName(type, procType);
            return TryGetValue(procName, out val);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="procType">Type of the proc.</param>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public bool TryGetValue(EntityMap map, MappedStatementType procType, out StatementMap val)
        {
            var procName = BuildMappedStatementName(map, procType);
            return TryGetValue(procName, out val);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public bool TryGetValue(string key, out StatementMap val)
        {
            return InnerProcedureList.TryGetValue(key, out val);
        }

        /// <summary>
        /// Builds the name of the procedure.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public  static string BuildMappedStatementName(EntityMap map, MappedStatementType type)
        {
            return string.Format("{0}_{1}", map.FullName, type);
        }

        /// <summary>
        /// Builds the name of the procedure.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="procType">Type of the proc.</param>
        /// <returns></returns>
        public static string BuildMappedStatementName(Type type, MappedStatementType procType)
        {
            return string.Format("{0}_{1}", type.FullName, procType);
        }

        #region IEnumerable<SqlProcedure> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<StatementMap> GetEnumerator()
        {
            return InnerProcedureList.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlProcedureStore : IEnumerable<SqlProcedure>
    {
        
        public RdbmsBackend Platform { get; private set; }

        internal Dictionary<string, SqlProcedure> InnerProcedureList { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlProcedureStore"/> class.
        /// </summary>
        internal SqlProcedureStore()
            : this(RdbmsBackend.SupportedSystemNames.Sqlite3)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlProcedureStore"/> class.
        /// </summary>
        /// <param name="platform">The platform.</param>
        public SqlProcedureStore(string platform)
        {
            //platformId = platform;
            SetPlatform(platform);
            InnerProcedureList = new Dictionary<string, SqlProcedure>();
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
        public void Add(SqlProcedure item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (string.IsNullOrEmpty(item.CanRunOn))
            {
                InnerProcedureList.Add(item.Name, item);
            }
            else
            {
                string[] supportedDbs = item.CanRunOn.Split(new string[] { ", ", ";", " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var rdbms in supportedDbs)
                {
                    if (Platform.Id.Equals(rdbms) || Platform.CompatibilityGroup.Contains(rdbms))
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
        public SqlProcedure Add(EntityMap map, ProcedureType type, string body)
        {
            string procName = BuildProcedureName(map, type);
            return Add(procName, procName, type, body, Platform.Id);
        }

        /// <summary>
        /// Adds the specified map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="type">The type.</param>
        /// <param name="body">The body.</param>
        public SqlProcedure Add(EntityMap map, string dbName, ProcedureType type, string body)
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
        public SqlProcedure Add(EntityMap map, string dbName, ProcedureType type, string body, string supportedRdbms)
        {
            string procName = BuildProcedureName(map, type);
            SqlProcedure proc = new SqlProcedure(procName, dbName, type) { Body = body, CanRunOn = supportedRdbms, DependsOnEntity = map.FullName };
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
        public SqlProcedure Add(string procedureName, string dbName, ProcedureType type, string body, string supportedRdbms)
        {
            SqlProcedure proc = new SqlProcedure(procedureName, dbName, type) { Body = body, CanRunOn = supportedRdbms };
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
        public bool TryGetValue(Type type, ProcedureType procType, out SqlProcedure val)
        {
            var procName = BuildProcedureName(type, procType);
            return TryGetValue(procName, out val);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="procType">Type of the proc.</param>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public bool TryGetValue(EntityMap map, ProcedureType procType, out SqlProcedure val)
        {
            var procName = BuildProcedureName(map, procType);
            return TryGetValue(procName, out val);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public bool TryGetValue(string key, out SqlProcedure val)
        {
            return InnerProcedureList.TryGetValue(key, out val);
        }

        internal static string BuildProcedureName(EntityMap map, ProcedureType type)
        {
            return string.Format("{0}_{1}", map.FullName, type); ;
        }

        internal static string BuildProcedureName(Type type, ProcedureType procType)
        {
            return string.Format("{0}_{1}", type.FullName, procType); ;
        }



        #region IEnumerable<SqlProcedure> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<SqlProcedure> GetEnumerator()
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

    //class SqlProcSet
    //{
    //    //NOTE: using integer instead of the enum as dictionary key, better performance.
    //    Dictionary<string, SqlProcedure> procedures = new Dictionary<string, SqlProcedure>();

    //    public string Name { get; private set; }

    //    public Dictionary<string, SqlProcedure> Procedures
    //    {
    //        get { return procedures; }
    //    }

    //    public SqlProcSet(string name)
    //    {
    //        Name = name;
    //    }

    //    #region Access methods

    //    public void Add(SqlProcedure proc)
    //    {
    //        if (proc == null)
    //            throw new ArgumentNullException("proc");

    //        string id = proc.CanRunOn;
    //        Add(id, proc);
    //    }

    //    public void Add(string id, SqlProcedure proc)
    //    {
    //        if (procedures.ContainsKey(id))
    //            throw new GoliathDataException(string.Format("Procedure already exists {0} - {1} - {2}. Could not add.", proc.Name, proc.OperationType, proc.CanRunOn));

    //        procedures.Add(id, proc);
    //    }

    //    public bool Remove(string platform)
    //    {
    //        return procedures.Remove(platform);
    //    }

    //    public bool Remove(SqlProcedure proc)
    //    {
    //        if (proc == null)
    //            throw new ArgumentNullException("proc");

    //        return Remove(proc.CanRunOn);
    //    }

    //    public bool Contains(string id)
    //    {
    //        return procedures.ContainsKey(id);
    //    }

    //    #endregion

    //    public bool TryGetValue(string platform, out SqlProcedure proc)
    //    {
    //        return procedures.TryGetValue(platform, out proc);
    //    }

    //}
}

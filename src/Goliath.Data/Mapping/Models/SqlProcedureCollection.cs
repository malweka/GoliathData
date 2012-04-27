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
    public class SqlProcedureCollection : ICollection<SqlProcedure>
    {
        internal SupportedRdbms Platform { get; set; }
        Dictionary<string, SqlProcSet> innerProcedureList;
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlProcedureCollection"/> class.
        /// </summary>
        public SqlProcedureCollection()
            : this(SupportedRdbms.Sqlite3)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlProcedureCollection"/> class.
        /// </summary>
        /// <param name="platform">The platform.</param>
        public SqlProcedureCollection(SupportedRdbms platform)
        {
            Platform = platform;
            innerProcedureList = new Dictionary<string, SqlProcSet>();
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public void Add(SqlProcedure item)
        {
            SqlProcSet procSet;
            if (innerProcedureList.TryGetValue(item.Name, out procSet))
            {
                procSet.Add(item);
            }
            else
            {
                procSet = new SqlProcSet(item.Name);
                procSet.Add(item);
                innerProcedureList.Add(procSet.Name, procSet);
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
            return Add(procName, procName, type, body, Platform);
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
            return Add(map, dbName, type, body, Platform);
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
        public SqlProcedure Add(EntityMap map, string dbName, ProcedureType type, string body, SupportedRdbms supportedRdbms)
        {
            string procName = BuildProcedureName(map, type);
            return Add(procName, dbName, type, body, supportedRdbms);
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
        public SqlProcedure Add(string procedureName, string dbName, ProcedureType type, string body, SupportedRdbms supportedRdbms)
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
            //if (base.TryGetValue(key, out val))
            //{
            //    if ((val.CanRunOn & Platform) == Platform)
            //    {
            //        return true;
            //    }
            //}

            //val = null;
            //return false;
            throw new NotImplementedException();

        }

        internal static string BuildProcedureName(EntityMap map, ProcedureType type)
        {
            return string.Format("{0}_{1}", map.FullName, type); ;
        }

        internal static string BuildProcedureName(Type type, ProcedureType procType)
        {
            return string.Format("{0}_{1}", type.FullName, procType); ;
        }

        #region ICollection<SqlProcedure> Members



        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(SqlProcedure item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(SqlProcedure[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(SqlProcedure item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<SqlProcedure> Members

        public IEnumerator<SqlProcedure> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        class SqlProcSet
        {
            //NOTE: using integer instead of the enum as dictionary key, better performance.
            Dictionary<int, SqlProcedure> procedures = new Dictionary<int, SqlProcedure>();

            public string Name { get; private set; }

            public SqlProcSet(string name)
            {
                Name = name;
            }

            #region Access methods

            public void Add(SqlProcedure proc)
            {
                if (proc == null)
                    throw new ArgumentNullException("proc");

                int id = (int)proc.CanRunOn;
                Add(id, proc);
            }

            public void Add(int id, SqlProcedure proc)
            {
                if (procedures.ContainsKey(id))
                    throw new GoliathDataException(string.Format("Procedure already exists {0} - {1} - {2}. Couldn't not add.", proc.Name, proc.OperationType, proc.CanRunOn));

                procedures.Add(id, proc);
            }

            public bool Remove(int id)
            {
                return procedures.Remove(id);
            }

            public bool Remove(SupportedRdbms platform)
            {
                return Remove((int)platform);
            }

            public bool Remove(SqlProcedure proc)
            {
                if (proc == null)
                    throw new ArgumentNullException("proc");

                return Remove((int)proc.CanRunOn);
            }

            public bool Contains(int id)
            {
                return procedures.ContainsKey(id);
            }

            public bool Contains(SupportedRdbms platform)
            {
                return Contains((int)platform);
            }

            #endregion

            public bool TryGetValue(int key, out SqlProcedure proc)
            {
                return procedures.TryGetValue(key, out proc);
            }

            public bool TryGetValue(SupportedRdbms platform, out SqlProcedure proc)
            {
                return TryGetValue((int)platform, out proc);
            }

        }
    }


}

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
    public class SqlProcedureCollection : KeyedCollectionBase<string, SqlProcedure>
    {
        internal SupportedRdbms Platform{get;set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlProcedureCollection"/> class.
        /// </summary>
        public SqlProcedureCollection()
        {
            Platform = SupportedRdbms.All;
        }

        internal static string BuildProcedureName(EntityMap map, ProcedureType type)
        {
            return string.Format("{0}_{1}", map.FullName, type); ;
        }

        internal static string BuildProcedureName(Type type, ProcedureType procType)
        {
            return string.Format("{0}_{1}", type.FullName, procType); ;
        }

        /// <summary>
        /// Adds the specified map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="type">The type.</param>
        /// <param name="body">The body.</param>
        public void Add(EntityMap map, ProcedureType type, string body)
        {
            string procName = BuildProcedureName(map, type);
            Add(procName, procName, type, body);
        }

        /// <summary>
        /// Adds the specified map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="type">The type.</param>
        /// <param name="body">The body.</param>
        public void Add(EntityMap map, string dbName, ProcedureType type, string body)
        {
            string procName = BuildProcedureName(map, type);
            Add(procName, dbName, type, body);
        }

        /// <summary>
        /// Adds the specified map.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="type">The type.</param>
        /// <param name="body">The body.</param>
        public void Add(string procedureName, string dbName, ProcedureType type, string body)
        {
            SqlProcedure proc = new SqlProcedure(procedureName, dbName, type) { Body = body };
            Add(proc);
        }

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>
        /// The key for the specified element.
        /// </returns>
        protected override string GetKeyForItem(SqlProcedure item)
        {
            return item.Name;
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
    }
}

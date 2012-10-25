using System;
using System.Collections.Generic;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class RdbmsBackend
    {
        readonly static Dictionary<string, Lazy<RdbmsBackend>> supportedRdbms = new Dictionary<string, Lazy<RdbmsBackend>> { 
            { SupportedSystemNames.Mssql2005, new Lazy<RdbmsBackend>(() => new SqlServer2005Backend()) },
            { SupportedSystemNames.Mssql2008, new Lazy<RdbmsBackend>(() => new SqlServer2008Backend()) },
            { SupportedSystemNames.Mssql2008R2, new Lazy<RdbmsBackend>(() => new SqlServer2008R2Backend()) },
            { SupportedSystemNames.Sqlite3, new Lazy<RdbmsBackend>(() => new SqliteBackend()) },
            { SupportedSystemNames.Postgresql8, new Lazy<RdbmsBackend>(() => new Postgresql8Backend()) },
            { SupportedSystemNames.Postgresql9, new Lazy<RdbmsBackend>(() => new Postgresql9Backend()) },
            { SupportedSystemNames.MySql5, new Lazy<RdbmsBackend>(() => new Mysql5Backend()) },
        };

        /// <summary>
        /// Tries the get backend.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static bool TryGetBackend(string key, out Lazy<RdbmsBackend> item)
        {
            return supportedRdbms.TryGetValue(key, out item);
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public string Id { get; protected set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the compatibility group.
        /// </summary>
        public HashSet<string> CompatibilityGroup { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RdbmsBackend"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public RdbmsBackend(string title)
        {
            Title = title;
            CompatibilityGroup = new HashSet<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        public struct SupportedSystemNames
        {
            /// <summary>
            /// 
            /// </summary>
            public const string Mssql2005 = "Mssql2005";
            /// <summary>
            /// 
            /// </summary>
            public const string Mssql2008 = "Mssql2008";
            /// <summary>
            /// 
            /// </summary>
            public const string Mssql2008R2 = "Mssql2008R2";
            ///// <summary>
            ///// 
            ///// </summary>
            //public const string MssqlAll = "MssqlAll";
            /// <summary>
            /// 
            /// </summary>
            public const string Sqlite3 = "Sqlite3";
            /// <summary>
            /// 
            /// </summary>
            public const string Postgresql8 = "Postgresql8";
            /// <summary>
            /// 
            /// </summary>
            public const string Postgresql9 = "Postgresql9";
            /// <summary>
            /// 
            /// </summary>
            public const string MySql5 = "MySql5";
        }
    }
}

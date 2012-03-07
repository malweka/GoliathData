using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    /// <summary>
    /// Supported RDBMS
    /// </summary>
    [Flags]
    public enum SupportedRdbms
    {
        None = 0,
        /// <summary>
        /// Microsoft SQL Server 2005
        /// </summary>
        Mssql2005 = 1,
        /// <summary>
        /// Microsoft SQL Server 2008
        /// </summary>
        Mssql2008 = 2,
        /// <summary>
        /// Microsoft SQL Server 2008 R2
        /// </summary>
        Mssql2008R2 = 4,
        /// <summary>
        /// All supported version of SQL Server from 2005 to 2008 R2
        /// </summary>
        MssqlAll = Mssql2005 | Mssql2008 | Mssql2008R2,
        /// <summary>
        /// Sqlite 3
        /// </summary>
        Sqlite3 = 8,
        /// <summary>
        /// PostgreSQL 9.x
        /// </summary>
        Postgresql9 = 16,
        /// <summary>
        /// MySql 5
        /// </summary>
        MySql5 = 32,
        /// <summary>
        /// All systems
        /// </summary>
        All = MssqlAll | Sqlite3 | Postgresql9 | MySql5,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// 
        /// </summary>
        None,
        /// <summary>
        /// 
        /// </summary>
        Ascending,
        /// <summary>
        /// 
        /// </summary>
        Descending,
    }
}

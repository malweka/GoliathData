using System;
using System.Collections.Generic;
using Goliath.Data.Mapping;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateSqlExecutionList
    {
        readonly Dictionary<string, UpdateSqlBodyInfo> statements = new Dictionary<string, UpdateSqlBodyInfo>();
        readonly List<Tuple<string,List<QueryParam>>> manyToManyStatements = new List<Tuple<string, List<QueryParam>>>();

        //private readonly Dictionary<string, Tuple<string, string, object>> columnsTableMap = new Dictionary<string, Tuple<string, string, object>>();

        /// <summary>
        /// Gets the statements.
        /// </summary>
        /// <value>
        /// The statements.
        /// </value>
        public Dictionary<string, UpdateSqlBodyInfo> Statements
        {
            get { return statements; }
        }

        /// <summary>
        /// Gets the many to many statements.
        /// </summary>
        /// <value>
        /// The many to many statements.
        /// </value>
        public List<Tuple<string, List<QueryParam>>> ManyToManyStatements { get { return manyToManyStatements; } }

    }
}
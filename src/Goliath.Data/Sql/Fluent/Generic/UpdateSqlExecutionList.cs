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

        ///// <summary>
        ///// Adds the column.
        ///// </summary>
        ///// <param name="entityMapName">Name of the entity map.</param>
        ///// <param name="property">The property.</param>
        ///// <param name="value">The value.</param>
        ///// <exception cref="MappingException">Entity  + entityMapName +  contains more than one property named  + property.PropertyName</exception>
        //public void AddColumn(string entityMapName, Property property, object value)
        //{
        //    if (columnsTableMap.ContainsKey(property.Name))
        //        throw new MappingException("Entity " + entityMapName + " contains more than one property named " + property.PropertyName);

        //    columnsTableMap.Add(property.Name, Tuple.Create(entityMapName, property.ColumnName, value));
        //}
    }
}
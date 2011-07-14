using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Mapping;

namespace Goliath.Data.Sql
{
    class SqlJoin : SqlStatement
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; private set; }

        /// <summary>
        /// Gets the on table.
        /// </summary>
        public EntityMap OnEntityMap { get; private set; }
        /// <summary>
        /// Gets the type of the join.
        /// </summary>
        /// <value>
        /// The type of the join.
        /// </value>
        public JoinType JoinType { get; private set; }
        /// <summary>
        /// Gets the left column.
        /// </summary>
        public Relation LeftColumn { get; private set; }
        /// <summary>
        /// Gets the right column.
        /// </summary>
        public string RightColumn { get; private set; }

        //public bool IsLazy { get; set; }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SqlJoin"/> class.
        ///// </summary>
        ///// <param name="joinType">Type of the join.</param>
        ///// <param name="tableName">Name of the table.</param>
        public SqlJoin(JoinType joinType)
        {
            JoinType = joinType;
            //if (!string.IsNullOrWhiteSpace(tableName))
            //    TableName = tableName;
        }

        /// <summary>
        /// Called when [table].
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public SqlJoin OnTable(EntityMap entity)
        {
            OnEntityMap = entity;
            return this;
        }

        /// <summary>
        /// Called when [left column].
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public SqlJoin OnLeftColumn(Relation column)
        {
            LeftColumn = column;
            return this;
        }

        /// <summary>
        /// Called when [right column].
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public SqlJoin OnRightColumn(string column)
        {
            RightColumn = column;
            return this;
        }
    }


}

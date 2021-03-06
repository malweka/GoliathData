﻿using System;
using System.Collections.Generic;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{CommandType} = {SqlText}")]
    [Serializable]
    public struct SqlOperationInfo
    {
        /// <summary>
        /// Gets or sets the type of the command.
        /// </summary>
        /// <value>
        /// The type of the command.
        /// </value>
        public SqlStatementType CommandType { get; set; }
        /// <summary>
        /// Gets or sets the SQL text.
        /// </summary>
        /// <value>
        /// The SQL text.
        /// </value>
        public string SqlText { get; set; }
        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public ICollection<QueryParam> Parameters { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Priority = {Priority}, Operation = {Operation.SqlText}")]
    [Serializable]
    public struct KeyGenOperationInfo
    {
        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>
        /// The type of the property.
        /// </value>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public object Entity { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public SqlOperationPriority Priority { get; set; }
        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public SqlOperationInfo Operation { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Priority = {Priority}, Operations = {Operations.Count}, SubOperations = {SubOperations.Count}")]
    [Serializable]
    public class BatchSqlOperation : IDisposable
    {
        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public SqlOperationPriority Priority { get; set; }
        /// <summary>
        /// Gets or sets the key generation operations.
        /// </summary>
        /// <value>
        /// The key generation operations.
        /// </value>
        public Dictionary<string, KeyGenOperationInfo> KeyGenerationOperations { get; set; }
        /// <summary>
        /// Gets or sets the operations.
        /// </summary>
        /// <value>
        /// The operations.
        /// </value>
        public List<SqlOperationInfo> Operations { get; set; }
        /// <summary>
        /// Gets or sets the sub operations.
        /// </summary>
        /// <value>
        /// The sub operations.
        /// </value>
        public List<BatchSqlOperation> SubOperations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchSqlOperation"/> class.
        /// </summary>
        public BatchSqlOperation()
        {
            Operations = new List<SqlOperationInfo>();
            SubOperations = new List<BatchSqlOperation>();
            KeyGenerationOperations = new Dictionary<string, KeyGenOperationInfo>();
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Operations != null)
            {
                Operations.Clear();
            }
            if (SubOperations != null)
            {
                SubOperations.Clear();
            }
            if (KeyGenerationOperations != null)
            {
                KeyGenerationOperations.Clear();
            }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
     [Serializable]
    public enum SqlOperationPriority
    {
        /// <summary>
        /// 
        /// </summary>
        Low = 0,
        /// <summary>
        /// 
        /// </summary>
        Medium = 1,
        /// <summary>
        /// 
        /// </summary>
        High = 5,
        /// <summary>
        /// 
        /// </summary>
        Highest = 9999,
    }
}

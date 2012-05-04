using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    using Sql;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class SqlProcedure : IMapModel
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();

        /// <summary>
        /// Gets a value indicating whether this instance is ready.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </value>
        public bool IsReady { get; internal set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public Dictionary<string, object> Parameters
        {
            get
            {
                return parameters;
            }
        }

        /// <summary>
        /// Gets or sets the type of the operation.
        /// </summary>
        /// <value>The type of the operation.</value>
        public virtual ProcedureType OperationType { get; private set; }

        /// <summary>
        /// Gets or sets the can run on.
        /// </summary>
        /// <value>
        /// The can run on.
        /// </value>
        public string CanRunOn { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the name of the db.
        /// </summary>
        /// <value>The name of the db.</value>
        public string DbName { get; private set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <value>The body.</value>
        public string Body { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlProcedure"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="operationType">Type of the operation.</param>
        public SqlProcedure(string name, string dbName, ProcedureType operationType)
        {
            Name = name;
            DbName = dbName;
            OperationType = operationType;
        }


        /// <summary>
        /// Loads this instance.
        /// </summary>
        public virtual void Load()
        {
            IsReady = true;
        }


    }

    /// <summary>
    /// Dynamic Sql Procedure
    /// </summary>
    [Serializable]
    [DataContract]
    public class DynamicSqlProcedure: SqlProcedure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSqlProcedure"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="operationType">Type of the operation.</param>
        public DynamicSqlProcedure(string name, string dbName, ProcedureType operationType):base(name, dbName, operationType){}

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public override void Load()
        {
            if (!IsReady)
            {
                Compile();
                base.Load();
            }
        }

        /// <summary>
        /// Compiles this instance.
        /// </summary>
        public void Compile()
        {

        }
    }
}

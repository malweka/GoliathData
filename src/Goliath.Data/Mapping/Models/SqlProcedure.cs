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
        public SupportedRdbms CanRunOn { get; set; }

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


    }
}

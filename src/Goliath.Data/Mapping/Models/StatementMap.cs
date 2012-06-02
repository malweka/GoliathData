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
    public class StatementMap : IMapModel
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();

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
        public Dictionary<string, string> ParametersMap
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
        public virtual MappedStatementType OperationType { get; internal set; }

        /// <summary>
        /// Gets or sets the depends on entity. This is mainly for code generation -- think about moving it.
        /// </summary>
        /// <value>
        /// The depends on entity.
        /// </value>
        public string DependsOnEntity { get; set; }

        /// <summary>
        /// The system full type name of the input object that will be passed to the code executing the procedure. 
        /// </summary>
        /// <value>
        /// The type of the input parameter.
        /// </value>
        public string InputParameterType { get; set; }

        /// <summary>
        /// The type to serialize the result of the procedure.
        /// </summary>
        /// <value>
        /// The result map.
        /// </value>
        public string ResultMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [result is collection].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [result is collection]; otherwise, <c>false</c>.
        /// </value>
        public bool ResultIsCollection { get; set; }

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
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the name of the db.
        /// </summary>
        /// <value>The name of the db.</value>
        public string DbName { get; internal set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <value>The body.</value>
        public string Body { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementMap"/> class.
        /// </summary>
        internal StatementMap(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementMap"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="operationType">Type of the operation.</param>
        public StatementMap(string name, string dbName, MappedStatementType operationType)
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
    public class DynamicStatementMap: StatementMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicStatementMap"/> class.
        /// </summary>
        internal DynamicStatementMap(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicStatementMap"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="operationType">Type of the operation.</param>
        public DynamicStatementMap(string name, string dbName, MappedStatementType operationType):base(name, dbName, operationType){}

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

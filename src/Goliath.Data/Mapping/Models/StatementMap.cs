using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class StatementMap : IMapModel
    {
        Dictionary<string, QueryParam> parameters = new Dictionary<string, QueryParam>();
        Dictionary<string, string> inputParams = new Dictionary<string, string>();
        Dictionary<string, StatementInputParam> paramPropertyMap = new Dictionary<string, StatementInputParam>();
        /// <summary>
        /// Gets a value indicating whether this instance is ready.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </value>
        public bool IsReady { get; internal set; }


        /// <summary>
        /// Gets a value indicating whether this instance requires parsing.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance requires parsing; otherwise, <c>false</c>.
        /// </value>
        public bool IsParsingRequired { get; set; }

        //internal bool IsParsingRequired
        //{
        //    get
        //    {
        //        if ((parameters.Count > 0) || (inputParams.Count > 0))
        //        {
        //            return true;
        //        }
        //        else
        //            return false;
        //    }
        //}

        internal Dictionary<string, StatementInputParam> ParamPropertyMap
        {
            get { return paramPropertyMap; }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public Dictionary<string, QueryParam> DBParametersMap
        {
            get
            {
                return parameters;
            }
        }

        /// <summary>
        /// Gets the input parameters map.
        /// </summary>
        public Dictionary<string, string> InputParametersMap
        {
            get { return inputParams; }
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

        ///// <summary>
        ///// The system full type name of the input object that will be passed to the code executing the procedure. 
        ///// </summary>
        ///// <value>
        ///// The type of the input parameter.
        ///// </value>
        //public string InputParameterType { get; set; }

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
        public string Body { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementMap"/> class.
        /// </summary>
        internal StatementMap() : this(null, null, MappedStatementType.Undefined) { }

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

    }
}

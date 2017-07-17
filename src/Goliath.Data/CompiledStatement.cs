using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Body}")]
    public class CompiledStatement
    {
        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public string Body { get; set; }

        readonly Dictionary<string, StatementInputParam> paramPropertyMap = new Dictionary<string, StatementInputParam>();

        /// <summary>
        /// Gets the parameter property map.
        /// </summary>
        /// <value>
        /// The parameter property map.
        /// </value>
        public Dictionary<string, StatementInputParam> ParamPropertyMap => paramPropertyMap;
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Name={Name}, Type={Type}")]
    public class StatementInputParam
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name{get;set;}
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value{get;set;}
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type{get;set;}
        /// <summary>
        /// Gets or sets a value indicating whether this instance is mapped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is mapped; otherwise, <c>false</c>.
        /// </value>
        public bool IsMapped{get;set;}

        internal Goliath.Data.Mapping.EntityMap Map { get; set; }
        internal string QueryParamName { get; set; }
        internal Type ClrType { get; set; }
        internal VarPropNameInfo Property { get; set; }
    }


    struct VarPropNameInfo
    {
        public string VarName;
        public string PropName;
    }
}

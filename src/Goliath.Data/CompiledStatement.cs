using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    [System.Diagnostics.DebuggerDisplay("{Body}")]
    public class CompiledStatement
    {
        public string Body { get; set; }

        Dictionary<string, StatementInputParam> paramPropertyMap = new Dictionary<string, StatementInputParam>();

        public Dictionary<string, StatementInputParam> ParamPropertyMap
        {
            get { return paramPropertyMap; }
        }
    }

    [System.Diagnostics.DebuggerDisplay("Name={Name}, Type={Type}")]
    public class StatementInputParam
    {
        public string Name{get;set;}
        public string Value{get;set;}
        public string Type{get;set;}
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

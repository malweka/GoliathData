using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    [System.Diagnostics.DebuggerDisplay("{Body}")]
    class CompiledStatement
    {
        public string Body { get; set; }

        Dictionary<string, StatemenInputParam> paramPropertyMap = new Dictionary<string, StatemenInputParam>();

        public Dictionary<string, StatemenInputParam> ParamPropertyMap
        {
            get { return paramPropertyMap; }
        }
    }

    [System.Diagnostics.DebuggerDisplay("Name={Name}, Type={Type}")]
    struct StatemenInputParam
    {
        public string Name;
        public string Value;
        public string Type;
        public bool IsMapped;
    }
}

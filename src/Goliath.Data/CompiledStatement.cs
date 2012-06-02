using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    class CompiledStatement
    {
        public string Body { get; set; }

        Dictionary<string, string> paramPropertyMap = new Dictionary<string, string>();

        public Dictionary<string, string> ParamPropertyMap
        {
            get { return paramPropertyMap; }
        }
    }
}

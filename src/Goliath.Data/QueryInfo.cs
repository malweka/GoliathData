using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    public struct QueryInfo
    {
        public string QuerySqlText { get; set; }
        public ICollection<QueryParam> Parameters { get; set; }
    }
}

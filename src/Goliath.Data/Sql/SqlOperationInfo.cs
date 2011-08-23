using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    public struct SqlOperationInfo
    {
        public SqlStatementType CommandType { get; set; }
        public string SqlText { get; set; }
        public ICollection<QueryParam> Parameters { get; set; }
    }
}

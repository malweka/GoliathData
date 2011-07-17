using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    class GroupBy : SqlStatement
    {
        public string Column { get; private set; }

        public GroupBy(string column)
        {
            Column = column;
        }
    }
}

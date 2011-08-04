using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// Sql Part
    /// </summary>
    public class SqlPart
    {
        protected List<string> parts = new List<string>();
    }

    /// <summary>
    /// column list sql part
    /// </summary>
    public class ColumnListSqlPart : SqlPart
    {
    }
}

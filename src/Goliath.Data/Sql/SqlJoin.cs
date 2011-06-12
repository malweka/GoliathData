using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    class SqlJoin
    {
        //public string TableName { get; private set; }
         
        public string OnTable { get; private set; }
        public JoinType JoinType { get; private set; }
        public string LeftColumn { get; set; }
        public string RightColumn { get; set; }

        public SqlJoin(JoinType joinType, string onTable)
        {
            JoinType = joinType;
            OnTable = onTable;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    enum JoinType
    {
        Inner = 1,
        Left = 2,
        Right = 4,
        Full = 8
    }
}

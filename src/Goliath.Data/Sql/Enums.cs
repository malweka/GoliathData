using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    enum JoinType
    {
        Inner = 1,
        Left = 2,
        Right = 4,
        Full = 8
    }

    /// <summary>
    /// 
    /// </summary>
    enum ComparisonOperator
    {
        Equals = 0,
        NotEquals,
        Like,
        NotLike,
        GreaterThan,
        GreaterOrEquals,
        LessThan,
        LessOrEquals,
        And = 400,
        Or = 401,
        IsNull = 500,
        IsNotNull=501,
        In = 600
    }

    enum SortType
    {
        Ascending,
        Descinding,
    }
}

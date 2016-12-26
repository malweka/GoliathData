
namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public enum JoinType
    {
        /// <summary>
        /// 
        /// </summary>
        Inner = 1,
        /// <summary>
        /// 
        /// </summary>
        Left = 2,
        /// <summary>
        /// 
        /// </summary>
        Right = 4,
        /// <summary>
        /// 
        /// </summary>
        Full = 8
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ComparisonOperator
    {
        /// <summary>
        /// 
        /// </summary>
        Equal = 0,
        /// <summary>
        /// 
        /// </summary>
        NotEqual,
        /// <summary>
        /// 
        /// </summary>
        Like,
        /// <summary>
        /// The like non case sensitive
        /// </summary>
        LikeNonCaseSensitive,
        /// <summary>
        /// 
        /// </summary>
        NotLike,
        /// <summary>
        /// 
        /// </summary>
        GreaterThan,
        /// <summary>
        /// 
        /// </summary>
        GreaterOrEquals,
        /// <summary>
        /// 
        /// </summary>
        LowerThan,
        /// <summary>
        /// 
        /// </summary>
        LowerOrEquals,
        //And = 400,
        //Or = 401,
        /// <summary>
        /// 
        /// </summary>
        IsNull = 500,
        /// <summary>
        /// 
        /// </summary>
        IsNotNull=501,
        /// <summary>
        /// 
        /// </summary>
        In = 600
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SqlOperator
    {
        /// <summary>
        /// 
        /// </summary>
        AND = 0,
        /// <summary>
        /// 
        /// </summary>
        OR,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SortType
    {
        /// <summary>
        /// 
        /// </summary>
        Ascending,
        /// <summary>
        /// 
        /// </summary>
        Descinding,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SqlStatementType
    {
        /// <summary>
        /// 
        /// </summary>
        Select = 0,
        /// <summary>
        /// 
        /// </summary>
        Insert,
        /// <summary>
        /// 
        /// </summary>
        Update,
        /// <summary>
        /// 
        /// </summary>
        Delete,      
    }
}

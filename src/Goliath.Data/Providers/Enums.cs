using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public enum EscapeValueType
    {
        /// <summary>
        /// The generic string
        /// </summary>
        GenericString = 0,
        /// <summary>
        /// The column
        /// </summary>
        Column,
        /// <summary>
        /// The table name
        /// </summary>
        TableName,
        /// <summary>
        /// The parameter
        /// </summary>
        Parameter,
    }
}

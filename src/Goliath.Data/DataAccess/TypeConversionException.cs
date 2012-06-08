using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class TypeConversionException : DataAccessException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConversionException"/> class.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public TypeConversionException(Type from, Type to)
            : base("Could not convert {0} to {1}", from, to)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConversionException"/> class.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="innerException">The inner exception.</param>
        public TypeConversionException(Type from, Type to, Exception innerException)
            : base(string.Format("Could not convert {0} to {1}", from, to), innerException)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    /// <summary>
    /// Data Access Exception
    /// </summary>
    public class DataAccessException : GoliathDataException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessException"/> class.
        /// </summary>
        /// <param name="messageFormat">The message format.</param>
        /// <param name="args">The args.</param>
        public DataAccessException(string messageFormat, params object[] args) : base(string.Format(messageFormat, args)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
    }
}

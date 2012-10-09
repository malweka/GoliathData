using System;

namespace Goliath.Data
{
    /// <summary>
    /// Goliath Data base Exception
    /// </summary>
    public class GoliathDataException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoliathDataException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public GoliathDataException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GoliathDataException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public GoliathDataException(string message, Exception innerException) : base(message, innerException) { }
    }
}

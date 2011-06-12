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

        public DataAccessException(string messageFormat, params object[] args) : base(string.Format(messageFormat, args)) { } 

        public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
    }
}

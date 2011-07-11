using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.DataAccess
{
    public class TypeConversionException : DataAccessException
    {
        public TypeConversionException(Type from, Type to)
            : base("Could not convert {0} to {1}", from, to)
        {
        }

        public TypeConversionException(Type from, Type to, Exception innerException)
            : base(string.Format("Could not convert {0} to {1}", from, to), innerException)
        {
        }
    }
}

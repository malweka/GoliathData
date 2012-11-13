using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Providers
{
    public enum EscapeValueType
    {
        GenericString = 0,
        Column,
        TableName,
        Parameter,
    }
}

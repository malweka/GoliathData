using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Goliath.Data.Mapping
{
    public interface ISqlTypeConverter
    {
        Type GetClrType(DbType dbType, bool isNullable);
        Type GetClrType(string typeName, bool isNullable);
    }
}

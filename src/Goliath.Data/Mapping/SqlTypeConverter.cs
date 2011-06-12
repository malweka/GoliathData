using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Goliath.Data.Mapping
{
    class SqlTypeConverter : ISqlTypeConverter
    {
        #region ISqlTypeConverter Members

        public Type GetClrType(DbType dbType, bool isNullable)
        {
            return SqlTypeHelper.GetClrType(dbType, isNullable);
        }

        public Type GetClrType(string typeName, bool isNullable)
        {
            throw new NotImplementedException();
        }
        //public string PrintTypeToString(Type type, bool nullable = false)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}

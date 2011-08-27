using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Sql;

namespace Goliath.Data.Providers.SqlServer
{
    [Serializable]
    class NewGuid : SqlFunction
    {
        public NewGuid()
            : base(FunctionNames.NewGuid, "newid")
        {
        }

        public override string ToSqlStatement(params QueryParam[] args)
        {
            return base.ToSqlStatement(null);
        }
    }

    [Serializable]
    class GetDate : SqlFunction
    {
        public GetDate()
            : base(FunctionNames.GetDate, "getdate")
        {
        }

        public override string ToSqlStatement(params QueryParam[] args)
        {
            return base.ToSqlStatement(null);
        }
    }

    [Serializable]
    class GetCurrentUser : SqlFunction
    {
        public GetCurrentUser()
            : base(FunctionNames.GetUserName, "suser_sname")
        {
        }
    }

    [Serializable]
    class GetHostName : SqlFunction
    {
        public GetHostName()
            : base(FunctionNames.GetHostName, "host_name")
        {
        }
    }

    [Serializable]
    class GetAppName : SqlFunction
    {
        public GetAppName()
            : base(FunctionNames.GetAppName, "app_name")
        {
        }
    }

    [Serializable]
    class GetDatabaseName : SqlFunction
    {
        public GetDatabaseName()
            : base(FunctionNames.GetDatabaseName, "db_name")
        {
        }
    }

    [Serializable]
    class GetUtcDate : SqlFunction
    {
        public GetUtcDate()
            : base(FunctionNames.GetUtcDate, "getutcdate")
        {

        }

        public override string ToSqlStatement(params QueryParam[] args)
        {
            return base.ToSqlStatement(null);
        }
    }
}

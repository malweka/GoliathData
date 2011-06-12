using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Sql;

namespace Goliath.Data.Providers.SqlServer
{
    class NewGuid : SqlFunction
    {
        public NewGuid()
            : base(Functions.NewGuid, "newid")
        {
        }

        public override string ToSqlStatement(params QueryParam[] args)
        {
            return base.ToSqlStatement(null);
        }
    }

    class GetDate:SqlFunction
    {
        public GetDate()
            : base(Functions.GetDate, "getdate")
        {
        }

        public override string ToSqlStatement(params QueryParam[] args)
        {
            return base.ToSqlStatement(null);
        }
    }

    class GetCurrentUser : SqlFunction
    {
        public GetCurrentUser()
            : base(Functions.GetUserName, "suser_sname")
        {
        }
    }

    class GetHostName : SqlFunction
    {
        public GetHostName()
            : base(Functions.GetHostName, "host_name")
        {
        }
    }

    class GetAppName : SqlFunction
    {
        public GetAppName()
            : base(Functions.GetAppName, "app_name")
        {
        }
    }

    class GetDatabaseName : SqlFunction
    {
        public GetDatabaseName()
            : base(Functions.GetDatabaseName, "db_name")
        {
        }
    }
    class GetUtcDate : SqlFunction
    {
        public GetUtcDate()
            : base(Functions.GetUtcDate, "getutcdate")
        {

        }

        public override string ToSqlStatement(params QueryParam[] args)
        {
            return base.ToSqlStatement(null);
        }
    }
}

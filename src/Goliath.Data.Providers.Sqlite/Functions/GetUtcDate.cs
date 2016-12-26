using System;
using Goliath.Data.Sql;

namespace Goliath.Data.Providers.Sqlite.Functions
{
    [Serializable]
    class GetUtcDate : SqlFunction
    {
        public GetUtcDate()
            : base(FunctionNames.GetUtcDate, "date")
        {

        }

        public override string ToSqlStatement(params QueryParam[] args)
        {
            return $"{Declaration}('now')";
        }
    }
}
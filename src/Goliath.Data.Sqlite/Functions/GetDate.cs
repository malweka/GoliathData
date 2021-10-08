using System;
using Goliath.Data.Sql;

namespace Goliath.Data.Sqlite.Functions
{
    [Serializable]
    class GetDate : SqlFunction
    {
        public GetDate()
            : base(FunctionNames.GetDate, "date")
        {
        }

        public override string ToSqlStatement(params QueryParam[] args)
        {
            return $"{Declaration}('now')";
        }
    }
}
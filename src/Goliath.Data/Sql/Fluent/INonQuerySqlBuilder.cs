using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    public interface INonQuerySqlBuilder
    {
        IFilterNonQueryClause Where(string propertyName);
    }

    public interface IBinaryNonQueryOperation
    {
        IFilterNonQueryClause And(string propertyName);
        IFilterNonQueryClause Or(string propertyName);
        //string Build();
        //int Execute();
    }

    public interface  IFilterNonQueryClause
    {
        IBinaryNonQueryOperation EqualToValue(object value);
        IBinaryNonQueryOperation EqualTo(string propertyName);

        IBinaryNonQueryOperation GreaterThanValue(object value);
        IBinaryNonQueryOperation GreaterThan(string propertyName);

        IBinaryNonQueryOperation GreaterOrEqualToValue(object value);
        IBinaryNonQueryOperation GreaterOrEqualTo(string propertyName);

        IBinaryNonQueryOperation LowerOrEqualToValue(object value);
        IBinaryNonQueryOperation LowerOrEqualTo(string propertyName);

        IBinaryNonQueryOperation LowerThanValue(object value);
        IBinaryNonQueryOperation LowerThan(string propertyName);

        IBinaryNonQueryOperation LikeValue(object value);
        IBinaryNonQueryOperation Like(string propertyName);
    }
}

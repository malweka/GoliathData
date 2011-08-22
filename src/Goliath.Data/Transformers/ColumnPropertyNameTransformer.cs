using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Utils;
using Goliath.Data.Mapping;


namespace Goliath.Data.Transformers
{
    class ColumnPropertyNameTransformer : INameTransformer<Property>
    {
        #region INameTransformer<EntityMap> Members

        public string Transform(Property mapModel, string original)
        {
            string val = original.Pascalize();
            return val;
        }

        #endregion
    }
}

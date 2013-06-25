using Goliath.Data.Mapping;
using Goliath.Data.Utils;


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

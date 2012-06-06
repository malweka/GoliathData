using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Utils;
using Goliath.Data.Mapping;

namespace Goliath.Data.Transformers
{
    using Diagnostics;

    class OneToManyRelationNameTransformer : INameTransformer<Relation>
    {
        static ILogger logger;
        static OneToManyRelationNameTransformer()
        {
            logger = Logger.GetLogger(typeof(OneToManyRelationNameTransformer));
        }

        #region INameTransformer<Relation> Members

        public string Transform(Relation mapModel, string original)
        {
            if (mapModel == null)
                throw new ArgumentNullException("mapModel");

            if (string.IsNullOrWhiteSpace(original))
                throw new ArgumentNullException("original");

            if (mapModel.RelationType != RelationshipType.ManyToOne)
                return original.Pascalize();

            try
            {
                string prefix = "Id";
                if (prefix.Equals(original, StringComparison.OrdinalIgnoreCase))
                {
                    //mapModel.KeyFieldName = original;
                    //mapModel.PropertyName = original;
                    return original.Pascalize();
                }

                if (original.EndsWith(prefix, StringComparison.InvariantCultureIgnoreCase) && !mapModel.IsPrimaryKey)
                {
                    string name = original.Substring(0, original.IndexOf("Id", StringComparison.OrdinalIgnoreCase));
                    //mapModel.PropertyName = name;
                    //mapModel.KeyFieldName = original;
                    return name.Pascalize();
                }
                else
                {
                    //mapModel.KeyFieldName = mapModel.PropertyName + "_Key";
                    return original.Pascalize();
                }

                //return original.Pascalize();
            }
            catch (Exception ex)
            {
                logger.LogException(string.Format("Transform: {0} -> {1}", original, mapModel.Name), ex);
                return original;
            }
        }

        #endregion
    }
}

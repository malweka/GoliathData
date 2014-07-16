using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.CodeGenerator.ViewBuilder
{
    public class EntityViewInfo : KeyedCollectionBase<string, PropertyGroupViewInfo>
    {
        public string EntityName { get; set; }
        public string ResourceType { get; set; }
        public string LabelResourceName { get; set; }
        public string DescriptionResourceName { get; set; }


        protected override string GetKeyForItem(PropertyGroupViewInfo item)
        {
            return item.Name;
        }
    }
}

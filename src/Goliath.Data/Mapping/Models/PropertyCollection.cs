using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
   [Serializable]
   [CollectionDataContract]
    public class PropertyCollection : KeyedCollectionBase<string, Property>
   {
      protected override string GetKeyForItem(Property item)
      {
         return item.PropertyName;
      }
   }

   [Serializable]
   [CollectionDataContract]
   public class RelationCollection : KeyedCollectionBase<string, Relation>
   {
      protected override string GetKeyForItem(Relation item)
      {
         return item.PropertyName;
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
   [Serializable]
   [CollectionDataContract]
   public class PropertyCollection : System.Collections.ObjectModel.KeyedCollection<string, Property>
   {
      public void AddRange(IEnumerable<Property> list)
      {
         foreach (var p in list)
            Add(p);
      }

      protected override string GetKeyForItem(Property item)
      {
         return item.PropertyName;
      }

      //protected override void InsertItem(int index, Property item)
      //{
      //    base.InsertItem(index, item);
      //}
   }

   [Serializable]
   [CollectionDataContract]
   public class RelationCollection : System.Collections.ObjectModel.KeyedCollection<string, Relation>
   {
      public void AddRange(IEnumerable<Relation> list)
      {
         foreach (var r in list)
            Add(r);
      }
      protected override string GetKeyForItem(Relation item)
      {
         return item.PropertyName;
      }
   }
}

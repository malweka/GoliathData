using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
   [Serializable]
   [CollectionDataContract]
   public class EntityCollection : System.Collections.ObjectModel.KeyedCollection<string, EntityMap>
   {
      public void AddRange(IEnumerable<EntityMap> list)
      {
         foreach (var ent in list)
         {
            Add(ent);
         }
      }

      internal void AddRange(IEnumerable<EntityMap> list, MapConfig config)
      {
          foreach (var ent in list)
          {
              ent.Parent = config;
              Add(ent);
          }
      }
      protected override string GetKeyForItem(EntityMap item)
      {
         return item.Name;
      }
   }

   [Serializable]
   [CollectionDataContract]
   public class ComplexTypeCollection : System.Collections.ObjectModel.KeyedCollection<string, ComplexType>
   {
      public void AddRange(IEnumerable<ComplexType> list)
      {
         foreach (var cmp in list)
         {
            Add(cmp);
         }
      }

      internal void AddRange(IEnumerable<ComplexType> list, MapConfig config)
      {
          foreach (var cmp in list)
          {
              Add(cmp);
          }
      }

      protected override string GetKeyForItem(ComplexType item)
      {
         return item.FullName;
      }
   }
}

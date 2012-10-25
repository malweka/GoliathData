using System;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
   [Serializable]
   [CollectionDataContract]
    public class PropertyCollection : KeyedCollectionBase<string, Property>
   {
       /// <summary>
       /// When implemented in a derived class, extracts the key from the specified element.
       /// </summary>
       /// <param name="item">The element from which to extract the key.</param>
       /// <returns>
       /// The key for the specified element.
       /// </returns>
      protected override string GetKeyForItem(Property item)
      {
         return item.PropertyName;
      }
   }

   /// <summary>
   /// 
   /// </summary>
   [Serializable]
   [CollectionDataContract]
   public class RelationCollection : KeyedCollectionBase<string, Relation>
   {
       /// <summary>
       /// When implemented in a derived class, extracts the key from the specified element.
       /// </summary>
       /// <param name="item">The element from which to extract the key.</param>
       /// <returns>
       /// The key for the specified element.
       /// </returns>
      protected override string GetKeyForItem(Relation item)
      {
         return item.PropertyName;
      }
   }
}

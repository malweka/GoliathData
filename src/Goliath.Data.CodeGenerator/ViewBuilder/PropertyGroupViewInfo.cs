namespace Goliath.Data.CodeGenerator.ViewBuilder
{
    public class PropertyGroupViewInfo : KeyedCollectionBase<string, PropertyViewInfo>
    {
        public string Name { get; set; }
        public string LabelResourceName { get; set; }
        public string DescriptionResourceName { get; set; }
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets the key for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected override string GetKeyForItem(PropertyViewInfo item)
        {
            return item.Name;
        }
    }
}
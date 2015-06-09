namespace Goliath.Data.CodeGenerator.ViewBuilder
{
    public class PropertyViewInfo
    {
        public string Name { get; set; }
        public string LabelResourceName { get; set; }
        public string DescriptionResourceName { get; set; }
        public string PromptResourceName { get; set; }
        public string RequiredErrorResourceName { get; set; }
        public string ResourceType { get; set; }
        public bool Editable { get; set; }
        public int Order { get; set; }
    }
}
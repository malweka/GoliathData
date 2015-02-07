using System.Data;

namespace Goliath.Data.CodeGenerator.ViewBuilder
{
    public class EntityControlMap : KeyedCollectionBase<string, ControlInfo>
    {
        public EntityControlMap(string entityFullName)
        {
            EntityFullName = entityFullName;
        }

        public string EntityFullName { get; private set; }

        protected override string GetKeyForItem(ControlInfo item)
        {
            return item.ControlName;
        }
    }

    public class ControlInfo
    {
        public ControlInfo(string controlName, string propertyName)
        {
            PropertyName = propertyName;
            ControlName = controlName;
        }

        public string ControlName { get; private set; }

        public string PropertyName { get; private set; }

        public ControlType ControlType { get; set; }

        public DbType DbType { get; set; }

        public bool IsComplexType { get; set; }

        public int MaxLength { get; set; }

        public string ReferenceEntityName { get; set; }
    }

    public class NumericControlInfo : ControlInfo
    {
        public int Max { get; set; }
        public int Min { get; set; }
        public NumericControlInfo(string controlName, string propertyName) : base(controlName, propertyName) { }
    }

    public enum ControlType
    {
        Textbox = 0,
        ReadOnlyTextbox,
        NumericTickUpDown,
        NumericOnlyTextBox,
        DropDown,
        TextArea,
        CheckBoxList,
        RadioButtonList,
        TextEditor,
        DatePicker,
        Label,
        Button,
        TimePicker,
        ColorPicker,
        Hidden,
    }
}
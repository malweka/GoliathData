using System;
using System.Data;
using Goliath.Data.Mapping;

namespace Goliath.Data.CodeGenerator.ViewBuilder
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityControlMapBuilder
    {
        /// <summary>
        /// Builds the map.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// config
        /// or
        /// entity
        /// </exception>
        public EntityControlMap BuildMap(EntityMap entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var controlMap = new EntityControlMap(entity.FullName);

            if (entity.PrimaryKey != null && entity.PrimaryKey.Keys.Count > 0)
            {
                //primary key should not be editable so by default use read-only text box.
                foreach (var k in entity.PrimaryKey.Keys)
                {
                    Property prop = k;
                    controlMap.Add(new ControlInfo(prop.PropertyName, prop.PropertyName)
                                   {
                                       ControlType = ControlType.Hidden,
                                       DbType = prop.DbType,
                                   });
                }

            }

            foreach (var prop in entity.Properties)
            {
                if (!IsPropertyEditable(prop))
                {
                    var pkCtrl = new ControlInfo(prop.PropertyName, prop.PropertyName)
                                 {
                                     ControlType = ControlType.ReadOnly,
                                     DbType = prop.DbType,
                                 };
                    ReplaceWithPreferredControlIfDefined(prop, pkCtrl);
                    controlMap.Add(pkCtrl);
                    continue;
                }

                var ctrl = CreateControlInfo(prop);
                if (!string.IsNullOrWhiteSpace(prop.ComplexTypeName))
                {
                    ctrl.IsComplexType = true;
                }
                controlMap.Add(ctrl);
            }

            foreach (var rel in entity.Relations)
            {
                if (!IsPropertyEditable(rel))
                {
                    var pkCtrl = new ControlInfo(rel.PropertyName, rel.PropertyName) { ControlType = ControlType.ReadOnly };
                    ReplaceWithPreferredControlIfDefined(rel, pkCtrl);
                    controlMap.Add(pkCtrl);
                    continue;
                }

                var ctrl = new ControlInfo(rel.PropertyName, rel.PropertyName)
                {
                    ControlType = ControlType.DropDown,
                    ReferenceEntityName = rel.ReferenceEntityName,
                    DbType = rel.DbType,
                    IsComplexType = true,
                };

                ReplaceWithPreferredControlIfDefined(rel, ctrl);
                controlMap.Add(ctrl);
            }

            return controlMap;
        }

        ControlInfo CreateControlInfo(Property prop)
        {
            ControlInfo ctrl = null;
            switch (prop.DbType)
            {
                //case DbType.Binary:
                //    break;
                //case DbType.Byte:
                //    break;
                case DbType.Boolean:
                    ctrl = new ControlInfo(prop.PropertyName, prop.PropertyName) { ControlType = ControlType.CheckBox };
                    break;
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    ctrl = new ControlInfo(prop.PropertyName, prop.PropertyName) { ControlType = ControlType.DatePicker };
                    break;
                case DbType.VarNumeric:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Single:
                case DbType.Currency:
                    ctrl = new NumericControlInfo(prop.PropertyName, prop.PropertyName) { ControlType = ControlType.NumericTextBox };
                    break;
                case DbType.Guid:
                    ctrl = new ControlInfo(prop.PropertyName, prop.PropertyName) { ControlType = ControlType.TextBox };
                    break;
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    ctrl = new NumericControlInfo(prop.PropertyName, prop.PropertyName) { ControlType = ControlType.TickUpDown };
                    SetMinAndMaxIfExist(prop, ctrl);
                    break;
                //case DbType.Object:
                //    break;
                //case DbType.SByte:
                //    break;
                case DbType.AnsiString:
                case DbType.String:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    ctrl = new ControlInfo(prop.PropertyName, prop.PropertyName) { ControlType = ControlType.TextBox };
                    break;
                case DbType.Time:
                    ctrl = new ControlInfo(prop.PropertyName, prop.PropertyName) { ControlType = ControlType.TimePicker };
                    break;

                case DbType.Xml:
                    ctrl = new ControlInfo(prop.PropertyName, prop.PropertyName) { ControlType = ControlType.TextArea };
                    break;
                default:
                    ctrl = new ControlInfo(prop.PropertyName, prop.PropertyName) { ControlType = ControlType.TextBox };
                    break;
            }

            ReplaceWithPreferredControlIfDefined(prop, ctrl);
            SetMaxLengthIfExist(prop, ctrl);

            ctrl.DbType = prop.DbType;
            return ctrl;
        }

        static void SetMaxLengthIfExist(Property prop, ControlInfo ctrl)
        {
            if (prop.Length > 0)
            {
                ctrl.MaxLength = prop.Length;
                if(prop.Length>499)
                    ctrl.ControlType = ControlType.TextArea;
            }
        }

        static void SetMinAndMaxIfExist(Property prop, ControlInfo ctrl)
        {
            var numCtrl = ctrl as NumericControlInfo;
            if (numCtrl == null) return;

            string maxText;
            if (prop.MetaDataAttributes.TryGetValue("max", out maxText))
            {
                int max;
                int.TryParse(maxText, out max);
                numCtrl.Max = max;
            }

            string minText;
            if (prop.MetaDataAttributes.TryGetValue("min", out minText))
            {
                int min;
                int.TryParse(minText, out min);
                numCtrl.Min = min;
            }
        }

        static void ReplaceWithPreferredControlIfDefined(Property prop, ControlInfo ctrl)
        {
            string pref;
            if (prop.MetaDataAttributes.TryGetValue("display_prefctrl", out pref))
            {
                switch (pref)
                {
                    case "checkbox":
                        ctrl.ControlType = ControlType.CheckBox;
                        break;
                    case "radio":
                        ctrl.ControlType = ControlType.RadioButtonList;
                        break;
                    case "text":
                        ctrl.ControlType = ControlType.TextBox;
                        break;
                    case "readonly":
                        ctrl.ControlType = ControlType.ReadOnly;
                        break;
                    case "numeric":
                        ctrl.ControlType = ControlType.NumericTextBox;
                        break;
                    case "editor":
                        ctrl.ControlType = ControlType.TextEditor;
                        break;
                    case "dropdown":
                        ctrl.ControlType = ControlType.DropDown;
                        break;
                    case "date":
                        ctrl.ControlType = ControlType.DatePicker;
                        break;
                    case "time":
                        ctrl.ControlType = ControlType.TimePicker;
                        break;
                    case "textarea":
                        ctrl.ControlType = ControlType.TextArea;
                        break;
                    case "label":
                        ctrl.ControlType = ControlType.Label;
                        break;
                    case "color":
                        ctrl.ControlType = ControlType.ColorPicker;
                        break;
                    case "hidden":
                        ctrl.ControlType = ControlType.Hidden;
                        break;
                    default: throw new Exception(string.Format("Control {0} is not supported", pref));
                }
            }
        }

        static bool IsPropertyEditable(Property prop)
        {
            string editableText;
            if (prop.MetaDataAttributes.TryGetValue("editable", out editableText))
            {
                bool editable;
                bool.TryParse(editableText, out editable);
                return editable;
            }

            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.CodeGenerator.ViewBuilder;
using Goliath.Data.Mapping;
using Mono.Cecil;

namespace Goliath.Data.CodeGenerator
{
    public static class ViewProcessor
    {
        /// <summary>
        /// Builds the view info.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="excludes">The excludes.</param>
        /// <returns></returns>
        public static IDictionary<string, EntityViewInfo> BuildViewInfo(this MapConfig config, string resource, params string[] excludes)
        {
            bool checkExcludes = true;
            if ((excludes == null) || (excludes.Length == 0))
            {
                checkExcludes = false;
                excludes = new string[] { };
            }

            IDictionary<string, EntityViewInfo> views = new Dictionary<string, EntityViewInfo>();
            foreach (var entMap in config.EntityConfigs)
            {
                if (checkExcludes && excludes.Contains(entMap.FullName))
                    continue;

                var entView = entMap.BuildViewInfo(resource);
                views.Add(entMap.FullName, entView);
            }

            return views;
        }

        /// <summary>
        /// Builds the view info.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="classNames">The class names.</param>
        /// <returns></returns>
        public static IDictionary<string, EntityViewInfo> BuildViewInfo(string assemblyPath, string[] classNames)
        {
            IDictionary<string, EntityViewInfo> views = new Dictionary<string, EntityViewInfo>();
            if ((classNames == null) || (classNames.Length == 0))
                return views;

            AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);
            foreach (var typeDef in assemblyDefinition.MainModule.Types)
            {
                if (!classNames.Contains(typeDef.FullName))
                    continue;

                views.Add(typeDef.FullName, BuildViewFromReflection(typeDef));
            }


            return views;
        }

        static EntityViewInfo BuildViewFromReflection(TypeDefinition typedefinition)
        {
            var viewInfo = new EntityViewInfo()
            {
                EntityName = typedefinition.FullName,
                LabelResourceName = PrintHelper.PrintClassResourceName(typedefinition.FullName, ResourceItemType.Label),
                DescriptionResourceName = PrintHelper.PrintClassResourceName(typedefinition.FullName, ResourceItemType.Description)
            };

            foreach (var customAttr in typedefinition.CustomAttributes)
            {
                Console.WriteLine(customAttr.AttributeType);
                Console.WriteLine(customAttr.Constructor.DeclaringType);
            }

            foreach (var propDef in typedefinition.Properties)
            {
                PropertyViewInfo pview = new PropertyViewInfo();
                string groupName = "default";
                string resourceType = string.Empty;
                pview.Name = propDef.Name;
                bool attributeFound = false;
                foreach (var attr in propDef.CustomAttributes)
                {
                    if (attr.AttributeType.Name.Equals("DisplayAttribute"))
                    {
                        attributeFound = true;
                        foreach (var attProp in attr.Properties)
                        {
                            if (attProp.Name.Equals("Name") && (attProp.Argument.Value != null))
                            {
                                pview.LabelResourceName = attProp.Argument.Value.ToString();
                                continue;
                            }

                            if (attProp.Name.Equals("Description") && (attProp.Argument.Value != null))
                            {
                                pview.DescriptionResourceName = attProp.Argument.Value.ToString();
                                continue;
                            }

                            if (attProp.Name.Equals("ResourceType") && (attProp.Argument.Value != null))
                            {
                                pview.ResourceType = attProp.Argument.Value.ToString();
                                resourceType = pview.ResourceType;
                                continue;
                            }

                            if (attProp.Name.Equals("Prompt") && (attProp.Argument.Value != null))
                            {
                                pview.PromptResourceName = attProp.Argument.Value.ToString();
                                continue;
                            }

                            if (attProp.Name.Equals("GroupName") && (attProp.Argument.Value != null))
                            {
                                groupName = attProp.Argument.Value.ToString();
                                continue;
                            }

                            if (attProp.Name.Equals("Order") && (attProp.Argument.Value != null))
                            {
                                var orderString = attProp.Argument.Value.ToString();
                                int order;
                                if (int.TryParse(orderString, out order))
                                    pview.Order = order;
                            }
                        }
                    }

                    if (attr.AttributeType.Name.Equals("EditableAttribute"))
                    {
                        if (attr.HasConstructorArguments && attr.ConstructorArguments[0].Value != null)
                        {
                            attributeFound = true;
                            var editableValue = attr.ConstructorArguments[0].Value.ToString();

                            bool isEditable;
                            if (bool.TryParse(editableValue, out isEditable))
                                pview.Editable = isEditable;
                            else
                                pview.Editable = true;
                        }
                    }

                    if (attr.AttributeType.Name.Equals("RequiredAttribute"))
                    {
                        foreach (var attProp in attr.Properties)
                        {
                            if (attProp.Name.Equals("ErrorMessageResourceName") && (attProp.Argument.Value != null))
                            {
                                attributeFound = true;
                                pview.RequiredErrorResourceName = attProp.Argument.Value.ToString();
                            }
                        }
                    }
                }

                if (!attributeFound) continue;

                PropertyGroupViewInfo propCollection;
                if (!viewInfo.TryGetValue(groupName, out propCollection))
                {
                    string groupLabel;
                    if ("default".Equals(groupName))
                        groupLabel = PrintHelper.PrintPropertyGroupResourceName(groupName, ResourceItemType.Label);

                    else groupLabel = groupName;

                    propCollection = new PropertyGroupViewInfo()
                    {
                        Name = groupName,
                        LabelResourceName = groupLabel,
                        ResourceType = resourceType,
                        DescriptionResourceName = PrintHelper.PrintPropertyGroupResourceName(groupName, ResourceItemType.Description)
                    };
                    viewInfo.Add(propCollection);
                }

                propCollection.Add(pview);
            }

            return viewInfo;
        }

        /// <summary>
        /// Builds the view info.
        /// </summary>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        public static EntityViewInfo BuildViewInfo(this EntityMap entityMap, string resource)
        {
            var viewInfo = new EntityViewInfo()
            {
                EntityName = entityMap.FullName,
                ResourceType = resource,
                LabelResourceName = entityMap.PrintResourceName(ResourceItemType.Label),
                DescriptionResourceName = entityMap.PrintResourceName(ResourceItemType.Description)
            };


            foreach (var prop in entityMap)
            {
                var rel = prop as Relation;
                if (rel != null)
                {
                    if (rel.RelationType != RelationshipType.ManyToOne)
                        continue;
                }

                string groupName;
                if (!prop.TryGetAttribute("display_groupname", out groupName))
                {
                    groupName = "default";
                }

                PropertyGroupViewInfo propCollection;
                if (!viewInfo.TryGetValue(groupName, out propCollection))
                {
                    string groupLabel;
                    if ("default".Equals(groupName))
                        groupLabel = PrintHelper.PrintPropertyGroupResourceName(groupName, ResourceItemType.Label);

                    else groupLabel = groupName;

                    propCollection = new PropertyGroupViewInfo()
                    {
                        Name = groupName,
                        LabelResourceName = groupLabel,
                        ResourceType = resource,
                        DescriptionResourceName = PrintHelper.PrintPropertyGroupResourceName(groupName, ResourceItemType.Description)
                    };
                    viewInfo.Add(propCollection);
                }

                if (propCollection.Contains(prop.Name))
                    continue;

                var propViewInfo = new PropertyViewInfo()
                {
                    Name = prop.Name,
                    ResourceType = resource,
                    Editable = true,
                    LabelResourceName = prop.PrintResourceName(entityMap, ResourceItemType.Label),
                    DescriptionResourceName = prop.PrintResourceName(entityMap, ResourceItemType.Description),
                };
                string attr;
                if (prop.TryGetAttribute("display_prompt", out attr))
                {
                    propViewInfo.PromptResourceName = prop.PrintResourceName(entityMap, ResourceItemType.Prompt);
                }

                if (prop.TryGetAttribute("required", out attr))
                {
                    propViewInfo.PromptResourceName = prop.PrintResourceName(entityMap, ResourceItemType.ErrorWhenMissing);
                }

                if (prop.TryGetAttribute("display_order", out attr))
                {
                    int order;
                    if (int.TryParse(attr, out order))
                        propViewInfo.Order = order;
                }

                if (prop.TryGetAttribute("editable", out attr))
                {
                    bool editable;
                    if (bool.TryParse(attr, out editable))
                        propViewInfo.Editable = editable;
                }

                propCollection.Add(propViewInfo);

            }

            return viewInfo;
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Transformers;
using Goliath.Data.Providers;

namespace Goliath.Data.Mapping
{
    public class DataModelGenerator
    {
        ISchemaDescriptor schemaDescriptor;
        NameTransformerFactory transfactory;
        ITableNameAbbreviator tableAbbreviator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataModelGenerator"/> class.
        /// </summary>
        /// <param name="schemaDescriptor">The schema descriptor.</param>
        public DataModelGenerator(ISchemaDescriptor schemaDescriptor, NameTransformerFactory transformerFactory, ITableNameAbbreviator tableAbbreviator)
        {
            this.schemaDescriptor = schemaDescriptor;
            transfactory = transformerFactory;
            this.tableAbbreviator = tableAbbreviator;
        }

        /// <summary>
        /// Builds the map.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public MapConfig GenerateMap(ProjectSettings settings, params ComplexType[] additionalTypes)
        {
            schemaDescriptor.ProjectSettings = settings;
            var tables = schemaDescriptor.GetTables();

            IPostGenerationProcessor nameProcessor = new NamePostProcessor(transfactory, tableAbbreviator);
            IPostGenerationProcessor relationshipProcessor = new RelationshipProcessor();

            nameProcessor.Process(tables);
            relationshipProcessor.Process(tables);

            MapConfig builder = new MapConfig();
            builder.Settings = settings;
            builder.GeneratedBy = schemaDescriptor.DatabaseProviderName;
            builder.EntityConfigs.AddRange(tables.Values);

            ComplexType baseModel = null;
            if ((additionalTypes != null) && (additionalTypes.Length > 0))
            {
                builder.ComplexTypes.AddRange(additionalTypes);
                if (!string.IsNullOrWhiteSpace(settings.BaseModel) && builder.ComplexTypes.Contains(settings.BaseModel))
                {
                    baseModel = builder.ComplexTypes[settings.BaseModel];
                }
            }

            foreach (var ent in builder.EntityConfigs)
            {
                ent.Parent = builder;
                if (string.IsNullOrWhiteSpace(ent.Extends) && (baseModel != null))
                {
                    ent.Extends = baseModel.FullName;
                }
                MapPrimaryKey(ent);
            }

            Console.WriteLine("we found {0} tables", tables.Count);

            return builder;
        }
        
        void MapPrimaryKey(EntityMap entMap)
        {
            var pks = entMap.Properties.Where(p => p.IsPrimaryKey).ToList();
            var pkr = entMap.Relations.Where(r => r.IsPrimaryKey).ToList();

            List<PrimaryKeyProperty> keys = new List<PrimaryKeyProperty>();
            foreach (var p in pks)
            {
                keys.Add(p);
                entMap.Properties.Remove(p.Name);
            }
            foreach (var p in pkr)
            {
                keys.Add(p);
                entMap.Relations.Remove(p.Name);
            }

            if (keys.Count > 0)
            {
                entMap.PrimaryKey = new PrimaryKey(keys.ToArray());
            }
        }

    }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.CodeGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public class CodeGenRunner : ICodeGenRunner
    {
        protected SupportedRdbms rdbms;
        protected IGenerator codeGen;

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public ProjectSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the script folder.
        /// </summary>
        /// <value>
        /// The script folder.
        /// </value>
        public string ScriptFolder { get; set; }

        /// <summary>
        /// Gets or sets the template folder.
        /// </summary>
        /// <value>
        /// The template folder.
        /// </value>
        public string TemplateFolder { get; set; }

        /// <summary>
        /// Gets or sets the working folder.
        /// </summary>
        /// <value>
        /// The working folder.
        /// </value>
        public string WorkingFolder { get; set; }

        /// <summary>
        /// Gets or sets the database folder.
        /// </summary>
        /// <value>
        /// The database folder.
        /// </value>
        public string DatabaseFolder { get; set; }

        /// <summary>
        /// Gets or sets the name of the query provider.
        /// </summary>
        /// <value>
        /// The name of the query provider.
        /// </value>
        public string QueryProviderName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenRunner"/> class.
        /// </summary>
        /// <param name="rdbms">The RDBMS.</param>
        /// <param name="codeGen">The code gen.</param>
        public CodeGenRunner(SupportedRdbms rdbms, IGenerator codeGen)
        {
            this.rdbms = rdbms;
            this.codeGen = codeGen;
            Settings = new ProjectSettings();
        }

        /// <summary>
        /// Creates the map.
        /// </summary>
        /// <param name="schemaDescriptor">The schema descriptor.</param>
        /// <param name="baseModel">The base model.</param>
        /// <param name="mapFilename">The map filename.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// schemaDescriptor
        /// or
        /// mapFilename
        /// </exception>
        public MapConfig CreateMap(ISchemaDescriptor schemaDescriptor, ComplexType baseModel, string mapFilename)
        {
            if (schemaDescriptor == null) throw new ArgumentNullException(nameof(schemaDescriptor));
            if (string.IsNullOrWhiteSpace(mapFilename)) throw new ArgumentNullException(nameof(mapFilename));

            var map = codeGen.GenerateMapping(WorkingFolder, schemaDescriptor, Settings, baseModel, rdbms, mapFilename);
            map.MapStatements(QueryProviderName);
            return map;
        }

        public static void ProcessMappedStatements(MapConfig map)
        {
            foreach (var stat in map.MappedStatements)
            {
                if (string.IsNullOrWhiteSpace(stat.ResultMap))
                    stat.ResultMap = "int"; //default is non query

                if (!string.IsNullOrWhiteSpace(stat.DependsOnEntity)) continue;

                EntityMap ent;
                if (map.EntityConfigs.TryGetValue(stat.ResultMap, out ent))
                {
                    //Console.WriteLine("ResultMap {0} is an entity - set dependance", stat.ResultMap);
                    stat.DependsOnEntity = ent.FullName;
                    stat.Name = $"{ent.FullName}_{stat.Name}";
                }
                else if (stat.InputParametersMap.Count == 1)
                {
                    var param = stat.InputParametersMap.Values.First();
                    if (!map.EntityConfigs.TryGetValue(param, out ent)) continue;

                    stat.DependsOnEntity = ent.FullName;
                    stat.Name = $"{ent.FullName}_{stat.Name}";
                }
            }
        }

        /// <summary>
        /// Creates the map.
        /// </summary>
        /// <param name="schemaDescriptor">The schema descriptor.</param>
        /// <param name="entityRenames">The entity renames.</param>
        /// <param name="baseModel">The base model.</param>
        /// <param name="mapFilename">The map filename.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// schemaDescriptor
        /// or
        /// mapFilename
        /// </exception>
        public MapConfig CreateMap(ISchemaDescriptor schemaDescriptor, IDictionary<string, string> entityRenames, ComplexType baseModel, string mapFilename)
        {
            if (schemaDescriptor == null) throw new ArgumentNullException(nameof(schemaDescriptor));
            if (string.IsNullOrWhiteSpace(mapFilename)) throw new ArgumentNullException(nameof(mapFilename));

            var map = codeGen.GenerateMapping(WorkingFolder, schemaDescriptor, entityRenames, Settings, baseModel, rdbms, mapFilename);
            map.MapStatements(QueryProviderName);
            return map;
        }

        /// <summary>
        /// Generates the classes.
        /// </summary>
        /// <param name="mapFile">The map file.</param>
        /// <param name="excludedEntities">The excluded entities.</param>
        public void GenerateClasses(string mapFile, params string[] excludedEntities)
        {
            var templateFile = Path.Combine(TemplateFolder, "TrackableClass.razt");
            codeGen.GenerateCodeForEachEntityMap(templateFile, WorkingFolder, mapFile, (name,iteration) => name + ".cs", excludedEntities);
        }

        /// <summary>
        /// Generates the classes.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="excludedEntities">The excluded entities.</param>
        public void GenerateClasses(MapConfig config, params string[] excludedEntities)
        {
            var templateFile = Path.Combine(TemplateFolder, "TrackableClass.razt");
            codeGen.GenerateCodeForEachEntityMap(templateFile, WorkingFolder, config, (name,iteration) => name + ".cs", excludedEntities);
        }

        /// <summary>
        /// Generates the code from template.
        /// </summary>
        /// <param name="mapFile">The map file.</param>
        /// <param name="templateFile">The template file.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="fileNameFunction">The file name function.</param>
        /// <param name="excludedEntities">The excluded entities.</param>
        public void GenerateClassesFromTemplate(string mapFile, string templateFile, string workingFolder, Func<string, int?, string> fileNameFunction = null, params string[] excludedEntities)
        {
            codeGen.GenerateCodeForEachEntityMap(templateFile, workingFolder, mapFile, fileNameFunction, excludedEntities);
        }

        /// <summary>
        /// Generates the code from template.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="templateFile">The template file.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="fileNameFunction">The file name function.</param>
        /// <param name="excludedEntities">The excluded entities.</param>
        public void GenerateClassesFromTemplate(MapConfig config, string templateFile, string workingFolder, Func<string, int?, string> fileNameFunction = null, params string[] excludedEntities)
        {
            codeGen.GenerateCodeForEachEntityMap(templateFile, workingFolder, config, fileNameFunction, excludedEntities);
        }

        /// <summary>
        /// Generates the code from template.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="templateFile">The template file.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="fileName">Name of the file.</param>
        public void GenerateCodeFromTemplate<T>(T model, string templateFile, string workingFolder, string fileName)
        {
            codeGen.GenerateCodeFromModel(model, templateFile, workingFolder, fileName);
        }
    }
}

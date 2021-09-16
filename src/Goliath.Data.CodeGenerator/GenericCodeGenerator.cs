using System.Collections.Generic;
using System.IO;
using Goliath.Data.Diagnostics;
using Goliath.Data.Generators;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Providers.SqlServer;
using Goliath.Data.Transformers;
using System;

namespace Goliath.Data.CodeGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public class GenericCodeGenerator : IGenerator
    {
        private readonly IInterpreter interpreter;
        static ILogger logger;

        static GenericCodeGenerator()
        {
            logger = Logger.GetLogger(typeof(GenericCodeGenerator));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericCodeGenerator" /> class.
        /// </summary>
        public GenericCodeGenerator() : this(new RazorInterpreter()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericCodeGenerator" /> class.
        /// </summary>
        /// <param name="interpreter">The interpreter.</param>
        public GenericCodeGenerator(IInterpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        #region ICodeGenerator Members


        /// <summary>
        /// Generates the code for each entity map.
        /// </summary>
        /// <param name="templateFile">The templateFile.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="mapfile">The mapfile.</param>
        /// <param name="fileNameFunction">The file name function.</param>
        /// <param name="excludedTables">The excluded tables.</param>
        /// <param name="properties"></param>
        public void GenerateCodeForEachEntityMap(string templateFile, string workingFolder, string mapfile,
            Func<string, int?, string> fileNameFunction = null, IDictionary<string, string> properties = null, params string[] excludedTables)
        {
            var project = MapConfig.Create(mapfile, true);
            GenerateCodeForEachEntityMap(templateFile, workingFolder, project, fileNameFunction, properties, excludedTables);
        }

        /// <summary>
        /// Generates the code for each entity map.
        /// </summary>
        /// <param name="templateFile">The templateFile.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="config">The config.</param>
        /// <param name="fileNameFunction">The file name function.</param>
        /// <param name="excludedTables">The excluded tables.</param>
        /// <param name="properties"></param>
        public void GenerateCodeForEachEntityMap(string templateFile, string workingFolder, MapConfig config,
            Func<string, int?, string> fileNameFunction = null, IDictionary<string, string> properties = null, params string[] excludedTables)
        {
            var counter = 0;
            foreach (var table in config.EntityConfigs)
            {
                if (table.IsLinkTable || SchemaDescriptor.IsExcludedEntity(excludedTables, table.Name))
                    continue;
                try
                {
                    var name = table.Name + ".txt";
                    counter++;

                    if (fileNameFunction != null)
                    {
                        name = fileNameFunction(table.Name, counter);
                    }

                    var fname = Path.Combine(workingFolder, name);
                    logger.Log(LogLevel.Debug, $"Running Code gen for entity {table}");
                    interpreter.Generate(templateFile, fname, table, properties);
                    logger.Log(LogLevel.Info, $"File {fname} generate from template {templateFile} for entity {table}");
                }
                catch (Exception ex)
                {
                    logger.LogException("Couldn't generate code file for " + table.Name, ex);
                }
            }
        }

        /// <summary>
        /// Generates the code from model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="templateFile">The templateFile.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="properties"></param>
        public void GenerateCodeFromModel<T>(T model, string templateFile, string workingFolder, string filename, IDictionary<string, string> properties = null)
        {
            interpreter.Generate(templateFile, Path.Combine(workingFolder, filename), model, properties);
        }

        /// <summary>
        /// Generates the mapping.
        /// </summary>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="schemaDescriptor">The schema descriptor.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="baseModel">The base model.</param>
        /// <param name="rdbms">The RDBMS.</param>
        /// <param name="mapFileName">Name of the map file.</param>
        /// <returns></returns>
        public MapConfig GenerateMapping(string workingFolder, ISchemaDescriptor schemaDescriptor, ProjectSettings settings, ComplexType baseModel, SupportedRdbms rdbms, string mapFileName)
        {
            return Build(workingFolder, settings, baseModel, schemaDescriptor, mapFileName);
        }

        /// <summary>
        /// Generates the mapping.
        /// </summary>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="schemaDescriptor">The schema descriptor.</param>
        /// <param name="entityRenames">The entity renames.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="baseModel">The base model.</param>
        /// <param name="rdbms">The RDBMS.</param>
        /// <param name="mapFileName">Name of the map file.</param>
        /// <returns></returns>
        public MapConfig GenerateMapping(string workingFolder, ISchemaDescriptor schemaDescriptor, IDictionary<string, string> entityRenames, ProjectSettings settings, ComplexType baseModel, SupportedRdbms rdbms, string mapFileName)
        {
            return Build(workingFolder, settings, baseModel, schemaDescriptor, entityRenames, mapFileName);
        }

        #endregion

        /// <summary>
        /// Builds the specified working folder.
        /// </summary>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="baseModel">The base model.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="mapFileName">Name of the map file.</param>
        /// <returns></returns>
        protected MapConfig Build(string workingFolder,
            ProjectSettings settings,
            ComplexType baseModel,
            ISchemaDescriptor schema,
            string mapFileName)
        {
            return Build(workingFolder, settings, baseModel, schema, new Dictionary<string, string>(), mapFileName);
        }

        protected MapConfig Build(string workingFolder,
            ProjectSettings settings,
            ComplexType baseModel,
            ISchemaDescriptor schema,
            IDictionary<string, string> entityRenames,
            string mapFileName)
        {
            var rwords = Mssq2008Dialect.SqlServerReservedWords.Split(new string[] { " ", "\n", "," }, StringSplitOptions.RemoveEmptyEntries);
            schema.ProjectSettings = settings;
            var generator = new DataModelGenerator(schema, new NameTransformerFactory(settings),
                new DefaultTableNameAbbreviator(rwords));

            MapConfig builder;
            if (baseModel != null)
                builder = generator.GenerateMap(settings, entityRenames, baseModel);
            else
                builder = generator.GenerateMap(settings, entityRenames);

            CreateFolderIfNotExist(workingFolder);

            string mapfile;
            if (!Path.IsPathRooted(mapFileName))
                mapfile = Path.Combine(workingFolder, mapFileName);
            else
                mapfile = mapFileName;

            builder.Save(mapfile, true);
            return builder;
        }

        static void CreateFolderIfNotExist(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

    }
}

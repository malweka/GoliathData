using System.IO;
using Goliath.Data.Generators;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericCodeGenerator" /> class.
        /// </summary>
        public GenericCodeGenerator():this(new RazorInterpreter()){ }

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
        /// <param name="templatefile">The templatefile.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="mapfile">The mapfile.</param>
        /// <param name="fileNameFunction">The file name function.</param>
        public void GenerateCodeForEachEntityMap(string templatefile, string workingFolder, string mapfile, Func<string,string> fileNameFunction = null)
        {            
            var project = MapConfig.Create(mapfile, true);
            GenerateCodeForEachEntityMap(templatefile,workingFolder,project,fileNameFunction);
        }

        /// <summary>
        /// Generates the code for each entity map.
        /// </summary>
        /// <param name="templatefile">The templatefile.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="config">The config.</param>
        /// <param name="fileNameFunction">The file name function.</param>
        public void GenerateCodeForEachEntityMap(string templatefile, string workingFolder, MapConfig config, Func<string, string> fileNameFunction = null)
        {
            foreach (var table in config.EntityConfigs)
            {
                if (table.IsLinkTable)
                    continue;

                var name = table.Name + ".txt";

                if (fileNameFunction != null)
                {
                    name = fileNameFunction(table.Name);
                }

                interpreter.Generate(templatefile, Path.Combine(workingFolder, name), table);
            }
        }

        /// <summary>
        /// Generates the code from model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="templatefile">The templatefile.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="filename">The filename.</param>
       public void GenerateCodeFromModel<T>(T model, string templatefile, string workingFolder,  string filename)
       {
           interpreter.Generate(templatefile, Path.Combine(workingFolder, filename), model);
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
        protected MapConfig Build(string workingFolder, ProjectSettings settings, ComplexType baseModel, ISchemaDescriptor schema, string mapFileName)
        {
            schema.ProjectSettings = settings;
            var generator = new DataModelGenerator(schema, new NameTransformerFactory(settings),
                new DefaultTableNameAbbreviator());

            MapConfig builder;
            if (baseModel != null)
                builder = generator.GenerateMap(settings, baseModel);
            else
                builder = generator.GenerateMap(settings);

            CreateFolderIfNotExist(workingFolder);

            var mapfile = Path.Combine(workingFolder, mapFileName);
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

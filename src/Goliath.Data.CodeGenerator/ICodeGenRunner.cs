using System;
using System.Collections.Generic;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.CodeGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICodeGenRunner
    {
        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        ProjectSettings Settings { get; set; }
        /// <summary>
        /// Gets or sets the script folder.
        /// </summary>
        /// <value>
        /// The script folder.
        /// </value>
        string ScriptFolder { get; set; }
        /// <summary>
        /// Gets or sets the template folder.
        /// </summary>
        /// <value>
        /// The template folder.
        /// </value>
        string TemplateFolder { get; set; }
        /// <summary>
        /// Gets or sets the working folder.
        /// </summary>
        /// <value>
        /// The working folder.
        /// </value>
        string WorkingFolder { get; set; }
        /// <summary>
        /// Gets or sets the database folder.
        /// </summary>
        /// <value>
        /// The database folder.
        /// </value>
        string DatabaseFolder { get; set; }

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
        MapConfig CreateMap(ISchemaDescriptor schemaDescriptor, ComplexType baseModel, string mapFilename);

        /// <summary>
        /// Generates the classes.
        /// </summary>
        /// <param name="mapFilename">The map filename.</param>
        /// <param name="excludedEntities">The excluded entities.</param>
        /// <param name="properties"></param>
        void GenerateClasses(string mapFilename, IDictionary<string, string> properties = null, params string[] excludedEntities);

        /// <summary>
        /// Generates the classes.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="excludedEntities">The excluded entities.</param>
        /// <param name="properties"></param>
        void GenerateClasses(MapConfig config, IDictionary<string, string> properties = null, params string[] excludedEntities);

        /// <summary>
        /// Generates the code from template.
        /// </summary>
        /// <param name="mapFile">The map file.</param>
        /// <param name="templateFile">The template file.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="fileNameFunction">The file name function.</param>
        /// <param name="excludedEntities">The excluded entities.</param>
        /// <param name="properties"></param>
        void GenerateClassesFromTemplate(string mapFile, string templateFile, string workingFolder, 
            Func<string,int?, string> fileNameFunction = null, IDictionary<string, string> properties = null, params string[] excludedEntities);

        /// <summary>
        /// Generates the code from template.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="templateFile">The template file.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="fileNameFunction">The file name function.</param>
        /// <param name="excludedEntities">The excluded entities.</param>
        /// <param name="properties"></param>
        void GenerateClassesFromTemplate(MapConfig config, string templateFile, string workingFolder, 
            Func<string, int?, string> fileNameFunction = null, IDictionary<string, string> properties = null, params string[] excludedEntities);

        /// <summary>
        /// Generates the code from template.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="templateFile">The template file.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="properties"></param>
        void GenerateCodeFromTemplate<T>(T model, string templateFile, string workingFolder, string fileName, IDictionary<string, string> properties = null);
    }
}
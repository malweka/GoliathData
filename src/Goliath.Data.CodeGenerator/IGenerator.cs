using System;
using System.Collections.Generic;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.CodeGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGenerator
    {

        /// <summary>
        /// Generates the code for each entity map.
        /// </summary>
        /// <param name="templateFile">The templateFile.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="mapFile">The map file.</param>
        /// <param name="fileNameFunction">The file name function.</param>
        /// <param name="excludedTables">The excluded tables.</param>
        /// <param name="properties"></param>
        void GenerateCodeForEachEntityMap(string templateFile, string workingFolder, string mapFile,
            Func<string, int?, string> fileNameFunction = null, IDictionary<string, string> properties = null, params string[] excludedTables);

        /// <summary>
        /// Generates the code for each entity map.
        /// </summary>
        /// <param name="templateFile">The templateFile.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="config">The config.</param>
        /// <param name="fileNameFunction">The file name function.</param>
        /// <param name="excludedTables">The excluded tables.</param>
        /// <param name="properties"></param>
        void GenerateCodeForEachEntityMap(string templateFile, string workingFolder, MapConfig config,
            Func<string, int?, string> fileNameFunction = null, IDictionary<string, string> properties = null, params string[] excludedTables);

        /// <summary>
        /// Generates the code from model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model.</param>
        /// <param name="templateFile">The templateFile.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="properties"></param>
        void GenerateCodeFromModel<T>(T model, string templateFile, string workingFolder, string filename, IDictionary<string, string> properties = null);

        MapConfig GenerateMapping(string workingFolder, ISchemaDescriptor schemaDescriptor, ProjectSettings settings, ComplexType baseModel, SupportedRdbms rdbms, string mapFileName);

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
        MapConfig GenerateMapping(string workingFolder, ISchemaDescriptor schemaDescriptor, IDictionary<string, string> entityRenames,
            ProjectSettings settings, ComplexType baseModel, SupportedRdbms rdbms, string mapFileName);
    }
}
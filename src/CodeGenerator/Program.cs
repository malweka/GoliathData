﻿using System;
using System.Collections.Generic;
using System.IO;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;


namespace Goliath.Data.CodeGenerator
{
    class Program
    {
        static readonly ILogger logger;

        static Program()
        {
            logger = Logger.GetLogger(typeof(Program));
        }

        static void Main(string[] args)
        {

#if DEBUG
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
#endif
            //Console.WriteLine(1.ToString("D3"));
            //Console.WriteLine(19.ToString("D3"));

            var opts = AppOptionHandler.ParseOptions(args);

            Console.WriteLine("Starting application. Generated files will be saved on Folder: {0} ", opts.WorkingFolder);
            Console.WriteLine("Template Folder: {0} \n", opts.TemplateFolder);

            //can we load sqlite
            Console.WriteLine("Loading sqlite provider");
            var sqlite = new Goliath.Data.Providers.Sqlite.SqliteDialect();
            Console.WriteLine("Loading postgresql provider");
            var postgres = new Goliath.Data.Providers.Postgres.PostgresDialect();

            SupportedRdbms rdbms;

            if (!string.IsNullOrWhiteSpace(opts.ProviderName))
            {
                switch (opts.ProviderName.ToUpper())
                {
                    case "MSSQL2008":
                        rdbms = SupportedRdbms.Mssql2008;
                        break;
                    case "MSSQL2008R2":
                        rdbms = SupportedRdbms.Mssql2008R2;
                        break;
                    case "POSTGRESQL8":
                        rdbms = SupportedRdbms.Postgresql8;
                        break;
                    case "POSTGRESQL9":
                        rdbms = SupportedRdbms.Postgresql9;
                        break;
                    case "SQLITE3":
                        rdbms = SupportedRdbms.Sqlite3;
                        break;
                    default:
                        rdbms = SupportedRdbms.Mssql2008;
                        break;
                }
            }

            else rdbms = SupportedRdbms.Mssql2008R2;

            if (string.IsNullOrWhiteSpace(opts.QueryProviderName))
                opts.QueryProviderName = SupportedRdbms.Mssql2008R2.ToString();

            Console.Write("Provider");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" {0} ", rdbms);
            Console.ResetColor();
            Console.Write("Activated.");
            Console.WriteLine("\n\nLoading settings...");

            var codeGenRunner = new CodeGenRunner(rdbms, new GenericCodeGenerator()) {TemplateFolder = opts.TemplateFolder, ScriptFolder = AppDomain.CurrentDomain.BaseDirectory, DatabaseFolder = AppDomain.CurrentDomain.BaseDirectory, WorkingFolder = opts.WorkingFolder, QueryProviderName = opts.QueryProviderName, Settings = {Namespace = opts.Namespace, AssemblyName = opts.AssemblyName, ConnectionString = opts.ConnectionString, Platform = rdbms.ToString()}};


            var action = opts.ActionName.ToUpper();
            switch (action)
            {
                case "GENERATEENTITIES":
                    try
                    {
                        GenerateEntities(opts, codeGenRunner);
                        logger.Log(LogLevel.Debug, string.Format("Entities generated in work folder {0}.", opts.WorkingFolder));
                    }
                    catch (Exception ex)
                    {
                        PrintError("Exception thrown while trying to generate entities.", ex);
                    }
                    break;
                case "GENERATEALL":
                    try
                    {
                        GenerateAllFromTemplate(opts, codeGenRunner);
                        logger.Log(LogLevel.Debug, string.Format("Generated all files based on templates in work folder {0}.", opts.WorkingFolder));
                    }
                    catch (Exception ex)
                    {
                        PrintError("Exception thrown while trying to generate all.", ex);
                    }
                    break;
                case "COMBINEMAPS":
                    try
                    {
                        CombineMaps(opts, codeGenRunner);
                        logger.Log(LogLevel.Debug, $"Merging map {opts.TemplateName} with {opts.MapFile}.");
                    }
                    catch (Exception ex)
                    {
                        PrintError("Exception thrown while trying to generate all.", ex);
                    }
                    break;
                case "GENERATE":
                    try
                    {
                        GenerateFromTemplate(opts, codeGenRunner);
                        logger.Log(LogLevel.Debug, $"File {opts.OutputFile} generated in folder {opts.WorkingFolder}.");
                    }
                    catch (Exception ex)
                    {
                        PrintError("Exception thrown while trying to generate from template.", ex);
                    }
                    break;
                case "EXPORT":
                    try
                    {
                        GenerateExportFromTemplate(opts, rdbms, codeGenRunner);
                        logger.Log(LogLevel.Debug, $"File {opts.OutputFile} generated in folder {opts.WorkingFolder}.");
                    }
                    catch (Exception ex)
                    {
                        PrintError("Exception thrown while trying to generate from template.", ex);
                    }
                    break;
                default:
                    try
                    {
                        CreateMap(opts, rdbms, codeGenRunner);
                        logger.Log(LogLevel.Debug, $"\n\nMap {opts.MapFile} in {opts.WorkingFolder} has been created.");
                    }
                    catch (Exception ex)
                    {
                        PrintError("Exception thrown while trying to create map.", ex);
                    }
                    break;
            }


            Console.WriteLine("\nDone!");
        }

        static string GetCodeMapFile(AppOptionInfo opts, bool throwIfNotExist = true)
        {
            string codeMapFile = opts.MapFile;

            if (!Path.IsPathRooted(codeMapFile))
            {
                codeMapFile = Path.GetFullPath(codeMapFile);
            }

            Console.WriteLine("Reading Code Map file...");
            if (!File.Exists(codeMapFile) && throwIfNotExist)
                throw new GoliathDataException($"Map file {codeMapFile} not found.");

            return codeMapFile;
        }

        static void CombineMaps(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            var codeMapFile = GetCodeMapFile(opts);
            var mainMap = MapConfig.Create(codeMapFile, true);

            string mapfile2 = opts.TemplateName;
            if (!File.Exists(mapfile2))
            {
                throw new GoliathDataException(string.Format("map file {0} does not exist", mapfile2));
            }

            var secondMap = MapConfig.Create(mapfile2, true);
            mainMap.MergeMap(secondMap);
            CodeGenRunner.ProcessMappedStatements(mainMap);
            mainMap.Save(codeMapFile, true);
        }

        static void GenerateFromTemplate(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            var codeMapFile = GetCodeMapFile(opts);
            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;

            if (string.IsNullOrWhiteSpace(opts.TemplateName))
                throw new GoliathDataException("Template file to use is required for generate operation. Please make sure that -in=\"Template_File_name.razt\" argument is passed in.");

            var template = Path.Combine(codeGenRunner.TemplateFolder, opts.TemplateName);

            if (!File.Exists(template))
                throw new GoliathDataException(string.Format("template file {0} not found.", template));

            if (string.IsNullOrWhiteSpace(opts.OutputFile))
                throw new GoliathDataException("Output file is required for generate operation. Please make sure that -out=\"YOUR_FILE.EXT\" argument is passed in.");

            if (!string.IsNullOrWhiteSpace(opts.EntityModel))
            {
                logger.Log(LogLevel.Debug, string.Format("Extracting model {0} from map entity models.", opts.EntityModel));

                EntityMap entMap;
                if (map.EntityConfigs.TryGetValue(opts.EntityModel, out entMap))
                {
                    codeGenRunner.GenerateCodeFromTemplate(entMap, template, codeGenRunner.WorkingFolder, opts.OutputFile);
                }

                //TODO: nice to have feature is to use reflection to load a type from an external assembly.
            }
            else
            {
                codeGenRunner.GenerateCodeFromTemplate(map, template, codeGenRunner.WorkingFolder, opts.OutputFile);
            }
        }


        static void GenerateExportFromTemplate(AppOptionInfo opts, SupportedRdbms rdbms, CodeGenRunner codeGenRunner)
        {
            var codeMapFile = GetCodeMapFile(opts);
            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;

            var providerFactory = new ProviderFactory();

            var dialect = providerFactory.CreateDialect(rdbms);
            var dbConnector = providerFactory.CreateDbConnector(rdbms, opts.ConnectionString);

            if (string.IsNullOrWhiteSpace(opts.TemplateName))
                throw new GoliathDataException("Template file to use is required for generate operation. Please make sure that -in=\"Template_File_name.razt\" argument is passed in.");

            var template = Path.Combine(codeGenRunner.TemplateFolder, opts.TemplateName);

            if (!File.Exists(template))
                throw new GoliathDataException(string.Format("template file {0} not found.", template));

            if (string.IsNullOrWhiteSpace(opts.OutputFile))
                throw new GoliathDataException("Output file is required for generate operation. Please make sure that -out=\"YOUR_FILE.EXT\" argument is passed in.");

            DataExporterAdapter exporter = new DataExporterAdapter(dialect, dbConnector, new TypeConverterStore());

            var counter =0;

            List<string> errors = new List<string>();
            foreach (var entityMap in map.EntityConfigs)
            {
                try
                {
                    counter++;
                    var data = exporter.Export(entityMap, opts.ExportIdentityColumn, opts.ExportDatabaseGeneratedColumns);
                    var fileName = GetFileName(entityMap.Name, counter, opts.OutputFile);

                    if (data == null || data.DataBag.Count == 0)
                    {
                        continue;
                    }

                    codeGenRunner.GenerateCodeFromTemplate(data, template, codeGenRunner.WorkingFolder, fileName);
                   
                }
                catch (Exception ex)
                {
                    errors.Add($"Error table [{entityMap.TableName}]: {ex.ToString()}");
                }

                
            }

            if (errors.Count > 0)
            {
                Console.WriteLine($"\n\nEncountered {errors.Count} Errors");
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var err in errors)
                {
                    Console.WriteLine(err);
                }
                
                Console.ResetColor();
               
            }


        }
        static void GenerateAllFromTemplate(AppOptionInfo opts, ICodeGenRunner codeGenRunner)
        {
            Console.WriteLine("\n\nGenerating for all entities...");

            var codeMapFile = GetCodeMapFile(opts);

            var template = Path.Combine(codeGenRunner.TemplateFolder, opts.TemplateName);

            if (!File.Exists(template))
                throw new GoliathDataException(string.Format("template file {0} not found.", template));

            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;

            codeGenRunner.GenerateClassesFromTemplate(map, template, codeGenRunner.WorkingFolder, (name, iteration) => GetFileName(name, iteration, opts.OutputFile), opts.ExcludedArray);

        }

        static string GetFileName(string entityName, int? iteration, string outputFile)
        {
            string fileName;
            if (outputFile.Contains("(name)") || outputFile.Contains("(iteration)"))
            {
                fileName = outputFile.Replace("(name)", entityName);
                if (iteration.HasValue)
                    fileName = fileName.Replace("(iteration)", $"{iteration.Value:D3}");
            }
            else
            {
                fileName = string.Concat(entityName, outputFile);
            }

            return fileName;
        }

        static void GenerateEntities(AppOptionInfo opts, ICodeGenRunner codeGenRunner)
        {
            Console.WriteLine("\n\nGenerating Entities...");

            var codeMapFile = GetCodeMapFile(opts);

            var template = Path.Combine(codeGenRunner.TemplateFolder, "TrackableClass.razt");

            if (!File.Exists(template))
                throw new GoliathDataException(string.Format("template file {0} not found.", template));

            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;

            codeGenRunner.GenerateClasses(map, opts.ExcludedArray);
        }

        //static void ProcessMapBeforeSave(MapConfig map, string mapFileName, AppOptionInfo opts)
        //{
        //    if (!string.IsNullOrWhiteSpace(opts.MappedStatementFile) && File.Exists(opts.MappedStatementFile))
        //    {
        //        Console.WriteLine("Load mapped statements from {0} into {1}", opts.MappedStatementFile, mapFileName);
        //        map.LoadMappedStatements(opts.MappedStatementFile);
        //        CodeGenRunner.ProcessMappedStatements(map);
        //    }

        //    foreach (var ent in map.EntityConfigs)
        //    {
        //        ProcessMetadata(opts, ent);
        //        ProcessActivatedProperties(opts, ent);

        //        //process keygen
        //        if (!string.IsNullOrWhiteSpace(opts.DefaultKeygen))
        //        {
        //            if (ent.PrimaryKey != null)
        //            {
        //                foreach (var k in ent.PrimaryKey.Keys)
        //                {
        //                    if (string.IsNullOrWhiteSpace(k.KeyGenerationStrategy))
        //                    {
        //                        k.KeyGenerationStrategy = opts.DefaultKeygen;
        //                    }
        //                }
        //            }
        //        }

        //        foreach (var prop in ent)
        //        {
        //            ProcessMetadata(opts, ent, prop);
        //            ProcessActivatedProperties(opts, ent, prop);

        //            if (!string.IsNullOrWhiteSpace(opts.ComplexTypeMap))
        //            {
        //                var key = string.Concat(ent.Name, ".", prop.Name);
        //                string complextType;
        //                if (opts.ComplexTypesTypeMap.TryGetValue(key, out complextType))
        //                {
        //                    prop.ComplexTypeName = complextType;
        //                    prop.IsComplexType = true;
        //                }
        //            }
        //        }

        //    }

        //    map.Save(mapFileName, true);
        //}
        private const string prima_orda_file = "prima_orda.log";
        static void CreateMap(AppOptionInfo opts, SupportedRdbms rdbms, CodeGenRunner codeGenRunner)
        {
            Console.WriteLine("\n\nCreate Map...");
            var providerFactory = new ProviderFactory();
            ComplexType baseModel = null;
            var mapFileName = Path.Combine(opts.WorkingFolder, opts.MapFile);

            if (!string.IsNullOrWhiteSpace(opts.BaseModelXml) && File.Exists(opts.BaseModelXml))
            {
                var baseMap = new MapConfig();
                Console.WriteLine("\n\nRead base model.");
                baseMap.Load(opts.BaseModelXml);
                if (baseMap.ComplexTypes.Count > 0)
                {
                    baseModel = baseMap.ComplexTypes[0];
                    codeGenRunner.Settings.BaseModel = baseModel.FullName;
                }
            }

            using (ISchemaDescriptor schemaDescriptor = providerFactory.CreateDbSchemaDescriptor(rdbms, codeGenRunner.Settings, opts.ExcludedArray))
            {
                var map = codeGenRunner.CreateMap(schemaDescriptor, opts.EntitiesToRename, baseModel, mapFileName);
                //Console.WriteLine("mapped statements: {0}", opts.MappedStatementFile);
                if (!string.IsNullOrWhiteSpace(opts.MappedStatementFile) && File.Exists(opts.MappedStatementFile))
                {
                    Console.WriteLine("Load mapped statements from {0} into {1}", opts.MappedStatementFile, mapFileName);
                    map.LoadMappedStatements(opts.MappedStatementFile);
                    CodeGenRunner.ProcessMappedStatements(map);
                }

                //let's try to read order from log if exists
                string prima_orda = Path.Combine(opts.WorkingFolder, prima_orda_file);
                var previousOrderDict = new Dictionary<string, Tuple<int, string>>();
                var orderCount = 0;
                if (File.Exists(prima_orda))
                {
                    using (var sr = new StreamReader(prima_orda))
                    {
                        var contentLog = sr.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(contentLog))
                        {
                            var split = contentLog.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                            for (var i = 0; i < split.Length; i++)
                            {
                                var tbName = split[i];
                                previousOrderDict.Add(tbName, Tuple.Create(i + 1, tbName));
                            }

                            orderCount = split.Length;
                        }
                    }
                }

                foreach (var ent in map.EntityConfigs)
                {
                    ProcessMetadata(opts, ent);
                    ProcessActivatedProperties(opts, ent);

                    Tuple<int, string> tbOrder;
                    if (previousOrderDict.TryGetValue(ent.FullName, out tbOrder))
                    {
                        ent.Order = tbOrder.Item1;
                    }
                    else
                    {
                        previousOrderDict.Add(ent.FullName, Tuple.Create(ent.Order, ent.FullName));
                        orderCount = orderCount + 1;
                        ent.Order = orderCount;
                    }

                    //process keygen
                    if (!string.IsNullOrWhiteSpace(opts.DefaultKeygen))
                    {
                        if (ent.PrimaryKey != null)
                        {
                            foreach (var k in ent.PrimaryKey.Keys)
                            {
                                if (string.IsNullOrWhiteSpace(k.KeyGenerationStrategy))
                                {
                                    k.KeyGenerationStrategy = opts.DefaultKeygen;
                                }
                            }
                        }
                    }

                    foreach (var prop in ent)
                    {
                        ProcessMetadata(opts, ent, prop);
                        ProcessActivatedProperties(opts, ent, prop);

                        if (!string.IsNullOrWhiteSpace(opts.ComplexTypeMap))
                        {
                            var key = string.Concat(ent.Name, ".", prop.Name);
                            string complextType;
                            if (opts.ComplexTypesTypeMap.TryGetValue(key, out complextType))
                            {
                                prop.ComplexTypeName = complextType;
                                prop.IsComplexType = true;
                            }
                        }
                    }

                }

                map.Save(mapFileName, true, true);
                using (var file = new StreamWriter(prima_orda))
                {
                    foreach (var tbOrder in previousOrderDict.Keys)
                    {
                        file.WriteLine(tbOrder);
                    }
                }
            }
        }

        static void ProcessMetadata(AppOptionInfo opts, EntityMap ent)
        {
            List<Tuple<string, string>> metadata;
            if (opts.MetadataDictionary.TryGetValue(ent.Name, out metadata))
            {
                foreach (var tuple in metadata)
                {
                    ent.MetaDataAttributes.Add(tuple.Item1.Replace("data_", string.Empty), tuple.Item2);
                }
            }
        }

        private static void ProcessMetadata(AppOptionInfo opts, EntityMap ent, Property prop)
        {
            var key = string.Concat(ent.Name, ".", prop.Name);
            List<Tuple<string, string>> metadata;
            if (opts.MetadataDictionary.TryGetValue(key, out metadata))
            {
                foreach (var tuple in metadata)
                {
                    prop.MetaDataAttributes.Add(tuple.Item1.Replace("data_", string.Empty), tuple.Item2);
                }
            }

            if (prop.PropertyName.Equals("CreatedOn") || prop.PropertyName.Equals("CreatedBy"))
            {
                prop.MetaDataAttributes.Add("editable", "false");
            }
        }

        private static void ProcessActivatedProperties(AppOptionInfo opts, EntityMap ent)
        {
            var isTrackable = string.Concat(ent.Name, ".IsTrackable");
            var extends = string.Concat(ent.Name, ".Extends");
            var tableAlias = string.Concat(ent.Name, ".TableAlias");

            if (opts.ActivatedProperties.ContainsKey(isTrackable))
            {
                bool val;
                bool.TryParse(opts.ActivatedProperties[isTrackable], out val);
                ent.IsTrackable = val;
            }

            if (opts.ActivatedProperties.ContainsKey(extends))
            {
                ent.Extends = opts.ActivatedProperties[extends];
            }

            if (opts.ActivatedProperties.ContainsKey(tableAlias))
            {
                ent.TableAlias = opts.ActivatedProperties[tableAlias];
            }
        }

        private static void ProcessActivatedProperties(AppOptionInfo opts, EntityMap ent, Property prop)
        {
            var ignoreOnUpdate = string.Concat(ent.Name, ".", prop.Name, ".IgnoreOnUpdate");
            var lazyload = string.Concat(ent.Name, ".", prop.Name, ".LazyLoad");
            var isNullable = string.Concat(ent.Name, ".", prop.Name, ".IsNullable");
            var isUnique = string.Concat(ent.Name, ".", prop.Name, ".IsUnique");

            if (opts.ActivatedProperties.ContainsKey(ignoreOnUpdate))
            {
                bool val;
                bool.TryParse(opts.ActivatedProperties[ignoreOnUpdate], out val);
                prop.IgnoreOnUpdate = val;
            }

            if (opts.ActivatedProperties.ContainsKey(lazyload))
            {
                bool val;
                bool.TryParse(opts.ActivatedProperties[lazyload], out val);
                prop.LazyLoad = val;
            }

            if (opts.ActivatedProperties.ContainsKey(isNullable))
            {
                bool val;
                bool.TryParse(opts.ActivatedProperties[isNullable], out val);
                prop.IsNullable = val;
            }

            if (opts.ActivatedProperties.ContainsKey(isUnique))
            {
                bool val;
                bool.TryParse(opts.ActivatedProperties[isUnique], out val);
                prop.IsUnique = val;
            }

            if (prop.PropertyName.Equals("CreatedOn") || prop.PropertyName.Equals("CreatedBy"))
            {
                prop.IgnoreOnUpdate = true;
            }
        }


        static void PrintError(string errorMessage, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: {0}", errorMessage);
            Console.WriteLine(ex);
            Console.ResetColor();
            Console.WriteLine("\n\nExited with errors...");
        }
    }
}

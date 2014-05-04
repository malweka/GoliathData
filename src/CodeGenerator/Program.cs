using System;
using System.IO;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Providers.SqlServer;

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
            var opts = AppOptionHandler.ParseOptions(args);

            Console.WriteLine("Starting application. Generated files will be saved on Folder: {0} ", opts.WorkingFolder);
            Console.WriteLine("Template Folder: {0} \n", opts.TemplateFolder);
#if DEBUG
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
#endif

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

            else rdbms = SupportedRdbms.Mssql2008;

            Console.Write("Provider");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" {0} ", rdbms);
            Console.ResetColor();
            Console.Write("Activated.");
            Console.WriteLine("\n\nLoading settings...");

            var codeGenRunner = new CodeGenRunner(rdbms, new GenericCodeGenerator())
                                {
                                    TemplateFolder = opts.TemplateFolder,
                                    ScriptFolder = AppDomain.CurrentDomain.BaseDirectory,
                                    DatabaseFolder = AppDomain.CurrentDomain.BaseDirectory,
                                    WorkingFolder = opts.WorkingFolder,
                                    Settings =
                                    {
                                        Namespace = opts.Namespace,
                                        AssemblyName = opts.AssemblyName,
                                        ConnectionString = opts.ConnectionString,
                                        Platform = "Mssql2008R2"

                                    }
                                };


            var action = opts.ActionName.ToUpper();
            switch (action)
            {
                case "GENERATEENTITIES":
                    try
                    {
                        GenerateEntities(opts, codeGenRunner);
                        logger.Log(LogLevel.Debug, string.Format("Entities generated in work folder {0}.", opts.WorkingFolder));
                    }
                    catch (Exception ex) { PrintError("Exception thrown while trying to generate entities.", ex); }
                    break;
                case "GENERATEALL":
                    try
                    {
                        GenerateAllFromTemplate(opts, codeGenRunner);
                        logger.Log(LogLevel.Debug, string.Format("Generated all files based on templates in work folder {0}.", opts.WorkingFolder));
                    }
                    catch (Exception ex) { PrintError("Exception thrown while trying to generate all.", ex); }
                    break;
                case "GENERATE":
                    try
                    {
                        GenerateFromTemplate(opts, codeGenRunner);
                        logger.Log(LogLevel.Debug, string.Format("File {0} generated in folder {1}.", opts.OutputFile, opts.WorkingFolder));
                    }
                    catch (Exception ex) { PrintError("Exception thrown while trying to generate from template.", ex); }
                    break;
                default:
                    try
                    {
                        CreateMap(opts, rdbms, codeGenRunner);
                        logger.Log(LogLevel.Debug, string.Format("\n\nMap {0} in {1} has been created.", opts.MapFile, opts.WorkingFolder));
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
            if (!File.Exists(opts.MapFile))
            {
                codeMapFile = Path.Combine(opts.WorkingFolder, Path.GetFileName(opts.MapFile));
            }

            Console.WriteLine("Reading Code Map file...");
            if (!File.Exists(codeMapFile) && throwIfNotExist)
                throw new GoliathDataException(string.Format("Map file {0} not found.", codeMapFile));

            return codeMapFile;
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

            codeGenRunner.GenerateClassesFromTemplate(map, template, codeGenRunner.WorkingFolder, (name) => GetFileName(name, opts.OutputFile), opts.ExcludedArray);

        }

        static string GetFileName(string entityName, string outputFile)
        {
            if(outputFile.Contains("(name)"))
            {
                return outputFile.Replace("(name)", entityName);
            }

            return string.Concat(entityName, outputFile);
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

        static void CreateMap(AppOptionInfo opts, SupportedRdbms rdbms, CodeGenRunner codeGenRunner)
        {
            Console.WriteLine("\n\nCreate Map...");
            var providerFactory = new ProviderFactory();
            ComplexType baseModel = null;
            var mapFileName = GetCodeMapFile(opts, false);

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
                    //process keygen
                    if (!string.IsNullOrWhiteSpace(opts.DefaultKeygen))
                    {
                        foreach (var ent in map.EntityConfigs)
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

                            if (!string.IsNullOrWhiteSpace(opts.ComplexTypeMap))
                            {
                                foreach (var prop in ent)
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
                    }
                    
                    map.Save(mapFileName, true);
                }
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

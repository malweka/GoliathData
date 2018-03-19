using System;
using System.Collections.Generic;
using System.IO;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.CodeGenerator.Actions
{
    class CreateMapAction : ActionRunner
    {
        public const string Name = "CREATEMAP";
        public CreateMapAction() : base(Name)
        {
        }

        public override void Exetute(AppOptionInfo opts, CodeGenRunner codeGenRunner)
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

            var rdbms = GetSupportedRdbms(opts);

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
                string prima_orda = Path.Combine(opts.WorkingFolder, PrimaOrdaFile);
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
    }
}
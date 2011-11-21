using System.IO;

namespace Goliath.Data.CodeGen
{
    using Generators;
    using Mapping;
    using Providers;
    using Providers.Sqlite;
    using Providers.SqlServer;
    using Transformers;

    class CodeGenerator : IGenerator
    {

        #region ICodeGenerator Members

        public void GenerateCode(string templateFolder, string workingFolder)
        {
            string mapfile = Path.Combine(workingFolder, Constants.MapFileName);
            MapConfig project = MapConfig.Create(mapfile);
            var basefolder = workingFolder;
            ICodeGenerator generator = new RazorCodeGenerator();
            foreach (var table in project.EntityConfigs)
            {
                if (table.IsLinkTable)
                    continue;

                string fname = Path.Combine(basefolder, table.Name + ".cs");
                generator.Generate(Path.Combine(templateFolder, "Class.razt"), fname, table);
            }
        }
             
        public MapConfig GenerateMapping(string workingFolder, ProjectSettings settings, ComplexType baseModel, SupportedRdbms rdbms)
        {
            if ((rdbms == SupportedRdbms.Mssql2005) || (rdbms == SupportedRdbms.Mssql2008))
            {
                SqlMapper mapper = new Mssq2008SqlMapper();
                IDbConnector dbConnector = new MssqlDbConnector(settings.ConnectionString);
                IDbAccess db = new DbAccess(dbConnector);
                using (ISchemaDescriptor schemaDescriptor = new MssqlSchemaDescriptor(db, dbConnector, mapper, settings))
                {
                    return Build(workingFolder, settings, baseModel, schemaDescriptor);
                }

            }
            else if (rdbms == SupportedRdbms.Sqlite3)
            {
                SqlMapper mapper = new SqliteSqlMapper();
                IDbConnector dbConnector = new SqliteDbConnector(settings.ConnectionString);
                IDbAccess db = new DbAccess(dbConnector);

                using (ISchemaDescriptor schema = new SqliteSchemaDescriptor(db, dbConnector, mapper, settings))
                {
                    return Build(workingFolder, settings, baseModel, schema);
                }
            }
			else
				return null;
        }

        #endregion

        MapConfig Build(string workingFolder, ProjectSettings settings, ComplexType baseModel, ISchemaDescriptor schema)
        {

            //SqlMapper mapper = new SqliteSqlMapper();
            //IDbConnector dbConnector = new SqliteDbConnector(settings.ConnectionString);
            //IDbAccess db = new DbAccess(dbConnector);

            schema.ProjectSettings = settings;
            DataModelGenerator generator = new DataModelGenerator(schema, new NameTransformerFactory(settings),
                new DefaultTableNameAbbreviator());

            MapConfig builder = generator.GenerateMap(settings, baseModel);
            CreateFolderIfNotExist(workingFolder);
            string mapfile = Path.Combine(workingFolder, Constants.MapFileName);
            builder.Save(mapfile, true);
			return builder;

        }

        static void CreateFolderIfNotExist(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

    }

    public class Constants
    {
        public const string MapFileName = "GoData.Map.xml";
    }
}

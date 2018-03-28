using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goliath.Data.CodeGenerator.Actions
{
    class ImportAction: ActionRunner
    {
        public const string Name = "IMPORT";

        public ImportAction() : base(Name)
        {
        }

        public override void Exetute(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            bool zipFile = false || !string.IsNullOrWhiteSpace(opts.TemplateName) && opts.TemplateName.ToUpper().EndsWith(".ZIP");

            if (zipFile)
            {
                var inputFile = Path.Combine(opts.WorkingFolder, opts.TemplateName);
                if(!File.Exists(inputFile))
                    throw new ArgumentException($"Could not find the input file: {inputFile}");

                using (var fs = File.Open(inputFile, FileMode.Open, FileAccess.Read))
                {
                    fs.Unzip(opts.WorkingFolder);
                }
            }
        }

       
    }

    class DataImporter
    {
        protected AppOptionInfo Options { get; }

        public DataImporter(AppOptionInfo opts)
        {
            Options = opts;
        }

        protected virtual bool DatabaseExists(string dbName)
        {
            return false;
        }

        public void Import()
        {
            var dbName = FileHelperMethods.GetDatabaseNameFromConnectionString(Options.ConnectionString);
            if (DatabaseExists(dbName))
            {
                //TODO: create database and tables
            }
        }
    }
}

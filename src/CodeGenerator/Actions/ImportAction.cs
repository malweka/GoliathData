using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Goliath.Data.DataAccess;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.CodeGenerator.Actions
{
    class ImportAction : ActionRunner
    {
        public const string Name = "IMPORT";
        TypeConverterStore converter = new TypeConverterStore();

        public ImportAction() : base(Name)
        {
        }

        public override void Exetute(AppOptionInfo opts, CodeGenRunner codeGenRunner)
        {
            bool zipFile = false || !string.IsNullOrWhiteSpace(opts.TemplateName) && opts.TemplateName.ToUpper().EndsWith(".ZIP");

            var codeMapFile = GetCodeMapFile(opts);
            var map = MapConfig.Create(codeMapFile, true);
            map.Settings.AssemblyName = opts.AssemblyName;
            map.Settings.Namespace = opts.Namespace;

            var importRdbms = GetRdbms(opts.ImportSqlDialect);
            var providerFactory = new ProviderFactory();
            var dbConnector = providerFactory.CreateDbConnector(importRdbms, opts.ConnectionString);
            //var dialect = providerFactory.CreateDialect(importRdbms);

            Logger.Log(LogLevel.Debug, $"Importing using {importRdbms}");

            if (zipFile)
            {
                var inputFile = Path.Combine(opts.WorkingFolder, opts.TemplateName);
                if (!File.Exists(inputFile))
                    throw new ArgumentException($"Could not find the input file: {inputFile}");

                using (var fs = File.Open(inputFile, FileMode.Open, FileAccess.Read))
                {
                    fs.Unzip(opts.WorkingFolder);
                }
            }

            var files = Directory.EnumerateFiles(opts.WorkingFolder, "*.xml").ToDictionary(Path.GetFileNameWithoutExtension);
            using (var conn = dbConnector.CreateNewConnection() as SqlConnection)
            {
                if (conn == null)
                    throw new InvalidOperationException("Import is only supported on SQL Server for now.");

                conn.Open();
                var transaction = conn.BeginTransaction();

                try
                {
                    foreach (var table in map.EntityConfigs)
                    {
                        if (!files.TryGetValue(table.Name, out var file))
                            continue;

                        Logger.Log(LogLevel.Debug, $"Reading: {file}");
                        ImportData(map, conn, transaction, opts, file);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    transaction.Dispose();
                }
            }
        }

        void ImportData(MapConfig mapConfig, SqlConnection conn, SqlTransaction transaction, AppOptionInfo opts, string filePath)
        {
            var exportedData = ExportedDataModel.LoadFromFile(filePath);
            var ent = mapConfig.GetEntityMap(exportedData.EntityName);
            Logger.Log(LogLevel.Debug, $"[{ent.FullName}] - table: {ent.TableName} - rows: {exportedData.DataRows.Count}");
            string destinationTableName = $"[{ent.SchemaName}].[{ent.TableName}]";

            try
            {
                DataTable table = ent.CreateTable();
                RazorInterpreter interpreter = new RazorInterpreter();
                int count = 0;

                if (opts.Merge)
                {
                    //create table

                    var tableCreate = interpreter.CompileTemplate(Templates.CreateTempTable, ent);
                    destinationTableName = $"[{ent.SchemaName}].[#{ent.TableName}]";

                    using (var outputFile = File.Create($"C:\\Junk\\Test\\{ent.TableName}Create.sql"))
                    {
                        interpreter.Generate(Templates.CreateTempTable, outputFile, ent);
                    }

                    using (var sf = File.Create($"C:\\Junk\\Test\\{ent.TableName}Merge.sql"))
                    {
                        interpreter.Generate(Templates.Merge, sf, ent);
                    }

                    ExecuteCommand(tableCreate, conn, transaction);
                }

                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
                {
                    foreach (DataColumn tableColumn in table.Columns)
                    {
                        sqlBulkCopy.ColumnMappings.Add(tableColumn.ColumnName, tableColumn.ColumnName);
                    }
                    foreach (var exportedDataDataRow in exportedData.DataRows)
                    {
                        var dataRow = table.NewRow();
                        foreach (var field in exportedDataDataRow)
                        {
                            var prop = ent.FindPropertyByColumnName(field.Key);
                            var clrType = SqlTypeHelper.GetClrType(prop.DbType, prop.IsNullable);
                            var column = table.Columns[field.Key];
                            if (field.Value == null || string.IsNullOrWhiteSpace(field.Value.ToString()))
                            {
                                dataRow[field.Key] = DBNull.Value;
                            }
                            else
                            {
                                var convertMethod = converter.GetConverterFactoryMethod(clrType);
                                var value = convertMethod(field.Value.ToString().Trim());
                                dataRow[field.Key] = value ?? DBNull.Value;
                            }
                        }

                        count++;
                        Logger.Log(LogLevel.Debug, "Adding row " + count);
                        table.Rows.Add(dataRow);
                        dataRow.AcceptChanges();
                    }

                    sqlBulkCopy.DestinationTableName = destinationTableName;
                    Logger.Log(LogLevel.Debug, $"Data table [{sqlBulkCopy.DestinationTableName}] loaded. Will now write to the database.");
                    sqlBulkCopy.WriteToServer(table);

                    if (opts.Merge)
                    {
                        var mergeScript = interpreter.CompileTemplate(Templates.Merge, ent);
                        ExecuteCommand(mergeScript, conn, transaction);
                        ExecuteCommand($"DROP TABLE {destinationTableName}", conn, transaction);
                    }

                    table.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Error trying to import data to {ent.TableName}...");
                throw;
            }

        }

        void ExecuteCommand(string commandText, SqlConnection conn, SqlTransaction transaction)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = commandText;
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();
            }
        }

    }
}

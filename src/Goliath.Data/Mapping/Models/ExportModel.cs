﻿using System;
using System.Collections.Generic;
using System.Data;
using Goliath.Data.DataAccess;

using Goliath.Data.Providers;
using Goliath.Data.Utilities;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public enum ExportType
    {
        /// <summary>
        /// The SQL
        /// </summary>
        Sql,
        /// <summary>
        /// The XML
        /// </summary>
        Xml,
        /// <summary>
        /// The excel
        /// </summary>
        Excel
    }

    /// <summary>
    /// 
    /// </summary>
    public class ExportModel
    {
        private static ILogger logger = Logger.GetLogger(typeof(ExportModel));
        /// <summary>
        /// Gets or sets the dialect.
        /// </summary>
        /// <value>
        /// The dialect.
        /// </value>
        public SqlDialect ImportDialect { get; set; }

        private SqlDialect exportDialect;

        /// <summary>
        /// Gets or sets the export dialect.
        /// </summary>
        /// <value>
        /// The export dialect.
        /// </value>
        public SqlDialect ExportDialect
        {
            get { return exportDialect ?? (exportDialect = ImportDialect); }
            set { exportDialect = value; }
        }

        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public EntityMap Map { get; set; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public ExportOptions Options { get; } = new ExportOptions();

        internal ITypeConverterStore TypeConverterStore { get; set; }

        /// <summary>
        /// Gets the data bag.
        /// </summary>
        /// <value>
        /// The data bag.
        /// </value>
        public IList<IDictionary<string, object>> DataBag { get; internal set; } = new List<IDictionary<string, object>>();

        /// <summary>
        /// Builds the SQL statement.
        /// </summary>
        /// <returns></returns>
        public virtual string SelectSqlStatement()
        {
            var columns = new Dictionary<string, string>();

            if (Map.PrimaryKey != null)
            {
                foreach (var pk in Map.PrimaryKey.Keys)
                {
                    Property prop = pk;

                    if (prop.IsIdentity && !Options.ExportIdentityColumn)
                        continue;

                    if (prop.IsAutoGenerated && !Options.ExportDatabaseGeneratedColumns)
                        continue;

                    var colName = ImportDialect.Escape(prop.ColumnName);
                    logger.Log(LogLevel.Debug, $"adding primary key: {colName} - property: {prop.PropertyName}");
                    columns.Add(colName, prop.PropertyName);
                }
            }

            foreach (var prop in Map.Properties)
            {
                if (prop.IsAutoGenerated && !Options.ExportDatabaseGeneratedColumns)
                    continue;

                var colName = ImportDialect.Escape(prop.ColumnName);
                logger.Log(LogLevel.Debug, $"adding column: {colName} - property: {prop.PropertyName}");
                if (!columns.ContainsKey(colName) && !prop.IsMappingComplexType())
                    columns.Add(colName, prop.PropertyName);
            }

            foreach (var prop in Map.Relations)
            {
                if (prop.RelationType > RelationshipType.ManyToOne)
                    continue;
                if (prop.IsAutoGenerated && !Options.ExportDatabaseGeneratedColumns)
                    continue;

                var colName = ImportDialect.Escape(prop.ColumnName);
                if (!columns.ContainsKey(colName))
                    columns.Add(colName, prop.PropertyName);
            }

            return $"SELECT TOP 20 {string.Join(", ", columns.Keys)} FROM {ImportDialect.Escape(Map.SchemaName)}.{ImportDialect.Escape(Map.TableName)}";
        }

        /// <summary>
        /// Lists the data for export.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        /// <returns></returns>
        public virtual IList<IDictionary<string, string>> ListDataForExport(ExportType exportType)
        {
            var list = new List<IDictionary<string, string>>();
            foreach (var row in DataBag)
            {
                var convertedRow = ProcessRow(row, exportType);
                list.Add(convertedRow);
            }

            return list;
        }

        IDictionary<string, string> ProcessRow(IDictionary<string, object> row, ExportType exportType)
        {
            var dico = new Dictionary<string, string>();
            foreach (var field in row)
            {
                Property prop = Map.FindPropertyByColumnName(field.Key);
                if (prop == null) continue;

                string value;
                string key;

                if (exportType == ExportType.Sql)
                {
                    key = ExportDialect.Escape(field.Key);
                    var clrType = ExportDialect.GetClrType(prop.DbType, prop.IsNullable);
                    var converter = TypeConverterStore.GetConverterFactoryMethod(clrType);
                    var clrValue = converter(field.Value);
                    value = ExportDialect.GetValueAsSqlString(clrValue, prop);
                }
                else
                {
                    key = field.Key;
                    value = DbTypeConverter.ConvertTo<string>(field.Value);
                }

                dico.Add(key, value);
            }

            return dico;
        }

    }
}
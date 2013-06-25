﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;

namespace Goliath.Data.Providers.Postgres
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PostgresSchemaDescriptor : SchemaDescriptor
    {
        static readonly ILogger logger;
        readonly IDbAccess db;
        readonly SqlDialect dialect;
        readonly IDbConnector dbConnector;

        DbConnection connection;
        DbConnection Connection
        {
            get
            {
                if (connection == null)
                {
                    connection = dbConnector.CreateNewConnection();
                    connection.Open();
                }
                return connection;
            }
        }


        private const string SELECT_TABLE_FROM_SCHEMA = @"SELECT table_name, table_schema FROM information_schema.tables 
where table_type = 'BASE TABLE'
and table_schema <> 'pg_catalog' and table_schema <> 'information_schema'";
        private const string SELECT_COLUMNS = @"select cols.*, colCons.constraint_name,cons.constraint_type,cons.is_deferrable,cons.initially_deferred from information_schema.columns cols
left join information_schema.constraint_column_usage colCons on cols.column_name = colCons.column_name and cols.table_name = colCons.table_name
left join information_schema.table_constraints cons on colCons.constraint_name = cons.constraint_name
where cols.table_name = $tableName";
        private const string SELECT_REFERENCES = @"select rf.*, cs.table_schema as referenced_table_schema, cs.table_name as referenced_table_name, col.column_name as referenced_column_name, col.constraint_name as referenced_constraint_name, cs.constraint_type 
from information_schema.referential_constraints rf
INNER JOIN information_schema.table_constraints cs on cs.constraint_name = rf.unique_constraint_name
INNER JOIN information_schema.constraint_column_usage col on col.constraint_name = cs.constraint_name
where rf.constraint_name = $constrainName";

        static PostgresSchemaDescriptor()
        {
            logger = Logger.GetLogger(typeof(PostgresSchemaDescriptor));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresSchemaDescriptor" /> class.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="dbConnector">The db connector.</param>
        /// <param name="dialect">The dialect.</param>
        /// <param name="settings">The settings.</param>
        public PostgresSchemaDescriptor(IDbAccess db, IDbConnector dbConnector, SqlDialect dialect, ProjectSettings settings)
            : base(RdbmsBackend.SupportedSystemNames.Postgresql9)
        {
            this.db = db;
            this.dbConnector = dbConnector;
            this.dialect = dialect;
            ProjectSettings = settings;
        }

        /// <summary>
        /// Gets the tables.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override IDictionary<string, EntityMap> GetTables()
        {
            var tables = new Dictionary<string, EntityMap>();
            try
            {
                using (var reader = db.ExecuteReader(Connection, SELECT_TABLE_FROM_SCHEMA))
                {
                    while (reader.Read())
                    {
                        string tablename = reader.GetValueAsString("table_name");
                        if (string.IsNullOrWhiteSpace(tablename) || (!string.IsNullOrWhiteSpace(tablename) && tablename.Equals("sysdiagrams", StringComparison.OrdinalIgnoreCase)))
                            continue;

                        string schemaName = reader.GetValueAsString("table_schema");
                        logger.Log(LogLevel.Info, string.Format("reading table {0}", tablename));
                        var table = new EntityMap(tablename, tablename);
                        table.Namespace = ProjectSettings.Namespace;
                        table.SchemaName = schemaName;
                        table.AssemblyName = ProjectSettings.AssemblyName;
                        table.TableAlias = tablename;
                        tables.Add(tablename, table);
                    }
                }

                foreach (var table in tables.Values)
                {
                    logger.Log(LogLevel.Info, string.Format("processing table {0}", table.Name));
                    var columns = ProcessColumns(table);
                    table.AddColumnRange(columns.Values);
                }
            }
            catch (Exception ex)
            {
                logger.LogException("Error while getting table structure", ex);
                throw;
            }
            return tables;
        }

        /// <summary>
        /// Processes the columns.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        protected virtual Dictionary<string, Property> ProcessColumns(EntityMap table)
        {
            var columnList = new Dictionary<string, Property>();
            using (var reader = db.ExecuteReader(Connection, SELECT_COLUMNS, new QueryParam("tableName", table.TableName)))
            {
                while (reader.Read())
                {
                    string colName = reader.GetValueAsString("column_name");
                    string dataType = reader.GetValueAsString("udt_name");
                    int? length = reader.GetValueAsInt("character_maximum_length");
                    int? precision = reader.GetValueAsInt("numeric_precision");
                    int? scale = reader.GetValueAsInt("numeric_scale");

                    logger.Log(LogLevel.Info, string.Format("\t column: {0} {1}({2})", colName, dataType, length ?? 0));
                    Property col = null;
                    if (length.HasValue)
                    {
                        if (length < 0)
                            length = 2000;
                        col = new Property(colName, colName, dialect.SqlStringToDbType(dataType)) { Length = length.Value };
                    }
                    else
                        col = new Property(colName, colName, dialect.SqlStringToDbType(dataType));

                    if (precision.HasValue)
                        col.Precision = precision.Value;
                    else
                        col.Precision = -1;
                    if (scale.HasValue)
                        col.Scale = scale.Value;
                    else
                        col.Scale = -1;

                    col.SqlType = dataType;
                    bool isNullable = reader.GetValueAsString("is_nullable").Equals("YES");
                    string columnDefault = reader.GetValueAsString("column_default");
                    if (!string.IsNullOrWhiteSpace(columnDefault) && columnDefault.Contains("nextval(") && columnDefault.Contains("seq'::regclass)"))
                    {
                        col.IsIdentity = true;
                        col.IsAutoGenerated = true;
                    }

                    col.IsNullable = isNullable;
                    //TODO: implement ProcessDefaultValue
                    //TODO: implement metadata
                    //col.DefaultValue = ProcessDefaultValue(reader.GetValueAsString("COLUMN_DEFAULT"));
                    col.ClrType = dialect.GetClrType(col.DbType, isNullable);
                    //OnTableAddProperty(table, col);

                    //get constraints
                    string constraintType = reader.GetValueAsString("constraint_type");
                    if (!string.IsNullOrWhiteSpace(constraintType))
                    {
                        string constrainName = reader.GetValueAsString("constraint_name");

                        if (constraintType.ToUpper().Equals("UNIQUE"))
                            col.IsUnique = true;
                        else if (constraintType.ToUpper().Equals("PRIMARY KEY"))
                        {
                            col.IsPrimaryKey = true;
                            col.IsUnique = true;
                        }
                        else if (constraintType.ToUpper().Equals("FOREIGN KEY"))
                        {
                            col = ReadForeignKeyReference(col, constrainName);
                        }

                        col.ConstraintName = constrainName;
                    }

                    columnList.Add(colName, col);
                }
            }

            return columnList;
        }

        Relation ReadForeignKeyReference(Property col, string fkConstraintName)
        {
            using (var reader = db.ExecuteReader(Connection, SELECT_REFERENCES, new QueryParam("constrainName", fkConstraintName)))
            {
                Relation rel = null;
                while (reader.Read())
                {
                    rel = new Relation(col);
                    string refTable = reader.GetValueAsString("referenced_table_name");
                    string refSchema = reader.GetValueAsString("referenced_table_schema");
                    string refColName = reader.GetValueAsString("referenced_column_name");
                    string refconstName = reader.GetValueAsString("referenced_constraint_name");
                    rel.ReferenceColumn = refColName;
                    rel.ReferenceProperty = refColName;
                    rel.ReferenceTable = refTable;
                    rel.ReferenceTableSchemaName = refSchema;
                    rel.ReferenceConstraintName = refconstName;
                    rel.RelationType = RelationshipType.ManyToOne;

                    rel.ReferenceEntityName = refTable;

                    break;
                }
                return rel;
            }
        }

        /// <summary>
        /// Gets the views.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override IDictionary<string, Mapping.View> GetViews()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the stored procs.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override IDictionary<string, Mapping.StatementMap> GetStoredProcs()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }
}

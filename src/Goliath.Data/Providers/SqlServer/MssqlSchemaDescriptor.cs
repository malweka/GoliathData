﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;

namespace Goliath.Data.Providers.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MssqlSchemaDescriptor : SchemaDescriptor
    {
        static ILogger logger;
        IDbAccess db;
        SqlDialect dialect;
        IDbConnector dbConnector;
        const string SELECT_TABLE_FROM_SCHEMA = "SELECT TABLE_NAME, TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
        const string SELECT_COLUMNS = "SELECT *, COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') AS IsIdentity, IDENT_SEED(TABLE_NAME) AS IdentitySeed, IDENT_INCR(TABLE_NAME) AS IdentityIncrement FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName ORDER BY ORDINAL_POSITION";
        const string SELECT_CONSTRAINTS = @"SELECT COLUMN_NAME, CONSTRAINT_TYPE, a.CONSTRAINT_NAME as ConstraintName,
	(SELECT COUNT(*) FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE c WHERE a.CONSTRAINT_NAME = c.CONSTRAINT_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE a
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS b ON a.CONSTRAINT_NAME = b.CONSTRAINT_NAME
WHERE a.TABLE_NAME = @tableName";
        const string SELECT_REFERENCES = @"SELECT
COLUMN_NAME = FK_COLS.COLUMN_NAME,
REFERENCED_TABLE_NAME = PK.TABLE_NAME,
REFERENCED_TABLE_SCHEMA = PK.TABLE_SCHEMA,
REFERENCED_COLUMN_NAME = PK_COLS.COLUMN_NAME,
REFERENCED_CONSTRAINT_NAME = FK.CONSTRAINT_NAME
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS REF_CONST
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK
ON REF_CONST.CONSTRAINT_CATALOG = FK.CONSTRAINT_CATALOG
AND REF_CONST.CONSTRAINT_SCHEMA = FK.CONSTRAINT_SCHEMA
AND REF_CONST.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
AND FK.CONSTRAINT_TYPE = 'FOREIGN KEY'
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON REF_CONST.UNIQUE_CONSTRAINT_CATALOG = PK.CONSTRAINT_CATALOG
AND REF_CONST.UNIQUE_CONSTRAINT_SCHEMA = PK.CONSTRAINT_SCHEMA
AND REF_CONST.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
AND PK.CONSTRAINT_TYPE = 'PRIMARY KEY'
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE FK_COLS ON REF_CONST.CONSTRAINT_NAME = FK_COLS.CONSTRAINT_NAME
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE PK_COLS ON PK.CONSTRAINT_NAME = PK_COLS.CONSTRAINT_NAME
WHERE FK.TABLE_NAME = @tableName";
        const string FIND_FOREIGN_KEYS = @"SELECT
CONSTRAINT_NAME = FK.CONSTRAINT_NAME,
COLUMN_NAME = FK_COLS.COLUMN_NAME,
REFERENCED_TABLE_NAME = PK.TABLE_NAME,
REFERENCED_TABLE_SCHEMA = PK.TABLE_SCHEMA,
REFERENCED_COLUMN_NAME = PK_COLS.COLUMN_NAME
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS REF_CONST
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK
ON REF_CONST.CONSTRAINT_CATALOG = FK.CONSTRAINT_CATALOG
AND REF_CONST.CONSTRAINT_SCHEMA = FK.CONSTRAINT_SCHEMA
AND REF_CONST.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
AND FK.CONSTRAINT_TYPE = 'FOREIGN KEY'
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON REF_CONST.UNIQUE_CONSTRAINT_CATALOG = PK.CONSTRAINT_CATALOG
AND REF_CONST.UNIQUE_CONSTRAINT_SCHEMA = PK.CONSTRAINT_SCHEMA
AND REF_CONST.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
AND PK.CONSTRAINT_TYPE = 'PRIMARY KEY'
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE FK_COLS ON REF_CONST.CONSTRAINT_NAME = FK_COLS.CONSTRAINT_NAME
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE PK_COLS ON PK.CONSTRAINT_NAME = PK_COLS.CONSTRAINT_NAME
WHERE FK.TABLE_NAME = @tableName";

        const string tableDescriptionScript = @"select t.id as TableId, t.name as TableName, t.uid, ep.name, ep.value as TableDescription from sysobjects t, sys.extended_properties ep
where t.type = 'u' and t.id = ep.major_id and ep.minor_id = 0";
        const string colDescriptionScript = @"select c.name as ColName, ep.value as ColDescription from sys.extended_properties ep, syscolumns c
where  ep.major_id = @tableId
and c.id = @tableId
and ep.major_id = c.id
and ep.minor_id = c.colid";

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

        static MssqlSchemaDescriptor()
        {
            logger = Logger.GetLogger(typeof(MssqlSchemaDescriptor));
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlSchemaDescriptor"/> class.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="dbConnector">The db connector.</param>
        /// <param name="dialect">The dialect.</param>
        /// <param name="settings">The settings.</param>
        public MssqlSchemaDescriptor(IDbAccess db, IDbConnector dbConnector, SqlDialect dialect, ProjectSettings settings)
            : base(RdbmsBackend.SupportedSystemNames.Mssql2008R2)
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
        public override IDictionary<string, EntityMap> GetTables()
        {
            var tables = new Dictionary<string, EntityMap>();
            try
            {
                using (DbDataReader reader = db.ExecuteReader(Connection, SELECT_TABLE_FROM_SCHEMA))
                {
                    while (reader.Read())
                    {
                        string name = reader.GetValueAsString("TABLE_NAME");
                        if (!string.IsNullOrWhiteSpace(name) && name.Equals("sysdiagrams", StringComparison.OrdinalIgnoreCase))
                            continue;

                        string schemaName = reader.GetValueAsString("TABLE_SCHEMA");
                        logger.Log(LogLevel.Info, string.Format("reading table {0}", name));
                        EntityMap table = new EntityMap(name, name);
                        table.Namespace = ProjectSettings.Namespace;
                        table.SchemaName = schemaName;
                        table.AssemblyName = ProjectSettings.AssemblyName;
                        table.TableAlias = name;
                        tables.Add(name, table);
                    }
                }
                var tableMetaDataDictionary = GetTableMetaData();

                foreach (var table in tables.Values)
                {
                    logger.Log(LogLevel.Info, string.Format("processing table {0}", table.Name));
                    var columns = ProcessColumns(table);
                    ProcessConstraints(table, columns);
                    ProcessForeignKeys(table);
                    ProcessReferences(table, columns);
                    table.AddColumnRange(columns.Values);

                    TableMetaData meta;
                    if (tableMetaDataDictionary.TryGetValue(table.TableName, out meta))
                    {
                        if(!string.IsNullOrWhiteSpace(meta.Description))
                        {
                            table.MetaDataAttributes.Add("description", meta.Description);
                        }

                        foreach (var column in columns)
                        {
                            string colMeta;
                            if(meta.ColumnMetadata.TryGetValue(column.Value.ColumnName, out colMeta))
                            {
                                column.Value.MetaDataAttributes.Add("description", colMeta);
                            }
                        }
                    }

                    
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
            Dictionary<string, Property> columnList = new Dictionary<string, Property>();
            using (DbDataReader reader = db.ExecuteReader(Connection, SELECT_COLUMNS, new QueryParam("tableName", table.TableName)))
            {
                while (reader.Read())
                {
                    string colName = reader.GetValueAsString("COLUMN_NAME");
                    string dataType = reader.GetValueAsString("DATA_TYPE");
                    int? length = reader.GetValueAsInt("CHARACTER_MAXIMUM_LENGTH");
                    int? precision = reader.GetValueAsInt("NUMERIC_PRECISION");
                    int? scale = reader.GetValueAsInt("NUMERIC_SCALE");

                    logger.Log(LogLevel.Info, string.Format("\t column: {0} {1}({2})", colName, dataType, length ?? 0));
                    Property col = null;
                    if (length.HasValue)
                    {
                        //col = new Property(table, colName, mapper.SqlStringToDbType(dataType), length.Value);
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

                    bool isNullable = reader.GetValueAsString("IS_NULLABLE").Equals("YES");
                    bool isIdentity = reader.GetValueAsInt("IsIdentity") == 1;

                    col.IsIdentity = isIdentity;
                    if (isIdentity)
                    {
                        col.IsAutoGenerated = true;                
                    }

                    col.IsNullable = isNullable;
                    col.DefaultValue = ProcessDefaultValue(reader.GetValueAsString("COLUMN_DEFAULT"));
                    col.ClrType = dialect.GetClrType(col.DbType, isNullable);
                    OnTableAddProperty(table, col);
                    columnList.Add(colName, col);

                }
            }
            return columnList;
        }

        /// <summary>
        /// Called when [table add property].
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="property">The property.</param>
        protected virtual void OnTableAddProperty(EntityMap table, Property property)
        {
            //should be overriden by sub class
        }

        /// <summary>
        /// Processes the constraints.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="columnList">The column list.</param>
        protected virtual void ProcessConstraints(EntityMap table, Dictionary<string, Property> columnList)
        {
            List<string> constraints = new List<string>();
            using (var reader = db.ExecuteReader(Connection, SELECT_CONSTRAINTS, new QueryParam("tableName", table.TableName)))
            {
                while (reader.Read())
                {
                    string colName = reader.GetValueAsString("COLUMN_NAME");
                    string constraintType = reader.GetValueAsString("CONSTRAINT_TYPE");
                    int? columnCount = reader.GetValueAsInt("ColumnCount");

                    if (columnCount.HasValue && columnCount.Value > 0)
                    {
                        var col = columnList[colName];
                        if (constraintType.ToUpper().Equals("UNIQUE"))
                            col.IsUnique = true;
                        else if (constraintType.ToUpper().Equals("PRIMARY KEY"))
                        {
                            col.IsPrimaryKey = true;
                            col.IsUnique = true;
                        }

                        col.ConstraintName = reader.GetValueAsString("ConstraintName");
                    }
                    //else
                    //{
                    //   string constraintName = reader.GetStringVal("ConstraintName");

                    //   if (!constraints.Contains(constraintName))
                    //   {
                    //      ConstraintType? type = null;
                    //      switch (constraintType)
                    //      {
                    //         case "UNIQUE":
                    //            type = ConstraintType.Unique;
                    //            break;
                    //         case "PRIMARY KEY":
                    //            type = ConstraintType.PrimaryKey;
                    //            break;
                    //      }
                    //      //if (type != null)
                    //      //{
                    //      //   constraint = new Constraint(constraintName, type.Value, null);
                    //      //   constraints.Add(constraintName, constraint);
                    //      //}
                    //   }
                    //   //constraint.Columns.Add(colName);
                    //}
                }
            }
        }

        /// <summary>
        /// Processes the references.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="columns">The columns.</param>
        protected virtual void ProcessReferences(EntityMap table, Dictionary<string, Property> columns)
        {
            using (var reader = db.ExecuteReader(Connection, SELECT_REFERENCES, new QueryParam("tableName", table.TableName)))
            {
                while (reader.Read())
                {
                    string colName = reader.GetValueAsString("COLUMN_NAME");
                    //var namefactory = transfactory.GetTransformer<Relation>();
                    Property col;
                    if (columns.TryGetValue(colName, out col))
                    {
                        Relation rel = new Relation(col);
                        string refTable = reader.GetValueAsString("REFERENCED_TABLE_NAME");
                        string refSchema = reader.GetValueAsString("REFERENCED_TABLE_SCHEMA");
                        string refColName = reader.GetValueAsString("REFERENCED_COLUMN_NAME");
                        string refconstName = reader.GetValueAsString("REFERENCED_CONSTRAINT_NAME");
                        rel.ReferenceColumn = refColName;
                        rel.ReferenceProperty = refColName;
                        rel.ReferenceTable = refTable;
                        rel.ReferenceTableSchemaName = refSchema;
                        rel.ReferenceConstraintName = refconstName;
                        rel.RelationType = RelationshipType.ManyToOne;

                        rel.ReferenceEntityName = refTable;// entityNameTransformer.Transform(null, refTable);
                        //namefactory.Transform(rel, colName);

                        columns.Remove(colName);
                        columns.Add(colName, rel);

                    }
                    else
                    {
                        logger.Log(LogLevel.Warning, "Not Found");
                    }

                    //column.ConstraintName = new ColumnReference(refTable, refSchema, refColName);
                    //if (!table.DependsOnTables.Contains(refTable) && !table.Name.Equals(refTable))
                    //{
                    //   table.DependsOnTables.Add(refTable);
                    //}
                }
            }
        }

        void ProcessForeignKeys(EntityMap table)
        {
            using (var reader = db.ExecuteReader(Connection, FIND_FOREIGN_KEYS, new QueryParam("tableName", table.TableName)))
            {
                while (reader.Read())
                {
                    //ForeignKey key = new ForeignKey(reader.GetStringVal("CONSTRAINT_NAME"), table, reader.GetStringVal("COLUMN_NAME"), reader.GetStringVal("REFERENCED_TABLE_NAME"),
                    //    reader.GetStringVal("REFERENCED_TABLE_SCHEMA"), reader.GetStringVal("REFERENCED_COLUMN_NAME"));
                    //table.ForeignKeys.Add(key);
                }
            }
        }

        /// <summary>
        /// Processes the default value.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        protected virtual string ProcessDefaultValue(string defaultValue)
        {
            if (!string.IsNullOrWhiteSpace(defaultValue))
            {
                defaultValue = defaultValue.Replace("(", string.Empty)
                    .Replace(")", string.Empty);

                switch (defaultValue.ToLower())
                {
                    case "newid":
                        defaultValue = Sql.FunctionNames.NewGuid;
                        break;
                    case "getdate":
                        defaultValue = Sql.FunctionNames.GetDate;
                        break;
                    case "getutcdate":
                        defaultValue = Sql.FunctionNames.GetUtcDate;
                        break;
                    case "suser_sname":
                        defaultValue = Sql.FunctionNames.GetUserName;
                        break;
                    case "host_name":
                        defaultValue = Sql.FunctionNames.GetHostName;
                        break;
                    case "app_name":
                        defaultValue = Sql.FunctionNames.GetAppName;
                        break;
                    case "db_name":
                        defaultValue = Sql.FunctionNames.GetDatabaseName;
                        break;
                    default:
                        break;
                }

            }
            return defaultValue;
        }

        public Dictionary<string, TableMetaData> GetTableMetaData()
        {
           var tables = new Dictionary<string, TableMetaData>();

            using (var reader = db.ExecuteReader(Connection, tableDescriptionScript))
            {
                while (reader.Read())
                {
                    TableMetaData tbMetadata = new TableMetaData();
                    tbMetadata.Name = reader.GetValueAsString("TableName");
                    tbMetadata.Id = reader.GetValueAsLong("TableId");
                    tbMetadata.Description = reader.GetValueAsString("TableDescription");
                    tables.Add(tbMetadata.Name, tbMetadata);
                }
            }

            foreach (var tb in tables.Values)
            {
                using (DbDataReader reader = db.ExecuteReader(connection, colDescriptionScript, new QueryParam("tableId", tb.Id)))
                {
                    while (reader.Read())
                    {
                        var colName = reader.GetValueAsString("ColName");
                        var colDescription = reader.GetValueAsString("ColDescription");
                        tb.ColumnMetadata.Add(colName, colDescription);
                    }
                }
            }
            return tables;
        }

        /// <summary>
        /// Gets the views.
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, View> GetViews()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the stored procs.
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, StatementMap> GetStoredProcs()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Members

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

        #endregion
    }
}

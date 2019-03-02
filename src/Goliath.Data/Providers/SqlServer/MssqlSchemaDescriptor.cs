using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.Providers.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MssqlSchemaDescriptor : SchemaDescriptor
    {
        static readonly ILogger logger;
        readonly IDbAccess db;
        readonly IDbConnector dbConnector;
        const string SelectTableFromSchema = "SELECT TABLE_NAME, TABLE_SCHEMA, OBJECT_ID(TABLE_SCHEMA +'.'+TABLE_NAME)  \"TableId\" FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' order by TABLE_NAME";
        const string SelectColumns = "SELECT *, COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') AS IsIdentity, IDENT_SEED(TABLE_NAME) AS IdentitySeed, IDENT_INCR(TABLE_NAME) AS IdentityIncrement FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName and TABLE_SCHEMA = @schema ORDER BY ORDINAL_POSITION";
        const string SelectConstraints = @"SELECT COLUMN_NAME, CONSTRAINT_TYPE, a.CONSTRAINT_NAME as ConstraintName,
	(SELECT COUNT(*) FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE c WHERE a.CONSTRAINT_NAME = c.CONSTRAINT_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE a
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS b ON a.CONSTRAINT_NAME = b.CONSTRAINT_NAME
WHERE a.TABLE_NAME = @tableName and a.TABLE_SCHEMA = @schema";
        const string SelectReferences = @"SELECT
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
WHERE FK.TABLE_NAME = @tableName and FK.TABLE_SCHEMA = @schema";
        const string FindForeignKeys = @"SELECT
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
WHERE FK.TABLE_NAME = @tableName and FK.TABLE_SCHEMA = @schema";

        const string TableDescriptionScript = @"select t.id as TableId, t.name as TableName, t.uid, ep.name, ep.value as TableDescription from sysobjects t, sys.extended_properties ep
where t.type = 'u' and t.id = ep.major_id and ep.minor_id = 0";
        const string ColDescriptionScript = @"select c.name as ColName, ep.value as ColDescription from sys.extended_properties ep, syscolumns c
where  ep.major_id = @tableId
and c.id = @tableId
and ep.major_id = c.id
and ep.minor_id = c.colid";

        DbConnection connection;
        DbConnection Connection
        {
            get
            {
                if (connection != null) return connection;
                connection = dbConnector.CreateNewConnection();
                connection.Open();
                return connection;
            }
        }

        static MssqlSchemaDescriptor()
        {
            logger = Logger.GetLogger(typeof(MssqlSchemaDescriptor));
        }

        

        /// <summary>
        /// Initializes a new instance of the <see cref="MssqlSchemaDescriptor" /> class.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="dbConnector">The db connector.</param>
        /// <param name="dialect">The dialect.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="tableBlackList">The excluded tables.</param>
        public MssqlSchemaDescriptor(IDbAccess db, IDbConnector dbConnector, SqlDialect dialect, ProjectSettings settings, params string[] tableBlackList)
            : base(RdbmsBackend.SupportedSystemNames.Mssql2008R2, dialect)
        {
            this.db = db;
            this.dbConnector = dbConnector;
            ProjectSettings = settings;

            if (tableBlackList != null)
            {
                FilterSettings = new FilterSettings {TableFilterList = tableBlackList};
            }
        }

        public MssqlSchemaDescriptor(IDbAccess db, IDbConnector dbConnector, SqlDialect dialect, 
            ProjectSettings settings, FilterSettings filterSettings)
            : base(RdbmsBackend.SupportedSystemNames.Mssql2008R2, dialect, filterSettings)
        {
            this.db = db;
            this.dbConnector = dbConnector;
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
                using (DbDataReader reader = db.ExecuteReader(Connection, SelectTableFromSchema))
                {
                    int counterOrder = 0;
                    while (reader.Read())
                    {
                        string name = reader.GetValueAsString("TABLE_NAME");
                        string schemaName = reader.GetValueAsString("TABLE_SCHEMA");

                        counterOrder++;
                        if (FilterSettings.Exclude)
                        {
                            if (IsTableInFilterList(schemaName, name))
                                continue;
                        }
                        else
                        {
                            if(!IsTableInFilterList(schemaName, name))
                                continue;
                        }

                        if (!string.IsNullOrWhiteSpace(name) && name.Equals("sysdiagrams", StringComparison.OrdinalIgnoreCase))
                            continue;

                        
                        string tableId = reader.GetValueAsString("TableId");

                        logger.Log(LogLevel.Info, $"reading table {schemaName}.{name}");
                        string @namespace = ProjectSettings.Namespace; 

                        if ( !string.IsNullOrWhiteSpace(schemaName) && !Dialect.DefaultSchemaName.ToUpper().Equals(schemaName.ToUpper()))
                        {
                            @namespace = $"{ProjectSettings.Namespace}.{schemaName.ToClrValPascal()}";
                        }

                        var table = new EntityMap(name, name)
                        {
                            Namespace = @namespace,
                            SchemaName = schemaName,
                            AssemblyName = ProjectSettings.AssemblyName,
                            TableAlias = name,
                            Order = counterOrder,
                            Id = tableId
                        };

                        tables.Add($"{schemaName}.{name}", table);
                    }
                }
                var tableMetaDataDictionary = GetTableMetaData();

                foreach (var table in tables.Values)
                {
                    logger.Log(LogLevel.Info, $"processing table {table.Name}");
                    var columns = ProcessColumns(table);
                    ProcessConstraints(table, columns);
                    ProcessForeignKeys(table);
                    ProcessReferences(table, columns);
                    table.AddColumnRange(columns.Values);

                    TableMetaData meta;
                    if (tableMetaDataDictionary.TryGetValue(table.Id, out meta))
                    {
                        if (!string.IsNullOrWhiteSpace(meta.Description))
                        {
                            table.MetaDataAttributes.Add("display_description", meta.Description);
                        }

                        foreach (var column in columns)
                        {
                            string colMeta;
                            if (meta.ColumnMetadata.TryGetValue(column.Value.ColumnName, out colMeta))
                            {
                                column.Value.MetaDataAttributes.Add("display_description", colMeta);
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
            using (DbDataReader reader = db.ExecuteReader(Connection, SelectColumns, 
                new QueryParam("tableName", table.TableName, DbType.String), 
                new QueryParam("schema", table.SchemaName, DbType.String)))
            {
                int countOrder = 0;
                while (reader.Read())
                {
                    countOrder++;
                    string colName = reader.GetValueAsString("COLUMN_NAME");
                    string dataType = reader.GetValueAsString("DATA_TYPE").ToLower();
                    int? length = reader.GetValueAsInt("CHARACTER_MAXIMUM_LENGTH");
                    int? precision = reader.GetValueAsInt("NUMERIC_PRECISION");
                    int? scale = reader.GetValueAsInt("NUMERIC_SCALE");

                    logger.Log(LogLevel.Info, $"\t column: {colName} {dataType}({length ?? 0})");
                    Property col = null;
                    if (length.HasValue)
                    {
                        //col = new Property(table, colName, mapper.SqlStringToDbType(dataType), length.Value);
                        if (length < 0)
                        {
                            if (dataType.Equals("nvarchar") || dataType.Equals("nchar"))
                                length = 4000;
                            else if (dataType.Equals("varchar") || dataType.Equals("char"))
                                length = 8000;

                            else
                            {
                                length = 8000;
                            }
                        }
                        col = new Property(colName, colName, Dialect.SqlStringToDbType(dataType)) { Length = length.Value };
                    }
                    else
                        col = new Property(colName, colName, Dialect.SqlStringToDbType(dataType));

                    if (precision.HasValue)
                        col.Precision = precision.Value;
                    else
                        col.Precision = -1;
                    if (scale.HasValue)
                        col.Scale = scale.Value;
                    else
                        col.Scale = -1;

                    col.SqlType = dataType;
                    col.Order = countOrder;

                    bool isNullable = reader.GetValueAsString("IS_NULLABLE").Equals("YES");
                    bool isIdentity = reader.GetValueAsInt("IsIdentity") == 1;

                    col.IsIdentity = isIdentity;
                    if (isIdentity)
                    {
                        col.IsAutoGenerated = true;
                    }

                    col.IsNullable = isNullable;
                    col.DefaultValue = ProcessDefaultValue(reader.GetValueAsString("COLUMN_DEFAULT"));
                    col.ClrType = Dialect.GetClrType(col.DbType, isNullable);
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
            using (var reader = db.ExecuteReader(Connection, SelectConstraints, 
                new QueryParam("tableName", table.TableName, DbType.String),
                new QueryParam("schema", table.SchemaName, DbType.String)))
            {
                while (reader.Read())
                {
                    string colName = reader.GetValueAsString("COLUMN_NAME");
                    string constraintType = reader.GetValueAsString("CONSTRAINT_TYPE");
                    int? columnCount = reader.GetValueAsInt("ColumnCount");

                    if (!columnCount.HasValue || columnCount.Value <= 0) continue;

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
            }
        }

        /// <summary>
        /// Processes the references.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="columns">The columns.</param>
        protected virtual void ProcessReferences(EntityMap table, Dictionary<string, Property> columns)
        {
            using (var reader = db.ExecuteReader(Connection, SelectReferences, 
                new QueryParam("tableName", table.TableName, DbType.String),
                new QueryParam("schema", table.SchemaName, DbType.String)))
            {
                while (reader.Read())
                {
                    string colName = reader.GetValueAsString("COLUMN_NAME");
                    Property col;
                    if (columns.TryGetValue(colName, out col))
                    {
                        var rel = new Relation(col);
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

                        if (FilterSettings.Exclude)
                        {
                            if (IsTableInFilterList(refSchema, refTable))
                                continue;
                        }
                        else
                        {
                            if (!IsTableInFilterList(refSchema, refTable))
                                continue;
                        }

                        rel.ReferenceEntityName = refTable;
                        columns.Remove(colName);
                        columns.Add(colName, rel);

                    }
                    else
                    {
                        logger.Log(LogLevel.Warning, "Not Found");
                    }
                }
            }
        }

        void ProcessForeignKeys(EntityMap table)
        {
            using (var reader = db.ExecuteReader(Connection, FindForeignKeys, 
                new QueryParam("tableName", table.TableName, DbType.String),
                new QueryParam("schema", table.SchemaName, DbType.String)))
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
            if (string.IsNullOrWhiteSpace(defaultValue)) return defaultValue;

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
            return defaultValue;
        }

        public Dictionary<string, TableMetaData> GetTableMetaData()
        {
            var tables = new Dictionary<string, TableMetaData>();

            using (var reader = db.ExecuteReader(Connection, TableDescriptionScript))
            {
                while (reader.Read())
                {
                    var tbMetadata = new TableMetaData
                    {
                        Name = reader.GetValueAsString("TableName"),
                        Id = reader.GetValueAsLong("TableId"),
                        Description = reader.GetValueAsString("TableDescription")
                    };
                    tables.Add(tbMetadata.Id.ToString(), tbMetadata);
                }
            }

            foreach (var tb in tables.Values)
            {
                using (DbDataReader reader = db.ExecuteReader(connection, ColDescriptionScript, new QueryParam("tableId", tb.Id, DbType.Int64)))
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
            connection?.Dispose();
        }

        #endregion
    }
}

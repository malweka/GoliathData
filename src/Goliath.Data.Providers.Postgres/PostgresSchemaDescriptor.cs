using System;
using System.Collections.Generic;
using System.Data;
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
where cols.table_name = @tableName
and (cons.constraint_type is null or cons.constraint_type <> 'FOREIGN KEY') 
ORDER BY cols.ordinal_position ASC";

        private const string SQL_CONSTRAINT_DETAILS = @"select rf.*, cs.table_schema as referenced_table_schema, cs.table_name as referenced_table_name, col.column_name as referenced_column_name, col.constraint_name as referenced_constraint_name, cs.constraint_type 
from information_schema.referential_constraints rf
INNER JOIN information_schema.table_constraints cs on cs.constraint_name = rf.unique_constraint_name
INNER JOIN information_schema.constraint_column_usage col on col.constraint_name = cs.constraint_name
where rf.constraint_name = @constrainName";

        private const string SQL_SELECT_REFERENCES = @"SELECT c.conname AS constraint_name,
          CASE c.contype           
            WHEN 'c' THEN 'CHECK'
            WHEN 'f' THEN 'FOREIGN KEY'
            WHEN 'p' THEN 'PRIMARY KEY'
            WHEN 'u' THEN 'UNIQUE'
          END AS constraint_type,
          CASE WHEN c.condeferrable = 'f' THEN 0 ELSE 1 END AS is_deferrable,
          CASE WHEN c.condeferred = 'f' THEN 0 ELSE 1 END AS is_deferred,
          t.relname AS table_name,
          array_to_string(c.conkey, ' ') AS constraint_key,
          CASE confupdtype
            WHEN 'a' THEN 'NO ACTION'
            WHEN 'r' THEN 'RESTRICT'
            WHEN 'c' THEN 'CASCADE'
            WHEN 'n' THEN 'SET NULL'
            WHEN 'd' THEN 'SET DEFAULT'
          END AS on_update,
          CASE confdeltype
            WHEN 'a' THEN 'NO ACTION'
            WHEN 'r' THEN 'RESTRICT'
            WHEN 'c' THEN 'CASCADE'
            WHEN 'n' THEN 'SET NULL'
            WHEN 'd' THEN 'SET DEFAULT'
          END AS on_delete,
          CASE confmatchtype
            WHEN 'u' THEN 'UNSPECIFIED'
            WHEN 'f' THEN 'FULL'
            WHEN 'p' THEN 'PARTIAL'
          END AS match_type,
          t2.relname AS references_table,
          array_to_string(c.confkey, ' ') AS fk_constraint_key
     FROM pg_constraint c
LEFT JOIN pg_class t  ON c.conrelid  = t.oid
LEFT JOIN pg_class t2 ON c.confrelid = t2.oid
where c.contype = 'f'";

        private Dictionary<string, string[]> tableColumnMap = new Dictionary<string, string[]>();

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
        /// <param name="tableBlackList">The excluded tables.</param>
        public PostgresSchemaDescriptor(IDbAccess db, IDbConnector dbConnector, SqlDialect dialect, ProjectSettings settings, params string[] tableBlackList)
            : base(RdbmsBackend.SupportedSystemNames.Postgresql9, tableBlackList, dialect)
        {
            this.db = db;
            this.dbConnector = dbConnector;
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
                    int countOrder = 0;
                    while (reader.Read())
                    {
                        string tablename = reader.GetValueAsString("table_name");
                        string schemaName = reader.GetValueAsString("table_schema");
                        countOrder++;

                        if (IsExcluded(schemaName, tablename))
                            continue;

                        if (string.IsNullOrWhiteSpace(tablename) || (!string.IsNullOrWhiteSpace(tablename) && tablename.Equals("sysdiagrams", StringComparison.OrdinalIgnoreCase)))
                            continue;

                       
                        logger.Log(LogLevel.Info, string.Format("reading table {0}", tablename));
                        var table = new EntityMap(tablename, tablename);
                        table.Namespace = ProjectSettings.Namespace;
                        table.SchemaName = schemaName;
                        table.Order = countOrder;
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

                ProcessReferences(tables);
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
            //var references = new Dictionary<string, Property>();

            using (var reader = db.ExecuteReader(Connection, SELECT_COLUMNS, new QueryParam("tableName", table.TableName, DbType.String)))
            {
                var columnMap = new List<string>();
                int countOrder = 0;
                while (reader.Read())
                {
                    string colName = reader.GetValueAsString("column_name");
                    string dataType = reader.GetValueAsString("udt_name");
                    int? length = reader.GetValueAsInt("character_maximum_length");
                    int? precision = reader.GetValueAsInt("numeric_precision");
                    int? scale = reader.GetValueAsInt("numeric_scale");
                    countOrder++;

                    logger.Log(LogLevel.Info, string.Format("\t column: {0} {1}({2})", colName, dataType, length ?? 0));
                    columnMap.Add(colName);

                    Property col = null;
                    if (length.HasValue)
                    {
                        if (length < 0)
                            length = 2000;
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
                    col.ClrType = Dialect.GetClrType(col.DbType, isNullable);
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
                        //else if (constraintType.ToUpper().Equals("FOREIGN KEY"))
                        //{
                        //    references.Add(constrainName, col);
                        //    continue;
                        //    //col = ReadForeignKeyReference(col, constrainName);
                        //}

                        col.ConstraintName = constrainName;
                    }

                    columnList.Add(colName, col);
                }

                tableColumnMap.Add(table.TableName, columnMap.ToArray());

                //foreach(var rl in references)
                //{
                //    var refCol = ReadForeignKeyReference(rl.Value, rl.Key);
                //    refCol.ConstraintName = rl.Key;
                //    if (columnList.ContainsKey(refCol.ColumnName))
                //        columnList.Remove(refCol.ColumnName);
                //    columnList.Add(refCol.ColumnName, refCol);
                //}
            }

            return columnList;
        }

        void ProcessReferences(Dictionary<string, EntityMap> tables)
        {
            try
            {
                var fkList = new List<ForeignKeyMeta>();
                using (var reader = db.ExecuteReader(Connection, SQL_SELECT_REFERENCES))
                {
                    while (reader.Read())
                    {
                        var meta = new ForeignKeyMeta
                        {
                            ConstraintName = reader.GetValueAsString("constraint_name"),
                            TableName = reader.GetValueAsString("table_name"),
                            ReferenceTableName = reader.GetValueAsString("references_table"),
                            ConstraintKey = reader.GetValueAsInt("constraint_key"),
                            ForeignKeyConstraintKey = reader.GetValueAsInt("fk_constraint_key")
                        };

                        logger.Log(LogLevel.Info, string.Format("Read foreign key: {0}", meta.ConstraintName));
                        fkList.Add(meta);
                    }
                }

                foreach (var meta in fkList)
                {
                    EntityMap entMap;
                    string[] entCols;
                    if (tables.TryGetValue(meta.TableName, out entMap) && tableColumnMap.TryGetValue(meta.TableName, out entCols))
                    {
                        if (!meta.ConstraintKey.HasValue)
                        {
                            logger.Log(LogLevel.Warning, string.Format("Constraint {0} didn't produce a key number", meta.ConstraintName));
                            continue;
                        }
                        var columnName = entCols[meta.ConstraintKey.Value - 1];
                        //find the column that references
                        var col = entMap[columnName];
                        //var refCol = ReadConstraintDetails(col, meta.ConstraintName);
                        EntityMap refEntMap;
                        string[] refEntCols;

                        if (tables.TryGetValue(meta.ReferenceTableName, out refEntMap) && tableColumnMap.TryGetValue(meta.ReferenceTableName, out refEntCols))
                        {

                            if (!meta.ForeignKeyConstraintKey.HasValue)
                            {
                                logger.Log(LogLevel.Warning, string.Format("ForeignKeyConstraintKey {0} didn't produce a key number", meta.ConstraintName));
                                continue;
                            }

                            var refColumnName = refEntCols[meta.ForeignKeyConstraintKey.Value - 1];
                            var refCol = refEntMap[refColumnName];

                            var rel = new Relation(col)
                            {
                                ReferenceColumn = refCol.ColumnName,
                                ReferenceProperty = refCol.Name,
                                ReferenceTable = refEntMap.TableName,
                                ReferenceTableSchemaName = refEntMap.SchemaName,
                                ReferenceConstraintName = meta.ConstraintName,
                                RelationType = RelationshipType.ManyToOne,
                            };

                            entMap.Remove(col);
                            entMap.Add(rel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogException("Error while getting table structure", ex);
                throw;
            }
        }

        //Relation ReadConstraintDetails(Property col, string fkConstraintName)
        //{
        //    using (var reader = db.ExecuteReader(Connection, SQL_CONSTRAINT_DETAILS, new QueryParam("constrainName", fkConstraintName)))
        //    {
        //        Relation rel = null;
        //        while (reader.Read())
        //        {
        //            rel = new Relation(col);
        //            string refTable = reader.GetValueAsString("referenced_table_name");
        //            string refSchema = reader.GetValueAsString("referenced_table_schema");
        //            string refColName = reader.GetValueAsString("referenced_column_name");
        //            string refconstName = reader.GetValueAsString("referenced_constraint_name");
        //            rel.ReferenceColumn = refColName;
        //            rel.ReferenceProperty = refColName;
        //            rel.ReferenceTable = refTable;
        //            rel.ReferenceTableSchemaName = refSchema;
        //            rel.ReferenceConstraintName = refconstName;
        //            rel.RelationType = RelationshipType.ManyToOne;

        //            rel.ReferenceEntityName = refTable;

        //            break;
        //        }
        //        return rel;
        //    }
        //}

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

        struct ForeignKeyMeta
        {
            public string ConstraintName;
            public string TableName;
            public int? ConstraintKey;
            public string ReferenceTableName;
            public int? ForeignKeyConstraintKey;
        }


    }
}

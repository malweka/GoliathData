using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Goliath.Data.Mapping;

namespace Goliath.Data.Providers.Sqlite
{
    [Serializable]
    public class SqliteSchemaDescriptor : SchemaDescriptor
    {
        IDbAccess db;
        SqlDialect dialect;
        IDbConnector dbConnector;

        const string SELECT_TABLE_FROM_SCHEMA = @"select * from sqlite_master where type = 'table' and tbl_name not like 'sqlite_%'";
        const string SELECT_COLUMN = @"pragma table_info('{0}')";
        const string SELECT_REFERENCES = @"pragma foreign_key_list('{0}')";
        const string FindIndexForTable = "pragma index_list('{0}')";
        const string IndexInfo = "pragma index_info('{0}')";
        DbConnection connection;

        public override string DefaultSchemaName => "main";

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


        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteSchemaDescriptor"/> class.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="dbConnector">The database connector.</param>
        /// <param name="dialect">The dialect.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="tableBlackList">The excluded tables.</param>
        public SqliteSchemaDescriptor(IDbAccess db, IDbConnector dbConnector, SqlDialect dialect, ProjectSettings settings, params string[] tableBlackList)
            : base(RdbmsBackend.SupportedSystemNames.Sqlite3, tableBlackList)
        {
            this.db = db;
            this.dbConnector = dbConnector;
            this.dialect = dialect;
            ProjectSettings = settings;
        }

        #region ISchemaDescriptor Members

        public override IDictionary<string, EntityMap> GetTables()
        {
            Dictionary<string, EntityMap> tables = new Dictionary<string, EntityMap>();

            try
            {
                using (DbDataReader reader = db.ExecuteReader(Connection, SELECT_TABLE_FROM_SCHEMA))
                {
                    int countOrder = 0;
                    while (reader.Read())
                    {
                        string name = reader.GetValueAsString("tbl_name");
                        countOrder++;

                        if (IsExcluded(DefaultSchemaName, name))
                            continue;
 
                        EntityMap table = new EntityMap(name, name);
                        table.Namespace = ProjectSettings.Namespace;
                        table.SchemaName = DefaultSchemaName;
                        table.AssemblyName = ProjectSettings.AssemblyName;
                        table.TableAlias = name;
                        table.Order = countOrder;
                        tables.Add(name, table);
                    }
                }

                foreach (var table in tables.Values)
                {
                    var columns = ProcessColumns(table);
                    ProcessReferences(table, columns);
                    ProcessIndex(table, columns);
                    table.AddColumnRange(columns.Values);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            return tables;
        }

        public override IDictionary<string, View> GetViews()
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, StatementMap> GetStoredProcs()
        {
            throw new NotImplementedException();
        }

        #endregion

        Dictionary<string, Property> ProcessColumns(EntityMap table)
        {
            Dictionary<string, Property> columnList = new Dictionary<string, Property>();
            using (DbDataReader reader = db.ExecuteReader(Connection, string.Format(SELECT_COLUMN, table.TableName)))
            {
                int countOrder = 0;
                while (reader.Read())
                {
                    countOrder++;
                    string colName = reader.GetValueAsString("name");
                    string dataType = reader.GetValueAsString("type");
                    var typeSplit = dataType.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    int? length = null;
                    if (typeSplit.Length > 1)
                    {
                        dataType = typeSplit[0];
                        int lgth;
                        if (int.TryParse(typeSplit[1], out lgth))
                        {
                            length = lgth;
                        }
                    }


                    Property col = null;
                    if (length.HasValue)
                    {
                        //col = new Property(table, colName, mapper.SqlStringToDbType(dataType), length.Value);
                        col = new Property(colName, colName, dialect.SqlStringToDbType(dataType)) { Length = length.Value };
                    }
                    else
                        col = new Property(colName, colName, dialect.SqlStringToDbType(dataType));

                    col.SqlType = dataType;

                    bool isNullable = reader.GetValueAsInt("notnull") != 1;
                    col.IsPrimaryKey = reader.GetValueAsInt("pk") == 1;
                    if (col.IsPrimaryKey)
                    {
                        isNullable = false;
                        col.IsUnique = true;
                    }

                    col.IsIdentity = false;
                    col.Order = countOrder;
                    //if (isIdentity)
                    //   col.IsAutoGenerated = true;

                    col.IsNullable = isNullable;
                    col.DefaultValue = reader.GetValueAsString("dflt_value");
                    col.ClrType = dialect.GetClrType(col.DbType, isNullable);

                    columnList.Add(colName, col);

                }
            }
            return columnList;
        }

        void ProcessReferences(EntityMap table, Dictionary<string, Property> columns)
        {
            using (var reader = db.ExecuteReader(Connection, string.Format(SELECT_REFERENCES, table.TableName)))
            {
                while (reader.Read())
                {
                    string colName = reader.GetValueAsString("from");
                    Property col;
                    if (columns.TryGetValue(colName, out col))
                    {
                        Relation rel = new Relation(col);
                        string refTable = reader.GetValueAsString("table");
                        string refSchema = "main";//reader.GetValueAsString("REFERENCED_TABLE_SCHEMA");
                        string refColName = reader.GetValueAsString("to");
                        string id = reader.GetValueAsString("id");
                        string refconstName = string.Format("fk_{0}_{1}{2}", table.Name, refTable, id);
                        rel.ReferenceColumn = refColName;
                        rel.ReferenceTable = refTable;
                        rel.ReferenceProperty = refColName;
                        rel.ReferenceTableSchemaName = refSchema;
                        rel.ReferenceConstraintName = refconstName;
                        rel.ReferenceEntityName = refTable;
                        rel.RelationType = RelationshipType.ManyToOne;
                        columns.Remove(colName);
                        columns.Add(colName, rel);

                    }
                    else
                    {
                        Console.Write("not found");
                    }
                }
            }
        }

        void ProcessIndex(EntityMap table, Dictionary<string, Property> columns)
        {
            List<string> indexes = new List<string>();
            using (var reader = db.ExecuteReader(Connection, string.Format(FindIndexForTable, table.TableName)))
            {

                while (reader.Read())
                {
                    string indexName = reader.GetValueAsString("name");
                    bool isUnique = reader.GetValueAsInt("unique") == 1;
                    if (isUnique)
                    {
                        indexes.Add(indexName);
                    }
                }
            }
            foreach (var iName in indexes)
            {
                using (var reader = db.ExecuteReader(Connection, string.Format(IndexInfo, iName)))
                {
                    while (reader.Read())
                    {
                        string colName = reader.GetValueAsString("name");
                        Property prop;
                        if (columns.TryGetValue(colName, out prop))
                        {
                            prop.IsUnique = true;
                        }
                    }
                }
            }
        }

        #region IDisposable Members

        public override void Dispose()
        {
            //if (db != null)
            //{
            //    db.Dispose();
            //}
            if (connection != null)
            {
                connection.Dispose();
            }
        }

        #endregion
    }
}

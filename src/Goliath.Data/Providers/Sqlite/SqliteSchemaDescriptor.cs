using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Goliath.Data.Transformers;
using Goliath.Data.Mapping;

namespace Goliath.Data.Providers.Sqlite
{
    public class SqliteSchemaDescriptor : SchemaDescriptor
    {
        IDbAccess db;
        SqlMapper mapper;
        IDbConnector dbConnector;

        const string SELECT_TABLE_FROM_SCHEMA = @"select * from sqlite_master where type = 'table' and tbl_name not like 'sqlite_%'";
        const string SELECT_COLUMN = @"pragma table_info('{0}')";
        const string SELECT_REFERENCES = @"pragma foreign_key_list('{0}')";
        const string FindIndexForTable = "pragma index_list('{0}')";
        const string IndexInfo = "pragma index_info('{0}')";
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


        public SqliteSchemaDescriptor(IDbAccess db, IDbConnector dbConnector, SqlMapper mapper, ProjectSettings settings)
            : base(Constants.ProviderName)
        {
            this.db = db;
            this.dbConnector = dbConnector;
            this.mapper = mapper;            
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
                    while (reader.Read())
                    {
                        string name = reader.GetValueAsString("tbl_name");
                        string schemaName = "main";
                        EntityMap table = new EntityMap(name, name);
                        table.Namespace = ProjectSettings.Namespace;
                        table.SchemaName = schemaName;
                        table.AssemblyName = ProjectSettings.AssemblyName;
                        table.TableAlias = name;
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

        public override IDictionary<string, StoredProcedure> GetStoredProcs()
        {
            throw new NotImplementedException();
        }

        #endregion

        Dictionary<string, Property> ProcessColumns(EntityMap table)
        {
            Dictionary<string, Property> columnList = new Dictionary<string, Property>();
            using (DbDataReader reader = db.ExecuteReader(Connection, string.Format(SELECT_COLUMN, table.TableName)))
            {
                while (reader.Read())
                {
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
                        col = new Property(colName, colName, mapper.SqlStringToDbType(dataType)) { Length = length.Value };
                    }
                    else
                        col = new Property(colName, colName, mapper.SqlStringToDbType(dataType));

                    col.SqlType = dataType;

                    bool isNullable = reader.GetValueAsInt("notnull") != 1;
                    col.IsPrimaryKey = reader.GetValueAsInt("pk") == 1;
                    if (col.IsPrimaryKey)
                    {
                        isNullable = false;
                        col.IsUnique = true;
                    }

                    col.IsIdentity = false;
                    //if (isIdentity)
                    //   col.IsAutoGenerated = true;

                    col.IsNullable = isNullable;
                    col.DefaultValue = reader.GetValueAsString("dflt_value");
                    col.ClrType = mapper.GetClrType(col.DbType, isNullable);

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

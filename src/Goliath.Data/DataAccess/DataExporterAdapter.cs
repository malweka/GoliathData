using System;
using System.Data;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class DataExporterAdapter
    {
        private readonly SqlDialect importDialect;
        private readonly SqlDialect exportDialect;
        private readonly IDbConnector dbConnector;
        private readonly ITypeConverterStore typeConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataExporterAdapter" /> class.
        /// </summary>
        /// <param name="importDialect">The dialect.</param>
        /// <param name="exportDialect">The export dialect.</param>
        /// <param name="dbConnector">The database connector.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public DataExporterAdapter(SqlDialect importDialect, SqlDialect exportDialect, IDbConnector dbConnector, ITypeConverterStore typeConverter)
        {
            if (importDialect == null) throw new ArgumentNullException(nameof(importDialect));
            if (dbConnector == null) throw new ArgumentNullException(nameof(dbConnector));
            if (typeConverter == null) throw new ArgumentNullException(nameof(typeConverter));

            this.importDialect = importDialect;
            this.exportDialect = exportDialect;
            this.dbConnector = dbConnector;
            this.typeConverter = typeConverter;
        }

        /// <summary>
        /// Exports the specified entity map.
        /// </summary>
        /// <param name="entityMap">The entity map.</param>
        /// <param name="exportIdentityColumn">if set to <c>true</c> [export identity column].</param>
        /// <param name="exportDatabaseGeneratedColumn">if set to <c>true</c> [export database generated column].</param>
        /// <returns></returns>
        public ExportModel Export(EntityMap entityMap, bool exportIdentityColumn = true, bool exportDatabaseGeneratedColumn = true)
        {
            var model = new ExportModel
            {
                ImportDialect = importDialect,
                ExportDialect = exportDialect,
                Map = entityMap,
                TypeConverterStore = typeConverter
            };
            model.Options.ExportIdentityColumn = exportIdentityColumn;
            model.Options.ExportDatabaseGeneratedColumns = exportDatabaseGeneratedColumn;

            IDbAccess dbAccess = new DbAccess(dbConnector);
            using (var conn = dbConnector.CreateNewConnection())
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                var dataBag = dbAccess.ExecuteDictionary(conn, model.SelectSqlStatement());
                model.DataBag = dataBag;
            }

            return model;
        }
    }
}
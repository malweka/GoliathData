using System;
using System.Data;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;

namespace Goliath.Data.DataAccess
{
    public class DataExporterAdapter
    {
        private readonly SqlDialect dialect;
        private readonly IDbConnector dbConnector;
        private ITypeConverterStore typeConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataExporterAdapter"/> class.
        /// </summary>
        /// <param name="dialect">The dialect.</param>
        /// <param name="dbConnector">The database connector.</param>
        /// <param name="typeConverter"></param>
        public DataExporterAdapter(SqlDialect dialect, IDbConnector dbConnector, ITypeConverterStore typeConverter)
        {
            if (dialect == null) throw new ArgumentNullException(nameof(dialect));
            if (dbConnector == null) throw new ArgumentNullException(nameof(dbConnector));
            if (typeConverter == null) throw new ArgumentNullException(nameof(typeConverter));

            this.dialect = dialect;
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
            var model = new ExportModel { Dialect = dialect, Map = entityMap, TypeConverterStore = typeConverter };
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
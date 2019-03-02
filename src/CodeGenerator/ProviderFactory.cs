using System;
using Goliath.Data.Mapping;
using Goliath.Data.Providers;
using Goliath.Data.Providers.SqlServer;
using Goliath.Data.Providers.Postgres;
using Goliath.Data.Providers.Sqlite;

namespace Goliath.Data.CodeGenerator
{
    public class ProviderFactory
    {
        /// <summary>
        /// Creates the dialect.
        /// </summary>
        /// <param name="platform">The platform.</param>
        /// <returns></returns>
        /// <exception cref="Goliath.Data.GoliathDataException"></exception>
        public SqlDialect CreateDialect(SupportedRdbms platform)
        {
            switch (platform)
            {
                case SupportedRdbms.Mssql2005:
                case SupportedRdbms.Mssql2008:
                case SupportedRdbms.Mssql2008R2:
                    return new Mssq2008Dialect();
                case SupportedRdbms.Postgresql8:
                case SupportedRdbms.Postgresql9:
                    return new PostgresDialect();
                case SupportedRdbms.Sqlite3:
                    return new SqliteDialect();
            }

            throw new GoliathDataException(string.Format("Platform not {0} supported", platform));
        }

        /// <summary>
        /// Creates the database connector.
        /// </summary>
        /// <param name="platform">The platform.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        /// <exception cref="Goliath.Data.GoliathDataException"></exception>
        public IDbConnector CreateDbConnector(SupportedRdbms platform, string connectionString)
        {
            switch (platform)
            {
                case SupportedRdbms.Mssql2005:
                case SupportedRdbms.Mssql2008:
                case SupportedRdbms.Mssql2008R2:
                    return new MssqlDbConnector(connectionString);
                case SupportedRdbms.Postgresql8:
                case SupportedRdbms.Postgresql9:
                    return new PostgresDbConnector(connectionString);
                case SupportedRdbms.Sqlite3:
                    return new SqliteDbConnector(connectionString);
            }

            throw new GoliathDataException(string.Format("Platform not {0} supported", platform));
        }

        /// <summary>
        /// Creates the database schema descriptor.
        /// </summary>
        /// <param name="platform">The platform.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="filterSettings">The filter settings.</param>
        /// <returns></returns>
        /// <exception cref="GoliathDataException"></exception>
        public ISchemaDescriptor CreateDbSchemaDescriptor(SupportedRdbms platform, ProjectSettings settings, FilterSettings filterSettings)
        {
            var dbConnector = CreateDbConnector(platform, settings.ConnectionString);
            var dialect = CreateDialect(platform);
            var dbAccess = new DbAccess(dbConnector);

            switch (platform)
            {
                case SupportedRdbms.Mssql2005:
                case SupportedRdbms.Mssql2008:
                case SupportedRdbms.Mssql2008R2:
                    return new MssqlSchemaDescriptor(dbAccess, dbConnector, dialect, settings, filterSettings);
                case SupportedRdbms.Postgresql8:
                case SupportedRdbms.Postgresql9:
                    return new PostgresSchemaDescriptor(dbAccess, dbConnector, dialect, settings, filterSettings);
                case SupportedRdbms.Sqlite3:
                    return new SqliteSchemaDescriptor(dbAccess, dbConnector, dialect, settings, filterSettings);
            }

            throw new GoliathDataException(string.Format("Platform not {0} supported", platform));
        }
    }
}

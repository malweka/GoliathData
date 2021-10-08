namespace Goliath.Data.Sqlite
{
    public interface ISqliteDataService
    {
        /// <summary>
        /// Creates the database.
        /// </summary>
        /// <param name="databaseFilePath">The database file path.</param>
        /// <param name="ddlScriptStream">The DDL script stream.</param>
        /// <returns></returns>
        IDbAccess CreateDatabase(string databaseFilePath, System.IO.Stream ddlScriptStream);

        /// <summary>
        /// Creates the database.
        /// </summary>
        /// <param name="databaseFilePath">The database file path.</param>
        /// <param name="ddlScripts">The DDL scripts.</param>
        /// <returns></returns>
        IDbAccess CreateDatabase(string databaseFilePath, string ddlScripts);

        /// <summary>
        /// Creates the session factory.
        /// </summary>
        /// <param name="mapfile">The mapfile.</param>
        /// <param name="databaseFile">The database file.</param>
        /// <returns></returns>
        ISessionFactory CreateSessionFactory(string mapfile, string databaseFile);
    }
}

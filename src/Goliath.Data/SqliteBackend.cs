
namespace Goliath.Data
{
    class SqliteBackend : RdbmsBackend
    {
        public SqliteBackend():base("Sqlite 3")
        {
            Id = SupportedSystemNames.Sqlite3;
        }
    }

    class Postgresql8Backend : RdbmsBackend
    {
        public Postgresql8Backend()
            : base("PostgreSql 8")
        {
            Id = SupportedSystemNames.Postgresql8;
        }
    }

    class Postgresql9Backend:RdbmsBackend
    {
        public Postgresql9Backend():base("PostgreSql 9")
        {
            Id = SupportedSystemNames.Postgresql9;
            CompatibilityGroup.Add(SupportedSystemNames.Postgresql8);
        }
    }

    class Mysql5Backend:RdbmsBackend
    {
        public Mysql5Backend():base("Mysql 5")
        {
            Id = SupportedSystemNames.MySql5;
        }
    }
}

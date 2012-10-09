
namespace Goliath.Data
{

    class SqlServer2005Backend : RdbmsBackend
    {
        public SqlServer2005Backend()
            : base("Microsoft SQL Server 2005")
        {
            Id = SupportedSystemNames.Mssql2005;
        }
    }

    ///// <summary>
    ///// 
    ///// </summary>
    class SqlServer2008Backend : RdbmsBackend
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServer2008Backend"/> class.
        /// </summary>
        public SqlServer2008Backend()
            : base("Microsoft SQL Server 2008")
        {
            Id = SupportedSystemNames.Mssql2008;
            CompatibilityGroup.Add(SupportedSystemNames.Mssql2005);
        }
    }


    class SqlServer2008R2Backend : RdbmsBackend
    {
        public SqlServer2008R2Backend()
            : base("Microsoft SQL Server 2008 R2")
        {
            Id = SupportedSystemNames.Mssql2008R2;
            CompatibilityGroup.Add(SupportedSystemNames.Mssql2005);
            CompatibilityGroup.Add(SupportedSystemNames.Mssql2008);
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.CodeGen
{
    using Generators;
    using Mapping;
    using Providers;
    using Providers.Sqlite;
    using Providers.SqlServer;
    using Sql;
    using Transformers;

    public interface IRunner
    {
        void RunQueryTests(string workingFolder);
    }

    public interface IGenerator
    {
        void GenerateCode(string templateFolder, string workingFolder);
        MapConfig GenerateMapping(string workingFolder, ProjectSettings settings, ComplexType baseModel, SupportedRdbms rdbms);
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SupportedRdbms
    {
        /// <summary>
        /// 
        /// </summary>
        Mssql2005,
        /// <summary>
        /// 
        /// </summary>
        Mssql2008,
        /// <summary>
        /// 
        /// </summary>
        Sqlite3,
        /// <summary>
        /// 
        /// </summary>
        Postgresql9,
    }
}

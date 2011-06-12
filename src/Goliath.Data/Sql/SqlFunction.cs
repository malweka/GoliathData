using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SqlFunction : ISqlFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlFunction"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="declaration">The declaration.</param>
        protected SqlFunction(string name, string declaration)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
            Declaration = declaration;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToSqlStatement();
        }


        #region ISqlFunction Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the function declaration.
        /// </summary>
        /// <value></value>
        public string Declaration { get; protected set; }

        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ReturnType { get; protected set; }

        /// <summary>
        /// Toes the SQL statement.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public virtual string ToSqlStatement(params QueryParam[] args)
        {
            StringBuilder parameters = new StringBuilder();
            if ((args != null) && (args.Length > 0))
            {
                int l = args.Length;                
                for(int i=0;i<l;i++)
                {
                    parameters.Append(args[i].Value.ToString());
                    if(i != (l-1))
                    {
                        parameters.Append(",");
                    }
                }
            }

            return string.Format("{0}({1})", Declaration, parameters.ToString());
        }

        #endregion
    }

    /// <summary>
    /// enumeration of sql function names
    /// </summary>
    public static class Functions
    {
        public const string NewGuid = "NewGuid";
        public const string GetDate = "GetDate";
        public const string GetUtcDate = "GetUtcDate";
        public const string GetUserName = "GetUserName";
        public const string GetDatabaseName = "GetDatabaseName";
        public const string GetHostName = "GetHostName";
        public const string GetAppName = "GetAppName";
    }
}

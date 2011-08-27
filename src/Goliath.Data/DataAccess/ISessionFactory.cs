using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISessionFactory
    {
        /// <summary>
        /// Gets the session settings.
        /// </summary>
        Config.ISessionSettings SessionSettings { get; }

        /// <summary>
        /// Opens the session.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        ISession OpenSession(DbConnection connection);
        /// <summary>
        /// Opens the session.
        /// </summary>
        /// <returns></returns>
        ISession OpenSession();

    }
}

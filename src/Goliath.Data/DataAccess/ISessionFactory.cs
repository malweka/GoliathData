using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISessionFactory
    {
        /// <summary>
        /// Opens the session.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        ISession OpenSession(IDbConnection connection);
        /// <summary>
        /// Opens the session.
        /// </summary>
        /// <returns></returns>
        ISession OpenSession();

    }
}

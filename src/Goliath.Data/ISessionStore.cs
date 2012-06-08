using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISessionStore //: IDisposable
    {
        /// <summary>
        /// stores the current session.
        /// </summary>
        /// <param name="sessionName">Name of the session.</param>
        /// <param name="session">The session.</param>
        void Store(string sessionName, ISession session);

        /// <summary>
        /// Tries the get session.
        /// </summary>
        /// <param name="sessionName">Name of the session.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        bool TryGetSession(string sessionName, out ISession session);

        /// <summary>
        /// Sessions the exists.
        /// </summary>
        /// <param name="sessionName">Name of the session.</param>
        /// <returns></returns>
        bool SessionExists(string sessionName);

        /// <summary>
        /// Evicts the specified session name.
        /// </summary>
        /// <param name="sessionName">Name of the session.</param>
        void Evict(string sessionName);
    }
}

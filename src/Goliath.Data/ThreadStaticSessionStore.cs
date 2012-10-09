using System;
using System.Collections.Generic;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class ThreadStaticSessionStore : ISessionStore
    {
        [ThreadStatic]
        static Dictionary<string, ISession> sessStore = new Dictionary<string, ISession>();

        #region ISessionStore Members

        /// <summary>
        /// stores the current session.
        /// </summary>
        /// <param name="sessionName"></param>
        /// <param name="session">The session.</param>
        public void Store(string sessionName, ISession session)
        {
            sessStore.Add(sessionName, session);
        }

        /// <summary>
        /// Gets the current session.
        /// </summary>
        /// <param name="sessionName">Name of the session.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public bool TryGetSession(string sessionName, out ISession session)
        {
            return sessStore.TryGetValue(sessionName, out session);
        }

        /// <summary>
        /// Sessions the exists.
        /// </summary>
        /// <param name="sessionName">Name of the session.</param>
        /// <returns></returns>
        public bool SessionExists(string sessionName)
        {
            if (sessStore.ContainsKey(sessionName))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Evicts the specified session name.
        /// </summary>
        /// <param name="sessionName">Name of the session.</param>
        public void Evict(string sessionName)
        {
            sessStore.Remove(sessionName);
        }

        #endregion

    }
}

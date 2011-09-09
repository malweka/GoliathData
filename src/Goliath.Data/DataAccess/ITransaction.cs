using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data
{
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is started.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is started; otherwise, <c>false</c>.
        /// </value>
        bool IsStarted { get; }
        /// <summary>
        /// Gets a value indicating whether [was committed].
        /// </summary>
        /// <value><c>true</c> if [was committed]; otherwise, <c>false</c>.</value>
        bool WasCommitted { get; }
        /// <summary>
        /// Gets a value indicating whether [was rolled back].
        /// </summary>
        /// <value><c>true</c> if [was rolled back]; otherwise, <c>false</c>.</value>
        bool WasRolledBack { get; }
        /// <summary>
        /// begin transaction with default isolation level
        /// </summary>
        void Begin();
        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <param name="isolatedLevel">The isolated level.</param>
        void Begin(System.Data.IsolationLevel isolatedLevel);
        /// <summary>
        /// Commits this instance.
        /// </summary>
        void Commit();
        /// <summary>
        /// Rollbacks this instance.
        /// </summary>
        void Rollback();
        /// <summary>
        /// Enlists the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        void Enlist(System.Data.IDbCommand command);
    }
}

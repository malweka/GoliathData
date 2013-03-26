using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    public interface IConnectionManager
    {
        ///// <summary>
        ///// Gets the current connection.
        ///// </summary>
        ///// <value>The current connection.</value>
        //DbConnection CurrentConnection { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has open connection.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has open connection; otherwise, <c>false</c>.
        /// </value>
        bool HasOpenConnection { get; }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns></returns>
        DbConnection OpenConnection();

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void CloseConnection();
    }
}
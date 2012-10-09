using System.Data.Common;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// Connection Provider Interface
    /// </summary>
   public interface IConnectionProvider
    {
        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns></returns>
        DbConnection GetConnection();

        /// <summary>
        /// Discards the of connection.
        /// </summary>
        /// <returns></returns>
        DbConnection DiscardOfConnection(DbConnection dbConnection);
    }
}

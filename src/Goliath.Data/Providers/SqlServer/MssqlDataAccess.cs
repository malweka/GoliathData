using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Goliath.Data.Providers.SqlServer
{
    public class MssqlDataAccess : DbAccess 
    {
        public MssqlDataAccess(string connectionString): base(Constants.ProviderName)
        {
            ConnectionString = connectionString;
        }
        //dispose of created connection? pool them if need be
        public override System.Data.Common.DbConnection CreateNewConnection()
        {
            DbConnection retVal;
            retVal = new SqlConnection(ConnectionString);
            //retVal.Open();
            return retVal;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override System.Data.Common.DbParameter CreateParameter(int i, object value)
        {
            return new SqlParameter("@" + i, value);
        }

        public override DbParameter CreateParameter(string parameterName, object value)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException("parameterName");

            if (value == null)
                value = DBNull.Value;

            SqlParameter param = new SqlParameter(string.Format("@{0}", parameterName), value);
            return param;
        }
    }
}

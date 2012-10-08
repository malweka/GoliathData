using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Mapping;

namespace Goliath.Data.DataAccess
{
    class MappedStatementRunner
    {
        public T RunStatement<T>(ISession session, string statementName, params QueryParam[] paramArray)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            StatementMap statement;
            if (!session.SessionFactory.DbSettings.Map.MappedStatements.TryGetValue(statementName, out statement))
            {
                throw new MappingException(statementName + " was not found. Verified if it is mapped properly.");
            }

            Console.Write("Test");

            return default(T);
        }

    }
}

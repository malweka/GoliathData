using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

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
            //does the statement needs to be parsed?
            //does the statement has input parameter and db parameters? 
            if (statement.IsParsingRequired && !statement.IsReady)
            {
                //parse it
                StatementMapParser parser = new StatementMapParser();

                statement.IsReady = true;
                //statement.Body should be equal to value of parse;
                //also store the inputparam for the parsing;
            }

            if ((statement.OperationType == MappedStatementType.ExecuteScalar) || (statement.OperationType == MappedStatementType.Query))
            {
                SqlCommandRunner runner = new SqlCommandRunner();
                var result = runner.Run<T>(session, statement.Body.Trim(), paramArray);
                return result;
            }
            else
            {
                throw new GoliathDataException(string.Format("", statementName, statement.OperationType));
            }
        }

    }
}

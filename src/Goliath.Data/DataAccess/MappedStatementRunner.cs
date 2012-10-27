﻿using System;
using System.Collections.Generic;
using System.Linq;
using Goliath.Data.Diagnostics;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class MappedStatementRunner
    {

        EntityAccessorStore EntityAccessorStore = new EntityAccessorStore();
        static ILogger logger;

        static MappedStatementRunner()
        {
            logger = Logger.GetLogger(typeof(MappedStatementRunner));
        }

        #region helper methods 

        StatementMap PrepareStatement(ISession session, string statementName, QueryParam[] paramArray, Dictionary<string, object> inObjects, params object[] inputParams)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            StatementMap statement;
            if (!session.SessionFactory.DbSettings.Map.MappedStatements.TryGetValue(statementName, out statement))
            {
                throw new MappingException("Statement " + statementName + " was not found. Please check that it is mapped properly and that it can run on this platform. Please check that the statement canRunOn support Current loaded platform: " + session.SessionFactory.DbSettings.Map.MappedStatements.Platform.Title);
            }

            if (string.IsNullOrWhiteSpace(statement.Body))
                throw new MappingException("Statement " + statementName + " body's was null or empty. Cannot run a mapped statement without SQL.");

            bool parse = statement.IsParsingRequired;          
            if (!parse)
            {
                parse = ((statement.DBParametersMap.Count > 0) || (statement.InputParametersMap.Count > 0) || ((inputParams != null) && (inputParams.Length > 0)));
            }

            if (parse && !statement.IsReady)
            {
                //parse it
                StatementMapParser parser = new StatementMapParser();

                if ((statement.InputParametersMap.Count > 1) && ((inputParams == null) || (inputParams.Length < statement.InputParametersMap.Count)))
                    throw new GoliathDataException(string.Format("{0} requires {1} input paremeters.", statement.Name, statement.InputParametersMap.Count));
                else if ((inputParams.Length > 0) && (statement.InputParametersMap.Count == 0) && !string.IsNullOrWhiteSpace(statement.DependsOnEntity))
                {
                    statement.InputParametersMap.Add("a", statement.DependsOnEntity);
                }

                int counter = 0;
                Dictionary<string, StatementInputParam> inParameters = new Dictionary<string, StatementInputParam>();


                foreach (var kpair in statement.InputParametersMap)
                {
                    inParameters.Add(kpair.Key, new StatementInputParam() { Name = kpair.Key, Type = inputParams[0].GetType().FullName, ClrType = inputParams[0].GetType() });
                    inObjects.Add(kpair.Key, inputParams[0]);
                    counter++;
                }

                var compiledStatement = parser.Parse(session.SessionFactory.DbSettings.SqlDialect, session.SessionFactory.DbSettings.Map, inParameters, statement.Body.Trim());
                ProcessCompiledStatement(compiledStatement, statement);

            }
            else
            {
                logger.Log(LogLevel.Debug, string.Format("Statement {0} do not require Parsing: IsParsingRequired={1} && IsReady={2}", statement.Name, statement.IsParsingRequired, statement.IsReady));
            }

            return statement;

        }

        StatementMap PrepareStatement<T>(ISession session, string statementName, params QueryParam[] paramArray)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            StatementMap statement;
            if (!session.SessionFactory.DbSettings.Map.MappedStatements.TryGetValue(statementName, out statement))
            {
                throw new MappingException("Statement " + statementName + " was not found. Verified if it is mapped properly.");
            }

            if (string.IsNullOrWhiteSpace(statement.Body))
                throw new MappingException("Statement " + statementName + " body's was null or empty. Cannot run a mapped statement without SQL.");


            //does the statement needs to be parsed?
            //does the statement has input parameter and db parameters? 
            bool parse = statement.IsParsingRequired;
            if (!parse)
            {
                parse = ((statement.DBParametersMap.Count > 0) || (statement.InputParametersMap.Count > 0));
            }

            if (parse && !statement.IsReady)
            {
                //parse it
                StatementMapParser parser = new StatementMapParser();
                if (statement.InputParametersMap.Count > 1)
                    throw new GoliathDataException(string.Format("{0} requires {1} input paremeters. None were provided.", statement.Name, statement.InputParametersMap.Count));

                EntityMap entityMap;
                Type type = typeof(T);
                if (!session.SessionFactory.DbSettings.Map.EntityConfigs.TryGetValue(type.FullName, out entityMap))
                {
                    entityMap = new DynamicEntityMap(type);
                }

                var compiledStatement = parser.Parse(session.SessionFactory.DbSettings.SqlDialect, entityMap, statement.Body.Trim(), null, paramArray);
                ProcessCompiledStatement(compiledStatement, statement);
            }

            return statement;
        }


        T RunStatementInternal<T>(StatementMap statement, ISession session, QueryParam[] parameters)
        {
            if ((statement.OperationType == MappedStatementType.ExecuteScalar) || (statement.OperationType == MappedStatementType.Query))
            {
                SqlCommandRunner runner = new SqlCommandRunner();
                var result = runner.Run<T>(session, statement.Body.Trim(), parameters);
                return result;
            }
            else
            {
                throw new GoliathDataException(string.Format("Operation {1} not supported on {0}. Use another Run method.", statement.Name, statement.OperationType));
            }
        }


        IList<T> RunListStatementInternal<T>(StatementMap statement, ISession session, QueryParam[] parameters)
        {
            if ((statement.OperationType == MappedStatementType.ExecuteScalar) || (statement.OperationType == MappedStatementType.Query))
            {
                SqlCommandRunner runner = new SqlCommandRunner();
                var result = runner.RunList<T>(session, statement.Body.Trim(), parameters);
                return result;
            }
            else
            {
                throw new GoliathDataException(string.Format("Operation {1} not supported on {0}. Use another Run method.", statement.Name, statement.OperationType));
            }
        }

        int RunNonQueryStatementInternal(StatementMap statement, ISession session, QueryParam[] parameters)
        {
            if (statement.OperationType > MappedStatementType.Update)
            {
                SqlCommandRunner runner = new SqlCommandRunner();
                var result = runner.RunNonQuery(session, statement.Body.Trim(), parameters);
                return result;
            }
            else
            {
                throw new GoliathDataException(string.Format("Operation {1} not supported on {0}. Use another Run method.", statement.Name, statement.OperationType));
            }
        }

        QueryParam[] BuildParameterArray(StatementMap statement, QueryParam[] paramArray, IDictionary<string, object> inputObjects)
        {
            Dictionary<string, QueryParam> dbParams = new Dictionary<string, QueryParam>();
            foreach (var p in paramArray)
            {
                dbParams.Add(p.Name, p);
            }

            if ((inputObjects != null) && (inputObjects.Count > 0))
            {

                EntityAccessor getSetInfo;

                foreach (var inParam in statement.ParamPropertyMap.Values)
                {
                    object paramObj;
                    if (inputObjects.TryGetValue(inParam.Property.VarName, out paramObj))
                    {
                        getSetInfo = EntityAccessorStore.GetEntityAccessor(inParam.ClrType, inParam.Map);

                        PropertyAccessor pInfo;
                        if (getSetInfo.Properties.TryGetValue(inParam.Property.PropName, out pInfo))
                        {
                            QueryParam dbParam = new QueryParam(inParam.QueryParamName);
                            dbParam.Value = pInfo.GetMethod(paramObj);
                            if (!dbParams.ContainsKey(inParam.QueryParamName))
                            {
                                dbParams.Add(inParam.QueryParamName, dbParam);
                            }
                        }
                    }

                }
            }

            return dbParams.Values.ToArray();
        }

        void ProcessCompiledStatement(CompiledStatement compiledStatement, StatementMap statement)
        {
            statement.Body = compiledStatement.Body;

            foreach (var keyVal in compiledStatement.ParamPropertyMap)
            {
                if (!statement.ParamPropertyMap.ContainsKey(keyVal.Key))
                {
                    statement.ParamPropertyMap.Add(keyVal.Key, keyVal.Value);
                }
            }



            statement.IsReady = true;
        }

        #endregion

        #region public methods 
        
        /// <summary>
        /// Runs the statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="paramArray">The param array.</param>
        /// <returns></returns>
        public T RunStatement<T>(ISession session, string statementName, params QueryParam[] paramArray)
        {
            if (paramArray == null)
                paramArray = new QueryParam[] { };

            var statement = PrepareStatement<T>(session, statementName, paramArray);
            var parameters = BuildParameterArray(statement, paramArray, new Dictionary<string, object>() { });
            return RunStatementInternal<T>(statement, session, parameters);
        }



        /// <summary>
        /// Runs the statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="paramArray">The param array.</param>
        /// <param name="inputParams">The input params.</param>
        /// <returns></returns>
        public T RunStatement<T>(ISession session, string statementName, QueryParam[] paramArray, params object[] inputParams)
        {
            if (paramArray == null)
                paramArray = new QueryParam[] { };

            Dictionary<string, object> inObjects = new Dictionary<string, object>();
            var statement = PrepareStatement(session, statementName, paramArray, inObjects, inputParams);
            var parameters = BuildParameterArray(statement, paramArray, inObjects);
            return RunStatementInternal<T>(statement, session, parameters);
        }

        public int RunNonQueryMappedStatement(ISession session, string statementName, params QueryParam[] paramArray)
        {
            if (paramArray == null)
                paramArray = new QueryParam[] { };

            var statement = PrepareStatement<object>(session, statementName, paramArray);
            var parameters = BuildParameterArray(statement, paramArray, new Dictionary<string, object>() { });
            return RunNonQueryStatementInternal(statement, session, parameters);
        }

        public int RunNonQueryMappedStatement(ISession session, string statementName, QueryParam[] paramArray, params object[] inputParams)
        {
            if (paramArray == null)
                paramArray = new QueryParam[] { };

            Dictionary<string, object> inObjects = new Dictionary<string, object>();
            var statement = PrepareStatement(session, statementName, paramArray, inObjects, inputParams);
            var parameters = BuildParameterArray(statement, paramArray, inObjects);
            return RunNonQueryStatementInternal(statement, session, parameters);
        }

        /// <summary>
        /// Runs the list statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="paramArray">The param array.</param>
        /// <returns></returns>
        public IList<T> RunListStatement<T>(ISession session, string statementName, params QueryParam[] paramArray)
        {
            if (paramArray == null)
                paramArray = new QueryParam[] { };

            var statement = PrepareStatement<T>(session, statementName, paramArray);
            var parameters = BuildParameterArray(statement, paramArray, new Dictionary<string, object>() { });
            return RunListStatementInternal<T>(statement, session, parameters);
        }

        /// <summary>
        /// Runs the list statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="paramArray">The param array.</param>
        /// <param name="inputParams">The input params.</param>
        /// <returns></returns>
        public IList<T> RunListStatement<T>(ISession session, string statementName, QueryParam[] paramArray, params object[] inputParams)
        {
            if (paramArray == null)
                paramArray = new QueryParam[] { };

            Dictionary<string, object> inObjects = new Dictionary<string, object>();
            var statement = PrepareStatement(session, statementName, paramArray, inObjects, inputParams);
            var parameters = BuildParameterArray(statement, paramArray, inObjects);
            return RunListStatementInternal<T>(statement, session, parameters);
        }

        #endregion

    }
}

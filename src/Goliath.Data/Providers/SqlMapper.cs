using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Goliath.Data.Mapping;
using Goliath.Data.Sql;

namespace Goliath.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public abstract class SqlMapper
    {

        Dictionary<string, DbTypeInfo> typeMap = new Dictionary<string, DbTypeInfo>();
        //Dictionary<SqlStatement, string> statements = new Dictionary<SqlStatement, string>(Utils.EnumComparer<SqlStatement>.Instance);
        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<string, ISqlFunction> functionMap = new Dictionary<string, ISqlFunction>();
        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<string, string> translationTypeMap;// = new Dictionary<string, string>();
        List<string> reservedWords = new List<string>();

        /// <summary>
        /// Gets the name of the database provider.
        /// </summary>
        /// <value>
        /// The name of the database provider.
        /// </value>
        public string DatabaseProviderName { get; private set; }

        object lockTypeMap = new object();
        object lockFunctionMap = new object();
        object lockTransmap = new object();
        bool canTranslate;

       // public bool SupportsIdentityColumn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlMapper"/> class.
        /// </summary>
        /// <param name="dbProviderName">Name of the db provider.</param>
        protected SqlMapper(string dbProviderName)
        {
            SupportIdentityColumns = true;
            RegisterType(DbType.Boolean, "bit");
            RegisterType(DbType.Int32, "int");
            RegisterType(DbType.Int32, "integer");
            RegisterType(DbType.Int32, "numeric");
            RegisterType(DbType.AnsiStringFixedLength, 8000, "char");
            RegisterType(DbType.AnsiString, 8000, "varchar");
            RegisterType(DbType.StringFixedLength, 4000, "nchar");
            RegisterType(DbType.String, 4000, "nvarchar");
            RegisterType(DbType.Binary, 8000, "varbinary");
            RegisterType(DbType.String, "text");
            RegisterType(DbType.Single, "real");
            RegisterType(DbType.Time, "time");
            RegisterType(DbType.Decimal, "decimal");
            RegisterType(DbType.DateTime, "datetime");
            RegisterType(DbType.Date, "date");
            RegisterType(DbType.Guid, "uniqueidentifier");
            OnRegisterTypes();

            OnRegisterFunctions();

            DatabaseProviderName = dbProviderName;
        }


        /// <summary>
        /// Called when [register types].
        /// </summary>
        protected virtual void OnRegisterTypes()
        {            
        }

        /// <summary>
        /// Called when [register translation types].
        /// </summary>
        protected virtual void OnRegisterTranslationTypes()
        {            
        }

        /// <summary>
        /// Called when [register functions].
        /// </summary>
        protected virtual void OnRegisterFunctions()
        {
            RegisterFunctions(new CountFunction());
        }

        /// <summary>
        /// Creates the name of the parameter.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns></returns>
        public virtual string CreateParameterName(string variableName)
        {
            return string.Format("@{0}", variableName);
        }

        /// <summary>
        /// Registers the reserved words.
        /// </summary>
        /// <param name="wordList">The word list.</param>
        public virtual void RegisterReservedWords(IEnumerable<string> wordList)
        {
            foreach (string word in wordList)
            {
                reservedWords.Add(word);
            }
        }

        /// <summary>
        /// Gets the type of the CLR.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <param name="isNullable">if set to <c>true</c> [is nullable].</param>
        /// <returns></returns>
        public virtual Type GetClrType(DbType dbType, bool isNullable)
        {
            return SqlTypeHelper.GetClrType(dbType, isNullable);
        }

        void LoadSqlStringType()
        {
            if (!canTranslate)
            {
                translationTypeMap = new Dictionary<string, string>();
                canTranslate = true;

                RegisterTranslateType("integer", "integer");
                RegisterTranslateType("int", "integer");
                RegisterTranslateType("char", "char");
                RegisterTranslateType("nvarchar", "nvarchar");
                RegisterTranslateType("nchar", "nchar");
                RegisterTranslateType("varchar", "varchar");
                RegisterTranslateType("text", "text");
                RegisterTranslateType("numeric", "numeric");
                RegisterTranslateType("date", "date");
                OnRegisterTranslationTypes();
            }
        }

        /// <summary>
        /// Translates the type of the SQL.
        /// </summary>
        /// <param name="fromType">From type.</param>
        /// <returns></returns>
        public string TranslateToSqlStringType(Property fromType)
        {
            if (fromType == null)
                throw new ArgumentNullException("fromType");

            LoadSqlStringType();
            

            return OnTranslateToSqlTypeString(fromType);
        }

        /// <summary>
        /// Prints the SQL type string.
        /// </summary>
        /// <param name="fromType">From type.</param>
        /// <returns></returns>
        public virtual string PrintSqlTypeString(Property fromType)
        {
            if (fromType == null)
                throw new ArgumentNullException("fromType");

            LoadSqlStringType();

            StringBuilder sqlSb = new StringBuilder();
            
            string to = null;
            string fType = fromType.SqlType.ToLower();
            if (!string.IsNullOrWhiteSpace(fType))
            {
                translationTypeMap.TryGetValue(fType, out to);
                if ((fromType.Length > 0) && !fType.Equals("text") && !fType.Equals("ntext") && !fType.Equals("image"))
                {
                    if (!string.IsNullOrWhiteSpace(to) && !to.ToUpper().Equals("NTEXT"))
                    {
                        to = string.Format("{0}({1})", to, fromType.Length);
                    }
                }
            }

            var sType = to ?? fromType.SqlType;
            sqlSb.AppendFormat(" {0}", sType);

            return sqlSb.ToString();

        }

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <param name="functionName">Name of the function. Check <see cref="Goliath.Data.Sql.FunctionNames"/> class for list of default functions names</param>
        /// <returns>returns <see cref="Goliath.Data.Sql.ISqlFunction"/> or null if not found</returns>
        public ISqlFunction GetFunction(string functionName)
        {
            ISqlFunction func;
            functionMap.TryGetValue(functionName, out func);
            return func;
        }

        /// <summary>
        /// Called when [translate to SQL type string].
        /// </summary>
        /// <param name="fromType">From type.</param>
        /// <returns></returns>
        protected virtual string OnTranslateToSqlTypeString(Property fromType)
        {
            string to = null;
            string fType = fromType.SqlType.ToLower();
            if (!string.IsNullOrWhiteSpace(fType))
            {
                translationTypeMap.TryGetValue(fType, out to);

                if (to == null)
                    to = fType;

                if ((fromType.Length > 0) && !fType.Equals("text") && !fType.Equals("ntext") && !fType.Equals("image"))
                {
                    if (!string.IsNullOrWhiteSpace(to) && to.ToUpper().Equals("BLOB"))
                    {
                        return to;
                    }
                    to = string.Format("{0}({1})", to, fromType.Length);
                }
            }

            return to;
        }

        #region Sql Types

        /// <summary>
        /// Registers the functions.
        /// </summary>
        /// <param name="sqlFunction">The SQL function.</param>
        protected void RegisterFunctions(ISqlFunction sqlFunction)
        {
            if (sqlFunction == null)
                throw new ArgumentNullException("sqlFunction");

            ISqlFunction func;
            lock (lockFunctionMap)
            {
                if (functionMap.TryGetValue(sqlFunction.Name, out func))
                {
                    functionMap.Remove(sqlFunction.Name);
                }

                functionMap.Add(sqlFunction.Name, sqlFunction);
            }
        }

        /// <summary>
        /// Translates the type.
        /// </summary>
        /// <param name="fromType">From type.</param>
        /// <param name="toType">To type.</param>
        protected void RegisterTranslateType(string fromType, string toType)
        {
            if (!canTranslate)
                throw new GoliathDataException("Register translate type can only be called from within OnRegisterTranslationTypes");
            
            string to;
            fromType = fromType.ToLower();
            lock (lockTransmap)
            {
                if (translationTypeMap.TryGetValue(fromType, out to))
                {
                    translationTypeMap.Remove(fromType);
                }

                translationTypeMap.Add(fromType, toType);
            }
        }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="capacity">The capacity.</param>
        /// <param name="sqlText">The SQL text.</param>
        protected void RegisterType(DbType type, int capacity, string sqlText)
        {
            DbTypeInfo tinfo;

            lock (lockTypeMap)
            {
                if (!typeMap.TryGetValue(sqlText, out tinfo))
                {
                    tinfo = new DbTypeInfo(type, sqlText) { Length = capacity };
                    typeMap.Add(sqlText, tinfo);
                }
                else
                {
                    tinfo.Length = capacity;
                    tinfo.DbType = type;
                }
            }
        }

        ///// <summary>
        ///// Registers the type.
        ///// </summary>
        ///// <param name="type">The type.</param>
        ///// <param name="precision">The precision.</param>
        ///// <param name="scale">The scale.</param>
        ///// <param name="sqlText">The SQL text.</param>
        //protected void RegisterType(DbType type, int precision, int scale, string sqlText)
        //{
        //    DbTypeInfo tinfo;

        //    lock (lockTypeMap)
        //    {
        //        if (!typeMap.TryGetValue(sqlText, out tinfo))
        //        {
        //            tinfo = new DbTypeInfo(type, sqlText) { Precision = precision, Scale = scale };
        //            typeMap.Add(sqlText, tinfo);
        //        }
        //        else
        //        {
        //            tinfo.Precision = precision;
        //            tinfo.Scale = scale;
        //            tinfo.DbType = type;
        //        }
        //    }
        //}

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="sqlText">The SQL text.</param>
        protected void RegisterType(DbType type, string sqlText)
        {
            RegisterType(type, 0, sqlText);
        }

        //TODO: create rule class for type that translate to something based on on precision and scale
        /// <summary>
        /// SQLs the type of the string to db.
        /// </summary>
        /// <param name="sqlType">Type of the SQL.</param>
        /// <returns></returns>
        public virtual DbType SqlStringToDbType(string sqlType)
        {
            DbTypeInfo dbInfo;
            
            if (typeMap.TryGetValue(sqlType, out dbInfo))
            {
                return dbInfo.DbType;
            }

            throw new Exception(string.Format("could not find {0}. This type was not be registered", sqlType));
        }

        /// <summary>
        /// Primary key 
        /// </summary>
        /// <returns></returns>
        public virtual string PrimarykeySql()
        {
            return "primary key";
        }

        /// <summary>
        /// Foreigns the key reference SQL.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <returns></returns>
        public virtual string ForeignKeyReferenceSql(Relation reference)
        {
            return string.Format("references {0} ({1}) ", reference.ReferenceTable, reference.ReferenceColumn);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [support identity columns].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [support identity columns]; otherwise, <c>false</c>.
        /// </value>
        public bool SupportIdentityColumns { get; set; }

        /// <summary>
        /// Identities the SQL.
        /// </summary>
        /// <param name="increment">The increment.</param>
        /// <param name="seed">The seed.</param>
        /// <returns></returns>
        public abstract string IdentitySql(int increment, int seed);

        /// <summary>
        /// Selects the last insert row id SQL.
        /// </summary>
        /// <returns></returns>
        public abstract string SelectLastInsertRowIdSql();

        /// <summary>
        /// Gets the column create SQL.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public virtual string GetColumnCreateSql(Property column)
        {
            StringBuilder builder = new StringBuilder(column.ColumnName);
            //builder.Append(DbTypeToSqlString(column.Type));
            //if (column.Length > 0)
            //    builder.AppendFormat("({0} ", column.Length);
            //if (column.IsPrimaryKey)
            //    builder.Append(PrimarykeySql());
            //if (!column.AllowNull)
            //    builder.Append(" not null");
            //if (column.IsUnique)
            //    builder.Append(" unique");            
            //if (column.DefaultValue != null)
            //    builder.AppendFormat(" default({0}) ", column.DefaultValue);
            //if (column.IsAutoGenerated && SupportIdentityColumns)
            //    builder.Append(IdentitySql(1, 1));
            //if (column.IsAutoGenerated && SupportIdentityColumns)
            //    builder.Append(IdentitySql(1, 1));
            //if (column.ForeignKey != null)
            //    builder.Append(ForeignKeyReferenceSql(column.ForeignKey));
            //if (column.ExtraOptions != null)
            //    builder.AppendFormat(" {0} ", column.ExtraOptions);

            return builder.ToString();

        }

        #endregion

        /// <summary>
        /// Escapes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual string Escape(string value)
        {
            return string.Format("[{0}]", value);
        }

        /// <summary>
        /// Escapes if reserve word.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string EscapeIfReserveWord(string value)
        {
            if (reservedWords.Contains(value.ToUpper()))
            {
                return Escape(value);
            }
            else
                return value;
        }

        /// <summary>
        /// Queries the with paging.
        /// </summary>
        /// <param name="queryBody">The query body.</param>
        /// <param name="pagingInfo">The paging info.</param>
        /// <returns></returns>
        public virtual string QueryWithPaging(SqlQueryBody queryBody, PagingInfo pagingInfo)
        {
            return string.Format("{0} LIMIT {1} OFFSET {2}", queryBody.ToString(), pagingInfo.Limit, pagingInfo.Offset);
        }
    }
}

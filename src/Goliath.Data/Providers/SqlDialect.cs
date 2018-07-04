using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Goliath.Data.Mapping;
using Goliath.Data.Sql;

namespace Goliath.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public abstract class SqlDialect
    {
        readonly Dictionary<string, DbTypeInfo> typeMap = new Dictionary<string, DbTypeInfo>();

        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<string, ISqlFunction> FunctionMap = new Dictionary<string, ISqlFunction>();

        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<string, string> translationTypeMap;
        protected static List<string> reservedWords = new List<string>();

        public abstract string DefaultSchemaName { get;}

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
        /// Initializes a new instance of the <see cref="SqlDialect"/> class.
        /// </summary>
        /// <param name="dbProviderName">Name of the db provider.</param>
        protected SqlDialect(string dbProviderName)
        {
            SupportIdentityColumns = true;
            StrictCaseSensitivity = false;

            RegisterType(DbType.Boolean, "bit");
            RegisterType(DbType.Int32, "int");
            RegisterType(DbType.Int32, "integer");
            RegisterType(DbType.Int32, "numeric");
            //RegisterType(DbType.Int32, "tinyint"); -- tinyint is byte
            RegisterType(DbType.AnsiStringFixedLength, 8000, "char");
            RegisterType(DbType.AnsiString, 8000, "varchar");
            RegisterType(DbType.StringFixedLength, 4000, "nchar");
            RegisterType(DbType.String, 4000, "nvarchar");
            RegisterType(DbType.Binary, 8000, "varbinary");
            RegisterType(DbType.AnsiString, "text");
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
            return $"@{variableName}";
        }

        /// <summary>
        /// Registers the reserved words.
        /// </summary>
        /// <param name="wordList">The word list.</param>
        public virtual void RegisterReservedWords(IEnumerable<string> wordList)
        {
            foreach (var word in wordList)
                reservedWords.Add(word);
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
            if (canTranslate) return;

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

        /// <summary>
        /// Translates the type of the SQL.
        /// </summary>
        /// <param name="fromType">From type.</param>
        /// <returns></returns>
        public string TranslateToSqlStringType(Property fromType)
        {
            if (fromType == null)
                throw new ArgumentNullException(nameof(fromType));

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
                throw new ArgumentNullException(nameof(fromType));

            LoadSqlStringType();

            var sqlSb = new StringBuilder();

            string to = null;
            string fType = fromType.SqlType.ToLower();
            if (!string.IsNullOrWhiteSpace(fType))
            {
                translationTypeMap.TryGetValue(fType, out to);
                if ((fromType.Length > 0) && !fType.Equals("text") && !fType.Equals("ntext") && !fType.Equals("image"))
                {
                    if (!string.IsNullOrWhiteSpace(to) && !to.ToUpper().Equals("NTEXT"))
                    {
                        to = $"{to}({fromType.Length})";
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
            FunctionMap.TryGetValue(functionName, out func);
            return func;
        }

        /// <summary>
        /// Prints the case incensitive like.
        /// </summary>
        /// <returns></returns>
        public virtual string PrintCaseIncensitiveLike()
        {
            return "LIKE";
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
                    to = $"{to}({fromType.Length})";
                }
            }

            return to;
        }

        #region Sql Types

        /// <summary>
        /// Registers the functions.
        /// </summary>
        /// <param name="sqlFunction">The SQL function.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected void RegisterFunctions(ISqlFunction sqlFunction)
        {
            if (sqlFunction == null)
                throw new ArgumentNullException(nameof(sqlFunction));

            lock (lockFunctionMap)
            {
                ISqlFunction func;
                if (FunctionMap.TryGetValue(sqlFunction.Name, out func))
                {
                    FunctionMap.Remove(sqlFunction.Name);
                }

                FunctionMap.Add(sqlFunction.Name, sqlFunction);
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

            fromType = fromType.ToLower();
            lock (lockTransmap)
            {
                string to;
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
            lock (lockTypeMap)
            {
                DbTypeInfo tinfo;
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

            throw new Exception($"could not find {sqlType}. This type was not be registered");
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
            return $"references {reference.ReferenceTable} ({reference.ReferenceColumn}) ";
        }

        /// <summary>
        /// Gets or sets a value indicating whether [support identity columns].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [support identity columns]; otherwise, <c>false</c>.
        /// </value>
        public bool SupportIdentityColumns { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [strict case sensitivity].
        /// </summary>
        /// <value>
        /// <c>true</c> if [strict case sensitivity]; otherwise, <c>false</c>.
        /// </value>
        public bool StrictCaseSensitivity { get; protected set; }

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
            var builder = new StringBuilder(column.ColumnName);
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
        /// Builds the insert statement.
        /// </summary>
        /// <param name="insertStatement">The insert statement.</param>
        /// <returns></returns>
        public virtual string BuildInsertStatement(InsertSqlInfo insertStatement)
        {
            var sb = new StringBuilder("INSERT INTO ");
            sb.AppendFormat("{0} (", this.Escape(insertStatement.TableName, EscapeValueType.TableName));
            int countChecker = insertStatement.Columns.Count - 1;
            int counter = 0;

            foreach (var column in insertStatement.Columns.Values)
            {
                sb.Append(Escape(column, EscapeValueType.Column));

                if (counter < countChecker)
                {
                    sb.Append(", ");
                }
                counter++;
            }
            sb.Append(") VALUES(");

            counter = 0;
            foreach (var param in insertStatement.Parameters.Values)
            {
                sb.Append(CreateParameterName(param.Name));
                if (counter < countChecker)
                    sb.Append(", ");
                counter++;
            }
            sb.Append(")");
            foreach (var keygen in insertStatement.DbKeyGenerateSql)
            {
                sb.AppendFormat("{0};\n", keygen);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds the update statement.
        /// </summary>
        /// <param name="updateStatement">The update statement.</param>
        /// <returns></returns>
        /// <exception cref="GoliathDataException">Couldn't not build update statement. Please verify that you have matchin parameters for all your columns.</exception>
        public virtual string BuildUpdateStatement(UpdateSqlBodyInfo updateStatement)
        {
            var sb = new StringBuilder("UPDATE ");
            sb.AppendFormat("{0} SET ", Escape(updateStatement.TableName, EscapeValueType.TableName));
            try
            {
                var counter = 0;
                foreach (var col in updateStatement.Columns)
                {
                    sb.AppendFormat("{0} = {1}", Escape(col.Key, EscapeValueType.Column), CreateParameterName(col.Value.Item1.Name));
                    if (counter < (updateStatement.Columns.Count - 1))
                        sb.Append(", ");

                    counter = counter + 1;
                }
            }
            catch (Exception exception)
            {
                throw new GoliathDataException("Couldn't not build update statement. Please verify that you have matchin parameters for all your columns.", exception);
            }

            sb.AppendFormat(" WHERE {0}", updateStatement.WhereExpression);
            return sb.ToString();
        }

        /// <summary>
        /// Builds the delete statement.
        /// </summary>
        /// <param name="deleteStatement">The delete statement.</param>
        /// <returns></returns>
        public virtual string BuildDeleteStatement(DeleteSqlBodyInfo deleteStatement)
        {
            var sb = new StringBuilder("DELETE FROM ");
            sb.AppendFormat("{0} ", Escape(deleteStatement.TableName, EscapeValueType.TableName));
            sb.AppendFormat("WHERE {0}", deleteStatement.WhereExpression);
            return sb.ToString();
        }

        /// <summary>
        /// Escapes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual string Escape(string value)
        {
            return Escape(value, EscapeValueType.GenericString);
        }

        /// <summary>
        /// Escapes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="escapeValueType">Type of the escape value.</param>
        /// <returns></returns>
        public virtual string Escape(string value, EscapeValueType escapeValueType)
        {
            return $"[{value}]";
        }

        /// <summary>
        /// Escapes if reserve word.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string EscapeIfReserveWord(string value)
        {
            return IsReservedWord(value.ToUpper()) ? Escape(value) : value;
        }

        /// <summary>
        /// Determines whether [is reserved word] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool IsReservedWord(string value)
        {
            return reservedWords.Contains(value.ToUpper());
        }

        /// <summary>
        /// Queries the with paging.
        /// </summary>
        /// <param name="queryBody">The query body.</param>
        /// <param name="pagingInfo">The paging info.</param>
        /// <returns></returns>
        public virtual string QueryWithPaging(SqlQueryBody queryBody, PagingInfo pagingInfo)
        {
            return $"{queryBody} LIMIT {pagingInfo.Limit} OFFSET {pagingInfo.Offset}";
        }

        /// <summary>
        /// Prints the color type to string.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="nullable">if set to <c>true</c> [nullable].</param>
        /// <returns></returns>
        public static string PrintClrTypeToString(Type type, bool nullable = false)
        {
            string print = type.Name;

            if (type.IsPrimitive || print.Equals("String") || print.Equals("Boolean"))
            {
                switch (type.Name.ToLower())
                {
                    case "int32":
                        print = "int";
                        break;
                    case "int64":
                        print = "long";
                        break;
                    case "int16":
                        print = "short";
                        break;
                    case "boolean":
                        print = "bool";
                        break;
                    case "single":
                        print = "float";
                        break;
                    default:
                        print = type.Name.ToLower();
                        break;
                }
            }

            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var args = type.GetGenericArguments();
                return PrintClrTypeToString(args[0], true);
            }

            return nullable ? $"{print}?" : print;
        }

        /// <summary>
        /// Gets the value as SQL string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="prop">The property.</param>
        /// <returns></returns>
        public virtual string GetValueAsSqlString(object value, Property prop)
        {
            if (value == null)
                return "NULL";

            switch (prop.DbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return $"'{value.ToString().Replace("'", "''")}'";
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Xml:
                    return $"N'{value.ToString().Replace("'", "''")}'";
                case DbType.Int32:
                case DbType.Int16:
                case DbType.Int64:
                case DbType.Decimal:
                case DbType.Single:
                case DbType.Currency:
                case DbType.Binary:
                case DbType.Byte:
                case DbType.Double:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.SByte:
                case DbType.VarNumeric:
                    return value.ToString();
                case DbType.Boolean:
                    bool boolVal;
                    bool.TryParse(value.ToString(), out boolVal);
                    return boolVal ? "1" : "0";
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                    var datetime = (DateTime)value;
                    if (DateTime.MinValue.Equals(datetime))
                        return "NULL";
                    var dateString = datetime.ToString("yyyy-MM-dd HH:mm:ss");
                    return $"'{dateString}'";
                case DbType.DateTimeOffset:
                    var date = (DateTimeOffset) value;
                    return date.ToString("O");
                case DbType.Guid:
                case DbType.Object:
                case DbType.Time:
                    return $"'{value}'";
                default:
                    return $"'{value}'";
            }

        }
    }
}

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
    public abstract class SqlMapper
    {

        Dictionary<string, DbTypeInfo> typeMap = new Dictionary<string, DbTypeInfo>();
        //Dictionary<SqlStatement, string> statements = new Dictionary<SqlStatement, string>(Utils.EnumComparer<SqlStatement>.Instance);
        Dictionary<string, ISqlFunction> functionMap = new Dictionary<string, ISqlFunction>();
        Dictionary<string, string> translationTypeMap;// = new Dictionary<string, string>();
        List<string> reservedWords = new List<string>();
       
        public string DatabaseProviderName { get; private set; }

        object lockTypeMap = new object();
        object lockFunctionMap = new object();
        object lockTransmap = new object();
        bool canTranslate;

       // public bool SupportsIdentityColumn { get; set; }

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
        

        protected virtual void OnRegisterTypes()
        {            
        }

        protected virtual void OnRegisterTranslationTypes()
        {            
        }

        protected virtual void OnRegisterFunctions()
        { }

        public virtual string CreateParameterName(string variableName)
        {
            return string.Format("@{0}", variableName);
        }

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
        /// <returns></returns>
        public virtual Type GetClrType(DbType dbType, bool isNullable)
        {
            return SqlTypeHelper.GetClrType(dbType, isNullable);
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

            if (!canTranslate)
            {
                translationTypeMap = new Dictionary<string, string>();
                canTranslate = true;

                RegisterTranslateType("integer", "integer");
                RegisterTranslateType("char", "char");
                RegisterTranslateType("nvarchar", "nvarchar");
                RegisterTranslateType("nchar", "nchar");
                RegisterTranslateType("varchar", "varchar");
                RegisterTranslateType("text", "text");
                RegisterTranslateType("numeric", "numeric");
                RegisterTranslateType("date", "date");
                OnRegisterTranslationTypes(); 
            }

            return OnTranslateToSqlTypeString(fromType);
        }

        /// <summary>
        /// Gets the function.
        /// </summary>
        /// <param name="declaration">The declaration.</param>
        /// <returns></returns>
        public ISqlFunction GetFunction(string declaration)
        {
            ISqlFunction func;
            functionMap.TryGetValue(declaration, out func);
            return func;
        }

        protected virtual string OnTranslateToSqlTypeString(Property fromType)
        {
            string to = null;
            string fType = fromType.SqlType.ToLower();
            if (!string.IsNullOrWhiteSpace(fType))
            {
                translationTypeMap.TryGetValue(fType, out to);
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

                translationTypeMap.Add(fromType, toType.ToUpper());
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
        public virtual DbType SqlStringToDbType(string sqlType)
        {
            DbTypeInfo dbInfo;
            
            if (typeMap.TryGetValue(sqlType, out dbInfo))
            {
                return dbInfo.DbType;
            }

            throw new Exception(string.Format("could not find {0}. This type was not be registered", sqlType));
        }

        public virtual string PrimarykeySql()
        {
            return "primary key";
        }

        public virtual string ForeignKeyReferenceSql(Relation reference)
        {
            return string.Format("references {0} ({1}) ", reference.ReferenceTable, reference.ReferenceColumn);
        }

        public bool SupportIdentityColumns { get; set; }

        public abstract string IdentitySql(int increment, int seed);

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

        public virtual string Escape(string value)
        {
            return string.Format("[{0}]", value);
        }

        public string EscapeIfReserveWord(string value)
        {
            if (reservedWords.Contains(value.ToUpper()))
            {
                return Escape(value);
            }
            else
                return value;
        }
    }
}

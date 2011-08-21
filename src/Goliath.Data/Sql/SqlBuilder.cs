﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Goliath.Data.Providers;
using Goliath.Data.Mapping;

namespace Goliath.Data.Sql
{
    abstract class SqlBuilder
    {
        protected SqlMapper sqlMapper;
        protected EntityMap entMap;
        readonly Dictionary<string, string> columns = new Dictionary<string, string>();
        internal Dictionary<string, string> Columns
        {
            get { return columns; }
        }

        protected SqlBuilder(SqlMapper sqlMapper, EntityMap entMap)
        {
            if (sqlMapper == null)
                throw new ArgumentNullException("sqlMapper");

            if (entMap == null)
                throw new ArgumentNullException("entMap");

            this.sqlMapper = sqlMapper;
            this.entMap = entMap;
        }

        public abstract string ToSqlString();

        public override string ToString()
        {
#if DEBUG
            return base.ToString();
#else 
            return Build();
#endif
        }

    }
}

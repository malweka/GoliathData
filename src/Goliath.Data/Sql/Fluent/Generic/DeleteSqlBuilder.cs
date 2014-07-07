using System.Collections.Generic;
using Goliath.Data.DataAccess;
using Goliath.Data.Mapping;
using Goliath.Data.Utils;

namespace Goliath.Data.Sql
{
    public class DeleteSqlBuilder<T> : NonQuerySqlBuilderBase<T>
    {
        public DeleteSqlBuilder(ISession session, EntityMap entityMap, T entity) : base(session, entityMap, entity) { }
        public DeleteSqlBuilder(ISession session, T entity) : base(session, entity) { }

        void LoadInfos(EntityMap entityMap, DeleteSqlExecutionList executionList)
        {
            var deleteInfo = new DeleteSqlBodyInfo() { TableName = entityMap.TableName };
            executionList.Statements.Add(entityMap.FullName, deleteInfo);

            if (entityMap.IsSubClass)
            {
                var parentMap = session.SessionFactory.DbSettings.Map.GetEntityMap(entityMap.Extends);
                LoadInfos(parentMap, executionList);
            }
        }

        internal DeleteSqlExecutionList Build()
        {
            var execList = new DeleteSqlExecutionList();
            var store = new EntityAccessorStore();
            var accessor = store.GetEntityAccessor(entityType, Table);
            var dialect = session.SessionFactory.DbSettings.SqlDialect;
            LoadInfos(Table, execList);

            var whereExpression = BuildWhereExpression(dialect);
            foreach (var deleteSqlBodyInfo in execList.Statements)
            {
                deleteSqlBodyInfo.Value.WhereExpression = whereExpression;
            }

            return execList;
        }

        public override int Execute()
        {
            var execList = Build();
            int total = 0;
            var runner = new SqlCommandRunner();
            var dialect = session.SessionFactory.DbSettings.SqlDialect;

            foreach (var del in execList.Statements.Values)
            {
                var parameters = new List<QueryParam>();
                parameters.AddRange(whereParameters);
                total += runner.ExecuteNonQuery(session, del.ToString(dialect), parameters.ToArray());
            }

            return total;
        }
    }
}
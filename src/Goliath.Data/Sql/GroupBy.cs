
namespace Goliath.Data.Sql
{
    class GroupBy : SqlStatement
    {
        public string Column { get; private set; }

        public GroupBy(string column)
        {
            Column = column;
        }
    }
}

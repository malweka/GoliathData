
namespace Goliath.Data.Sql
{
    class OrderBy : SqlStatement
    {
        public string Column { get; private set; }
        public SortType SortDirection { get; private set; }

        public OrderBy(string columnName)
        {
            Column = columnName;
            SortDirection = SortType.Ascending;
        }

        public void Sort(SortType sortType)
        {
            SortDirection = sortType;
            //return this;
        }
    }
}

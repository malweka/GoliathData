using System.Data;

namespace Goliath.Data
{
    class DbTypeInfo
    {
        public DbType DbType { get; set; }
        public int Length { get; set; }
        public string Text { get; set; }

        public DbTypeInfo(DbType type, string text)
        {
            DbType = type;
            Text = text;
        }
    }
}

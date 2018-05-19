namespace Goliath.Data.Mapping
{
    public class TableDataMigrationDefinition
    {
        public string EntityName { get; set; }
        public string QueryFilter { get; set; }
        public string PreExecuteStatements { get; set; }
        public string PostExecuteStatements { get; set; }
    }
}
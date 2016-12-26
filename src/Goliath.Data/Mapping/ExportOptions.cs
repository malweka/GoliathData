namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public class ExportOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether [export database generated columns].
        /// </summary>
        /// <value>
        /// <c>true</c> if [export database generated columns]; otherwise, <c>false</c>.
        /// </value>
        public bool ExportDatabaseGeneratedColumns { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [export identity column].
        /// </summary>
        /// <value>
        /// <c>true</c> if [export identity column]; otherwise, <c>false</c>.
        /// </value>
        public bool ExportIdentityColumn { get; set; }
    }
}
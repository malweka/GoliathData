using System.Collections.Generic;

namespace Goliath.Data.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public class TableMetaData
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public long? Id { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets the column metadata.
        /// </summary>
        /// <value>
        /// The column metadata.
        /// </value>
        public Dictionary<string, string> ColumnMetadata { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableMetaData" /> class.
        /// </summary>
        public TableMetaData()
        {
            ColumnMetadata = new Dictionary<string, string>();
        }
    }
}
using System;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PrimaryKeyProperty //: ISerializable
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        [DataMember]
        public Property Key { get; internal set; }
        /// <summary>
        /// Gets or sets the key generation strategy.
        /// </summary>
        /// <value>The key generation strategy.</value>
        [DataMember]
        public string KeyGenerationStrategy { get; set; }
        /// <summary>
        /// Gets or sets the unsaved value.
        /// </summary>
        /// <value>The unsaved value.</value>
        [DataMember]
        public string UnsavedValue { get; set; }

        /// <summary>
        /// Gets or sets the key generator.
        /// </summary>
        /// <value>The key generator.</value>
        public IKeyGenerator KeyGenerator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKeyProperty"/> class.
        /// </summary>
        public PrimaryKeyProperty() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKeyProperty"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="keygenStrategy">The keygen strategy.</param>
        public PrimaryKeyProperty(Property property, string keygenStrategy)
        {
            Key = property;
            KeyGenerationStrategy = keygenStrategy;
        }

        internal string GetQueryName(EntityMap map)
        {
            if (Key == null)
                return string.Empty;

            if (map == null)
                throw new ArgumentNullException("map");

            return string.Format("{0}_{1}", map.TableAlias, Key.ColumnName);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Goliath.Data.Mapping.Property"/> to <see cref="Goliath.Data.Mapping.PrimaryKeyProperty"/>.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator PrimaryKeyProperty(Property property)
        {
            return new PrimaryKeyProperty(property, null);
        }

    }
}

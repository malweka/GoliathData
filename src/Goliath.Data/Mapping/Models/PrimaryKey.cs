using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    //TODO: key generation algorithm
    [Serializable]
    [DataContract]
    public class PrimaryKey
    {
        /// <summary>
        /// Gets the keys.
        /// </summary>
        [DataMember]
        public IList<PrimaryKeyProperty> Keys { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKey"/> class.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public PrimaryKey(params PrimaryKeyProperty[] keys)
        {
            if (keys != null)
            {
                var list = new List<PrimaryKeyProperty>(keys.Length);
                list.AddRange(keys);
                Keys = list;
            }
            else
                Keys = new List<PrimaryKeyProperty>();
        }

        internal PrimaryKey(IList<Property> keys)
        {

            var list = new List<PrimaryKeyProperty>();
            foreach (var k in keys)
            {
                list.Add(k);
            }
            Keys = new System.Collections.ObjectModel.ReadOnlyCollection<PrimaryKeyProperty>(list);

        }
    }

    public interface IKeyGenerator
    {
        string Name { get; }
        Object GenerateKey();
    }

    [Serializable]
    public class PrimaryKeyProperty
    {
        [DataMember]
        public Property Key { get; set; }
        [DataMember]
        public string KeyGenerationStrategy { get; set; }

        public PrimaryKeyProperty() { }
        public PrimaryKeyProperty(Property property, string keygenStrategy)
        {
            Key = property;
            KeyGenerationStrategy = keygenStrategy;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Goliath.Data.Mapping.Property"/> to <see cref="Goliath.Data.Mapping.PrimaryKeyProperty"/>.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator PrimaryKeyProperty(Property property)
        {
            return new PrimaryKeyProperty(property, null);
        }
    }
}

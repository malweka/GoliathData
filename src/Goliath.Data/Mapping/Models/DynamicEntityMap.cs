using System;
using System.Reflection;

namespace Goliath.Data.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicEntityMap : EntityMap
    {
        Type type;

        /// <summary>
        /// Gets the type of the mapping.
        /// </summary>
        /// <value>
        /// The type of the mapping.
        /// </value>
        public Type MappingType { get { return type; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEntityMap"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DynamicEntityMap(Type type) : this(null, type.Name, type) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEntityMap"/> class.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="type">The type.</param>
        public DynamicEntityMap(string tableAlias, string tableName, Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            this.type = type;
            BuildMap(tableAlias, tableName);
        }

        void BuildMap(string tableAlias, string tableName)
        {
            Name = type.Name;
            Namespace = type.Namespace;
            TableName = tableName;
            TableAlias = tableAlias;

            var PropertyAccessors = MappingType.GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
            
            foreach (var prop in PropertyAccessors)
            {
                var mProp = new Property()
                {
                    ClrType = prop.PropertyType,
                    ColumnName = prop.Name,
                    PropertyName = prop.Name,
                };

                Add(mProp);
            }
        }
    }
}

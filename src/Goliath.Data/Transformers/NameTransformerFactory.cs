using System;
using Goliath.Data.Mapping;

namespace Goliath.Data.Transformers
{
    /// <summary>
    /// name transformers factory
    /// </summary>
    public class NameTransformerFactory
    {
        ProjectSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameTransformerFactory"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public NameTransformerFactory(ProjectSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Gets the transformer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public INameTransformer<T> GetTransformer<T>() where T : class, Mapping.IMapModel
        {
            Type t = typeof(T);
            if (t == typeof(EntityMap))
            {
                return new DefaultTableEntityNameTransformer(settings.TablePrefixes) as INameTransformer<T>;
            }
            else if (t == typeof(Relation))
            {
                return new OneToManyRelationNameTransformer() as INameTransformer<T>;
            }
            else if (t == typeof(Property))
            {
                return new ColumnPropertyNameTransformer() as INameTransformer<T>;
            }
            else
                return null;
        }
    }
}

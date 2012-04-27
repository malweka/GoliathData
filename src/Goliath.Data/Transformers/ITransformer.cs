using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Transformers
{
    /// <summary>
    /// Name transformer. Used to for transforming named of IMapModel implementations
    /// </summary>
    /// <typeparam name="TFor">The type of for.</typeparam>
    public interface INameTransformer<TFor> where TFor : class, Mapping.IMapModel
    {
        string Transform(TFor mapModel, string original);
    }
}

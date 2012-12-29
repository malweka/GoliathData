using System.Collections.Generic;
using Goliath.Data.Diagnostics;

namespace Goliath.Data.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TrackableList<T> : BaseTrackableCollection<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackableList&lt;T&gt;"/> class.
        /// </summary>
        public TrackableList()
        {
            //by default we're tracking
            IsTracking = true;
        }
    }
}

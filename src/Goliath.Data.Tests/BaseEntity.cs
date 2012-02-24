using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebZoo.Data
{
    public abstract class BaseEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public virtual Guid Id { get; set; }
    }

    public abstract class BaseEntityInt
    {
        public virtual int Id { get; set; }
    }
}

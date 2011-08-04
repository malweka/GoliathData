using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebZoo.Data
{
    public abstract class BaseEntity
    {
        public virtual Guid Id { get; set; }
        public void MethodIsParent() { }
    }
}

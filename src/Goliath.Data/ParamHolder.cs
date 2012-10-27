using System;

namespace Goliath.Data
{
    internal class ParamHolder : QueryParam
    {
        public bool IsNullable { get; set; }
        public Func<object,object> Getter { get; private set; }
        public Object Entity { get; private set; }

        public ParamHolder(string name, Func<object, object> getter, Object entity)
            : base(name)
        {
            Getter = getter;
            IsNullable = true;
            Entity = entity;
        }

        public override object Value
        {
            get
            {
                if (Getter == null)
                    return null;

                var val = Getter(Entity);
                if (val == null && !IsNullable)
                {
                    throw new DataAccessException("parameter {0} cannot be null", Name);
                }

                return val;
            }
            set
            {
                
            }
        }
    }
}
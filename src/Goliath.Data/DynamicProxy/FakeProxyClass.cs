using System;
using System.ComponentModel;
using Goliath.Data.Entity;

namespace Goliath.Data.DynamicProxy
{
    public class FakeProxyClass : FakeBaseProxy, ILazyObject
    {
        Type _typeToProxy;
        bool _isLoaded;
        IProxyHydrator _proxyHydrator;

        public FakeProxyClass(Type typeToProxy, IProxyHydrator proxyHydrator)
        {
            _typeToProxy = typeToProxy;
            _isLoaded = false;
            _proxyHydrator = proxyHydrator;
        }

        public override string Name
        {
            get
            {
                LoadMe();
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        public override double Age
        {
            get
            {
                LoadMe();
                return base.Age;
            }
            set
            {
                LoadMe();
                base.Age = value;
            }
        }

        void LoadMe()
        {
            if (!_isLoaded)
            {
                //load me here
                _proxyHydrator.Hydrate(this, _typeToProxy);
                _isLoaded = true;
                _proxyHydrator.Dispose();
            }
        }

        #region ILazyObject Members

        public Type ProxyOf
        {
            get { return _typeToProxy; }
        }

        public bool IsProxyLoaded
        {
            get { return _isLoaded; }
        }

        #endregion
    }

    public class FakeTrackableProxyClass : FakeBaseProxy, ILazyObject, ITrackable, INotifyPropertyChanged
    {
        Type _typeToProxy;
        bool _isLoaded;
        IChangeTracker _changeTracker;
        IProxyHydrator _proxyHydrator;

        public FakeTrackableProxyClass(Type typeToProxy, IProxyHydrator proxyHydrator)
        {
            _typeToProxy = typeToProxy;
            _isLoaded = (proxyHydrator == null);
            _proxyHydrator = proxyHydrator;
            _changeTracker = new ChangeTracker();
        }

        public override string Name
        {
            get
            {
                LoadMe();
                return base.Name;
            }
            set
            {
                LoadMe();
                if (!object.Equals(value, base.Name))
                {
                    base.Name = value;
                    NotifyChange("Name", value);
                }
            }
        }

        public override double Age
        {
            get
            {
                LoadMe();
                return base.Age;
            }
            set
            {
                LoadMe();
                if (!object.Equals(value, base.Age))
                {
                    base.Age = value;
                    NotifyChange("Age", value);
                }
            }
        }

        void LoadMe()
        {
            if (!_isLoaded)
            {
                //load me here
                _proxyHydrator.Hydrate(this, _typeToProxy);
                _isLoaded = true;
                _proxyHydrator.Dispose();
            }
        }

        void NotifyChange(string propName, object value)
        {
            ChangeTracker.Track(propName, value);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
                PropertyChanged(this, new PropertyChangedEventArgs("IsDirty"));
            }
        }

        #region ILazyObject Members

        public Type ProxyOf
        {
            get { return _typeToProxy; }
        }

        public bool IsProxyLoaded
        {
            get { return _isLoaded; }
        }

        #endregion

        #region ITrackable Members

        public bool IsDirty
        {
            get
            {
                if (ChangeTracker != null)
                {
                    return ChangeTracker.HasChanges;
                }
                else
                {
                    return false;
                }
            }
        }

        long version;
        public long Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Version"));
                }
            }
        }

        public IChangeTracker ChangeTracker
        {
            get { return _changeTracker; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class FakeBaseProxy
    {
        public virtual string Name { get; set; }
        public virtual double Age { get; set; }
    }
}
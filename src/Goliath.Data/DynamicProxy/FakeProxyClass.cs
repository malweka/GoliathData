using System;
using System.Collections.Generic;
using System.ComponentModel;
using Goliath.Data.Entity;

namespace Goliath.Data.DynamicProxy
{
    /* */
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Goliath.Data.DynamicProxy.FakeBaseProxy" />
    /// <seealso cref="Goliath.Data.DynamicProxy.ILazyObject" />
    public class FakeProxyClass : FakeBaseProxy, ILazyObject
    {
        Type _typeToProxy;
        bool _isLoaded;
        IProxyHydrator _proxyHydrator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeProxyClass"/> class.
        /// </summary>
        /// <param name="typeToProxy">The type to proxy.</param>
        /// <param name="proxyHydrator">The proxy hydrator.</param>
        public FakeProxyClass(Type typeToProxy, IProxyHydrator proxyHydrator)
        {
            _typeToProxy = typeToProxy;
            _isLoaded = (proxyHydrator == null);
            _proxyHydrator = proxyHydrator;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
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

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public override double Age
        {
            get
            {
                LoadMe();
                return base.Age;
            }
            set
            {
                //LoadMe();
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

        /// <summary>
        /// Gets the proxy of.
        /// </summary>
        /// <value>
        /// The proxy of.
        /// </value>
        public Type ProxyOf
        {
            get { return _typeToProxy; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is proxy loaded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is proxy loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsProxyLoaded
        {
            get { return _isLoaded; }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Goliath.Data.DynamicProxy.FakeBaseProxy" />
    /// <seealso cref="Goliath.Data.DynamicProxy.ILazyObject" />
    /// <seealso cref="Goliath.Data.Entity.ITrackable" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class FakeTrackableProxyClass : FakeBaseProxy, ILazyObject, ITrackable, INotifyPropertyChanged
    {
        Type _typeToProxy;
        bool _isLoaded;
        IChangeTracker _changeTracker;
        IProxyHydrator _proxyHydrator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeTrackableProxyClass"/> class.
        /// </summary>
        /// <param name="typeToProxy">The type to proxy.</param>
        /// <param name="proxyHydrator">The proxy hydrator.</param>
        public FakeTrackableProxyClass(Type typeToProxy, IProxyHydrator proxyHydrator)
        {
            _typeToProxy = typeToProxy;
            _isLoaded = (proxyHydrator == null);
            _proxyHydrator = proxyHydrator;
            _changeTracker = new ChangeTracker(GetInitialValues);
        }

        static IDictionary<string, object> GetInitialValues()
        {
            IDictionary<string, object> initialValues = new Dictionary<string, object>();
            initialValues.Add("Name", null);
            initialValues.Add("Age", null);
            return null;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name
        {
            get
            {
                LoadMe();
                return base.Name;
            }
            set
            {
                //LoadMe();
                if (!object.Equals(value, base.Name))
                {
                    base.Name = value;
                    NotifyChange("Name", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public override double Age
        {
            get
            {
                LoadMe();
                return base.Age;
            }
            set
            {
                //LoadMe();
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

        /// <summary>
        /// Gets the proxy of.
        /// </summary>
        /// <value>
        /// The proxy of.
        /// </value>
        public Type ProxyOf
        {
            get { return _typeToProxy; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is proxy loaded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is proxy loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsProxyLoaded
        {
            get { return _isLoaded; }
        }

        #endregion

        #region ITrackable Members

        /// <summary>
        /// Gets a value indicating whether this instance is dirty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is dirty; otherwise, <c>false</c>.
        /// </value>
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
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
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
                    PropertyChanged(this, new PropertyChangedEventArgs("IsDirty"));
                }
            }
        }

        /// <summary>
        /// Gets the change set.
        /// </summary>
        /// <value>
        /// The change set.
        /// </value>
        public IChangeTracker ChangeTracker
        {
            get { return _changeTracker; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion   
    }

    /// <summary>
    /// 
    /// </summary>
    public class FakeBaseProxy
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name { get; set; }
        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public virtual double Age { get; set; }
    }
    /* */
}
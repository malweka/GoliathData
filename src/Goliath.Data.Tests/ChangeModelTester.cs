using System;
using System.Collections.Generic;
using Goliath.Data.Entity;

namespace Goliath.Data.Tests
{
    class ChangeModelTester : ITrackable
    {
        private bool isbool;

        public bool IsBool
        {
            get { return isbool; }
            set
            {
                isbool = value;
                NotifyChange("IsBool", value);
            }
        }

        private bool? nullableBool;

        public bool? IsNullableBool
        {
            get
            {
                return nullableBool;
            }
            set
            {
                nullableBool = value;
                NotifyChange("IsNullableBool", value);
            }
        }

        private DateTime dd;

        public DateTime TheDateTime
        {
            get { return dd; }
            set
            {
                dd = value;
                NotifyChange("TheDateTime", value);
            }
        }

        private int dum;

        public int Dummy
        {
            get
            {
                return dum;
            }
            set
            {
                dum = value;
                NotifyChange("Dummy", value);
            }
        }

        readonly IChangeTracker changeTracker;

        protected void NotifyChange(string propName, object value)
        {
            ChangeTracker.Track(propName, value);
        }

        public bool IsDirty
        {
            get
            {
                return ChangeTracker != null && ChangeTracker.HasChanges;
            }
        }
        public IChangeTracker ChangeTracker
        {
            get { return changeTracker; }
        }

        public long Version { get; set; }

        static IDictionary<string, object> LoadInitialValues()
        {
            var initValues = new Dictionary<string, object>
            {
                {"IsBool", false},
                {"IsNullableBool", default(bool?)},
                {"TheDateTime", default(DateTime)},
                {"Dummy", default(int)},
            };

            return initValues;
        }

        public ChangeModelTester(bool startTrackingOnInit)
        {
            changeTracker = new ChangeTracker(LoadInitialValues);
            if (!startTrackingOnInit) return;
            changeTracker.Init();
            changeTracker.Start();
            Version = changeTracker.Version;
        }
    }
}
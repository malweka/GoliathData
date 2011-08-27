using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    [Serializable]
    [DataContract]
    //TODO: make this a struct?
    public class ProjectSettings
    {
        [DataMember]
        public string Version { get;  set; }
        [DataMember]
        public string ConnectionString { get; set; }
        [DataMember]
        public string Namespace { get; set; }
        [DataMember]
        public string AssemblyName { get; set; }
        [DataMember]
        public string TablePrefixes { get; set; }
        [DataMember]
        public string BaseModel { get; set; }

    }
}

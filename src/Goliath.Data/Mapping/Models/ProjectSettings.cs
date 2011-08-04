using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Goliath.Data.Mapping
{
    [Serializable]
    [DataContract]
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

        //static ProjectSettings defaultSettings = new ProjectSettings()
        //{
        //    Version = "1",
        //    ConnectionString = "Data Source=localhost;Initial Catalog=myDatabase;Integrated Security=True",
        //    Namespace = "MyNameSpace"
        //};

        //public static ProjectSettings CurrentSettings
        //{
        //    get { return defaultSettings; }
        //}
    }
}

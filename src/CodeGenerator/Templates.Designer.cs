﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Goliath.Data.CodeGenerator {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Templates {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Templates() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Goliath.Data.CodeGenerator.Templates", typeof(Templates).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @using System.Text
        ///@using System.Data
        ///@using Goliath.Data.Mapping
        ///@using Goliath.Data.Diagnostics
        ///@using Goliath.Data.Providers.SqlServer
        ///@using Goliath.Data.Utils
        ///@using Goliath.Data.CodeGenerator
        ///@{
        ///	var sqlMapper = new Mssq2008Dialect();
        ///	int counter = 0;
        ///	List&lt;string&gt; pks = new List&lt;string&gt;();
        ///	var uniqueConstrainst = new Dictionary&lt;string, List&lt;string&gt;&gt;();
        ///	var sortedProps = new System.Collections.Generic.SortedList&lt;int,Property&gt;();
        ///	var ent = Model;
        ///	@:CREATE TABLE [@(ent.SchemaName)].[@ [rest of string was truncated]&quot;;.
        /// </summary>
        public static string CreateTable {
            get {
                return ResourceManager.GetString("CreateTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @using System.Text
        ///@using System.Data
        ///@using Goliath.Data.Mapping
        ///@using Goliath.Data.Diagnostics
        ///@using Goliath.Data.Providers.SqlServer
        ///@using Goliath.Data.Utils
        ///@using Goliath.Data.CodeGenerator
        ///@{
        ///	var sqlMapper = new Mssq2008Dialect();
        ///	int counter = 0;
        ///	List&lt;string&gt; pks = new List&lt;string&gt;();
        ///	Dictionary&lt;string,string&gt; props = new Dictionary&lt;string, string&gt;();
        ///	var uniqueConstrainst = new Dictionary&lt;string, List&lt;string&gt;&gt;();
        ///	var sortedProps = new System.Collections.Generic.SortedList&lt;int,P [rest of string was truncated]&quot;;.
        /// </summary>
        public static string CreateTempTable {
            get {
                return ResourceManager.GetString("CreateTempTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @using System.Text
        ///@using System.Data
        ///@using Goliath.Data.Mapping
        ///@using Goliath.Data.Diagnostics
        ///@using Goliath.Data.Providers.SqlServer
        ///@using Goliath.Data.Utils
        ///@using Goliath.Data.CodeGenerator
        ///@{
        ///	var sqlMapper = new Mssq2008Dialect();
        ///	int counter = 0;
        ///	List&lt;string&gt; pks = new List&lt;string&gt;();
        ///	Dictionary&lt;string,string&gt; props = new Dictionary&lt;string, string&gt;();
        ///	var uniqueConstrainst = new Dictionary&lt;string, List&lt;string&gt;&gt;();
        ///	var sortedProps = new System.Collections.Generic.SortedList&lt;int,P [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Merge {
            get {
                return ResourceManager.GetString("Merge", resourceCulture);
            }
        }
    }
}

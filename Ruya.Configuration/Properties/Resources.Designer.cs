﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ruya.Configuration.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Ruya.Configuration.Properties.Resources", typeof(Resources).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Section encrypted.
        /// </summary>
        internal static string ConfigurationSectionHelper_ProtectSection {
            get {
                return ResourceManager.GetString("ConfigurationSectionHelper_ProtectSection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is no section with name {0}.
        /// </summary>
        internal static string ConfigurationSectionHelper_Section_KeyNotFound {
            get {
                return ResourceManager.GetString("ConfigurationSectionHelper_Section_KeyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Section {0} is locked or declared.
        /// </summary>
        internal static string ConfigurationSectionHelper_SectionLocked {
            get {
                return ResourceManager.GetString("ConfigurationSectionHelper_SectionLocked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Section decrypted.
        /// </summary>
        internal static string ConfigurationSectionHelper_UnprotectSection {
            get {
                return ResourceManager.GetString("ConfigurationSectionHelper_UnprotectSection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Settings collection does not contain the requested key which is {0}.
        /// </summary>
        internal static string ConfigurationSectionHelper_Value_KeyNotFound {
            get {
                return ResourceManager.GetString("ConfigurationSectionHelper_Value_KeyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to X.
        /// </summary>
        internal static string TripleDesProtectedConfigurationProvider_Separator {
            get {
                return ResourceManager.GetString("TripleDesProtectedConfigurationProvider_Separator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to token.
        /// </summary>
        internal static string TripleDesProtectedConfigurationProvider_Token {
            get {
                return ResourceManager.GetString("TripleDesProtectedConfigurationProvider_Token", resourceCulture);
            }
        }
    }
}

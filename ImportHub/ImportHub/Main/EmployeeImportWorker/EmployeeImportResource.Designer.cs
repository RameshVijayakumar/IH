﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EmployeeImportWorker {
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
    public class EmployeeImportResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal EmployeeImportResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EmployeeImportWorker.EmployeeImportResource", typeof(EmployeeImportResource).Assembly);
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
        ///   Looks up a localized string similar to The company domain service client call did not succeed..
        /// </summary>
        public static string CompanyDomainCallDidNotSucceed {
            get {
                return ResourceManager.GetString("CompanyDomainCallDidNotSucceed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exact duplicate record headers found at Employee {0} for Record Type {1}..
        /// </summary>
        public static string EEImportDuplicateRecords {
            get {
                return ResourceManager.GetString("EEImportDuplicateRecords", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exact duplicate record headers found for Sequence Number {0}..
        /// </summary>
        public static string EEImportDuplicateSeqNumberRecords {
            get {
                return ResourceManager.GetString("EEImportDuplicateSeqNumberRecords", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Effective Date year needs to be current year..
        /// </summary>
        public static string EEImportIncorrectEffectiveDateYear {
            get {
                return ResourceManager.GetString("EEImportIncorrectEffectiveDateYear", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Effective Date year needs to be current year in the Trailer record..
        /// </summary>
        public static string EEImportIncorrectEffectiveDateYearInTrailerRecord {
            get {
                return ResourceManager.GetString("EEImportIncorrectEffectiveDateYearInTrailerRecord", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified Client ID in the uploaded file is invalid..
        /// </summary>
        public static string EEImportInvalidClientId {
            get {
                return ResourceManager.GetString("EEImportInvalidClientId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified Employee Number in the uploaded file is invalid..
        /// </summary>
        public static string EEImportInvalidEENumber {
            get {
                return ResourceManager.GetString("EEImportInvalidEENumber", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified Effective Date in the uploaded file is invalid..
        /// </summary>
        public static string EEImportInvalidEffectiveDate {
            get {
                return ResourceManager.GetString("EEImportInvalidEffectiveDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified Sequence Number in the uploaded file is invalid..
        /// </summary>
        public static string EEImportInvalidSequence {
            get {
                return ResourceManager.GetString("EEImportInvalidSequence", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing Client ID which is required for export payload..
        /// </summary>
        public static string EEImportMissingClientId {
            get {
                return ResourceManager.GetString("EEImportMissingClientId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple Client IDs identified. All records in this import file must be for a single client id..
        /// </summary>
        public static string EEImportMultipleClientIds {
            get {
                return ResourceManager.GetString("EEImportMultipleClientIds", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid data in the uploaded file..
        /// </summary>
        public static string EEImportSubqueryException {
            get {
                return ResourceManager.GetString("EEImportSubqueryException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Field: {0}, Sequence: {1}, Record Type: {2}, Message Type: {3}, Description: {4}..
        /// </summary>
        public static string EEImportValidationIssue {
            get {
                return ResourceManager.GetString("EEImportValidationIssue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to parse validation error from Perform API.
        /// </summary>
        public static string EEImportValidationParseError {
            get {
                return ResourceManager.GetString("EEImportValidationParseError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The payload could not be cast to an employee import payload..
        /// </summary>
        public static string EERestApiPayloadNull {
            get {
                return ResourceManager.GetString("EERestApiPayloadNull", resourceCulture);
            }
        }
    }
}

using System;
using System.Configuration;
using Amazon.Common.Helpers;

namespace Ccen.Web
{
    /// <summary>
    /// This class was generated by the AppSettings T4 template
    /// </summary>
    public static class AppSettings
    {
        public static string DomainUrl { get { return GetConfigSettingItem<string>("DomainUrl"); } }
        public static string log4net_Config { get { return GetConfigSettingItem<string>("log4net.Config"); } }
        public static string webpages_Version { get { return GetConfigSettingItem<string>("webpages:Version"); } }
        public static string PreserveLoginUrl { get { return GetConfigSettingItem<string>("PreserveLoginUrl"); } }
        public static string ClientValidationEnabled { get { return GetConfigSettingItem<string>("ClientValidationEnabled"); } }
        public static string UnobtrusiveJavaScriptEnabled { get { return GetConfigSettingItem<string>("UnobtrusiveJavaScriptEnabled"); } }
        public static string aspnet_MaxJsonDeserializerMembers { get { return GetConfigSettingItem<string>("aspnet:MaxJsonDeserializerMembers"); } }
        public static string DefaultCompanyName { get { return GetConfigSettingItem<string>("DefaultCompanyName"); } }
        public static string UserName { get { return GetConfigSettingItem<string>("UserName"); } }
        public static string DefaultCustomType { get { return GetConfigSettingItem<string>("DefaultCustomType"); } }
        public static string LabelDirectory { get { return GetConfigSettingItem<string>("LabelDirectory"); } }
        public static string ReserveDirectory { get { return GetConfigSettingItem<string>("ReserveDirectory"); } }
        public static string TemplateDirectory { get { return GetConfigSettingItem<string>("TemplateDirectory"); } }
        public static string CustomerReportTemplate { get { return GetConfigSettingItem<string>("CustomerReportTemplate"); } }
        public static string InventoryReportTemplate { get { return GetConfigSettingItem<string>("InventoryReportTemplate"); } }
        public static string OrderReportTemplate { get { return GetConfigSettingItem<string>("OrderReportTemplate"); } }
        public static bool IsForceHttps { get { return GetConfigSettingItem<bool>("IsForceHttps"); } }
        public static bool IsSampleLabels { get { return GetConfigSettingItem<bool>("IsSampleLabels"); } }
        public static bool IsDebug { get { return GetConfigSettingItem<bool>("IsDebug"); } }
        public static bool IsDemo { get { return GetConfigSettingItem<bool>("IsDemo"); } }
    
        private const string MISSING_CONFIG = "Invalid configuration. Required AppSettings section is missing";
        private const string INVALID_CONFIG_SETTING = "Invalid configuration setting name: {0}";

        private static T GetConfigSettingItem<T>(string name)
        {
            if (ConfigurationManager.AppSettings == null)
                throw new ConfigurationErrorsException(MISSING_CONFIG);

            T value = default(T);
            if (ConfigurationManager.AppSettings.Count != 0)
            {
                try 
                {
                    value = AppConfigWrapper.Instance.GetValue<T>(name);
                } 
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(SettingItemErrorMessage(name, exception));
                }
            }
            return value;
        }
        
        private static string SettingItemErrorMessage(string name)
        {
            return string.Format(INVALID_CONFIG_SETTING, name);
        }

        private static string SettingItemErrorMessage(string name, Exception exception)
        {
            return string.Format(INVALID_CONFIG_SETTING, name) + exception.Message;
        }
    }
}

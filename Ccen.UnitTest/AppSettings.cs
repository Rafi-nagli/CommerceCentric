using System;
using System.Configuration;
using Amazon.Common.Helpers;

namespace Ccen.UnitTest
{
    /// <summary>
    /// This class was generated by the AppSettings T4 template
    /// </summary>
    public static class AppSettings
    {
        public static string log4net_Config { get { return GetConfigSettingItem<string>("log4net.Config"); } }
        public static string JavaPath { get { return GetConfigSettingItem<string>("JavaPath"); } }
        public static string RequestIntervalSec { get { return GetConfigSettingItem<string>("RequestIntervalSec"); } }
        public static string ReportDirectory { get { return GetConfigSettingItem<string>("ReportDirectory"); } }
        public static string FulfillmentResponseDirectory { get { return GetConfigSettingItem<string>("FulfillmentResponseDirectory"); } }
        public static string FulfillmentRequestDirectory { get { return GetConfigSettingItem<string>("FulfillmentRequestDirectory"); } }
        public static string WalmartFeedBaseDirectory { get { return GetConfigSettingItem<string>("WalmartFeedBaseDirectory"); } }
        public static string WalmartReportBaseDirectory { get { return GetConfigSettingItem<string>("WalmartReportBaseDirectory"); } }
        public static string SwatchImageDirectory { get { return GetConfigSettingItem<string>("SwatchImageDirectory"); } }
        public static string SwatchImageBaseUrl { get { return GetConfigSettingItem<string>("SwatchImageBaseUrl"); } }
        public static string WalmartImageDirectory { get { return GetConfigSettingItem<string>("WalmartImageDirectory"); } }
        public static string WalmartImageBaseUrl { get { return GetConfigSettingItem<string>("WalmartImageBaseUrl"); } }
        public static string JetImageDirectory { get { return GetConfigSettingItem<string>("JetImageDirectory"); } }
        public static string JetImageBaseUrl { get { return GetConfigSettingItem<string>("JetImageBaseUrl"); } }
        public static string eBayImageDirectory { get { return GetConfigSettingItem<string>("eBayImageDirectory"); } }
        public static string eBayImageBaseUrl { get { return GetConfigSettingItem<string>("eBayImageBaseUrl"); } }
        public static string GrouponImageDirectory { get { return GetConfigSettingItem<string>("GrouponImageDirectory"); } }
        public static string GrouponImageBaseUrl { get { return GetConfigSettingItem<string>("GrouponImageBaseUrl"); } }
        public static string ShopifyImageDirectory { get { return GetConfigSettingItem<string>("ShopifyImageDirectory"); } }
        public static string ShopifyImageBaseUrl { get { return GetConfigSettingItem<string>("ShopifyImageBaseUrl"); } }
        public static string InventoryReportRequestIntervalHour { get { return GetConfigSettingItem<string>("InventoryReportRequestIntervalHour"); } }
        public static string OrdersReportRequestIntervalHour { get { return GetConfigSettingItem<string>("OrdersReportRequestIntervalHour"); } }
        public static string ProcessOrdersIntervalHour { get { return GetConfigSettingItem<string>("ProcessOrdersIntervalHour"); } }
        public static string UpdateFulfillmentIntervalMinute { get { return GetConfigSettingItem<string>("UpdateFulfillmentIntervalMinute"); } }
        public static string AwaitReadyReportIntervalHour { get { return GetConfigSettingItem<string>("AwaitReadyReportIntervalHour"); } }
        public static string MaxRequestAttempt { get { return GetConfigSettingItem<string>("MaxRequestAttempt"); } }
        public static string OverdueAutoPurchaseTime { get { return GetConfigSettingItem<string>("OverdueAutoPurchaseTime"); } }
        public static int AmazonFulfillmentLatencyDays { get { return GetConfigSettingItem<int>("AmazonFulfillmentLatencyDays"); } }
        public static int WalmartFulfillmentLatencyDays { get { return GetConfigSettingItem<int>("WalmartFulfillmentLatencyDays"); } }
        public static string SMTP_ServerHost { get { return GetConfigSettingItem<string>("SMTP_ServerHost"); } }
        public static string SMTP_ServerPort { get { return GetConfigSettingItem<string>("SMTP_ServerPort"); } }
        public static string SMTP_EmailUsername { get { return GetConfigSettingItem<string>("SMTP_EmailUsername"); } }
        public static string SMTP_EmailPassword { get { return GetConfigSettingItem<string>("SMTP_EmailPassword"); } }
        public static string SMTP_DisplayFromEmail { get { return GetConfigSettingItem<string>("SMTP_DisplayFromEmail"); } }
        public static string SMTP_FromEmail { get { return GetConfigSettingItem<string>("SMTP_FromEmail"); } }
        public static string Support_ServerHost { get { return GetConfigSettingItem<string>("Support_ServerHost"); } }
        public static string Support_ServerPort { get { return GetConfigSettingItem<string>("Support_ServerPort"); } }
        public static string Support_EmailUsername { get { return GetConfigSettingItem<string>("Support_EmailUsername"); } }
        public static string Support_DisplayFromEmail { get { return GetConfigSettingItem<string>("Support_DisplayFromEmail"); } }
        public static string Support_EmailPassword { get { return GetConfigSettingItem<string>("Support_EmailPassword"); } }
        public static string Support_ProcessMessageThreadTimeoutSecond { get { return GetConfigSettingItem<string>("Support_ProcessMessageThreadTimeoutSecond"); } }
        public static string Support_MaxProcessMessageErrorsCount { get { return GetConfigSettingItem<string>("Support_MaxProcessMessageErrorsCount"); } }
        public static string Support_AttachmentDirectory { get { return GetConfigSettingItem<string>("Support_AttachmentDirectory"); } }
        public static string Support_AttachmentFolderRelativeUrl { get { return GetConfigSettingItem<string>("Support_AttachmentFolderRelativeUrl"); } }
        public static string UserName { get { return GetConfigSettingItem<string>("UserName"); } }
        public static string DefaultCustomType { get { return GetConfigSettingItem<string>("DefaultCustomType"); } }
        public static string LabelDirectory { get { return GetConfigSettingItem<string>("LabelDirectory"); } }
        public static string ReserveDirectory { get { return GetConfigSettingItem<string>("ReserveDirectory"); } }
        public static string TemplateDirectory { get { return GetConfigSettingItem<string>("TemplateDirectory"); } }
        public static bool IsSampleLabels { get { return GetConfigSettingItem<bool>("IsSampleLabels"); } }
        public static bool IsDebug { get { return GetConfigSettingItem<bool>("IsDebug"); } }
    
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Routing;
using Amazon.Core.Exceptions;

namespace Amazon.Common.Helpers
{
    public class ExceptionHelper
    {
        static public bool IsStampsExeption(Exception ex)
        {
            if (ex is FaultException || ex is CommunicationException)
                return true;
            return false;
        }

        static public bool IsStampsConversationSyncEx(Exception ex)
        {
            if (ex is FaultException || ex is StampsException)
            {
                //System.ServiceModel.FaultException: Conversation out-of-sync.
                //System.ServiceModel.FaultException: Invalid conversation token.
                var isConversationOutOfSync = ex.Message.Contains("out-of-sync") || ex.Message.Contains("conversation token");
                return isConversationOutOfSync;
            }
            return false;
        }

        static public bool IsStampsCommunicationEx(Exception ex)
        {
            if (ex is FaultException || ex is StampsException)
            {
                //System.ServiceModel.FaultException: Conversation out-of-sync.
                //System.ServiceModel.FaultException: Invalid conversation token.
                var isCommunicationEx = ex.Message.Contains("Communication with stamps");
                return isCommunicationEx;
            }
            return false;
        }
        
        static public Exception GetMostDeeperException(Exception ex)
        {
            if (ex == null)
                return null;

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            return ex;
        }

        static public string FormatExMessage(string message)
        {
            //"USPS Desc:No record of that item (-2147219302). Failed to get tracking events."
            if (message.Contains("-2147219302"))
            {
                return "USPS Desc: No record of that item";
            }
            return message;
        }

        static public string FormatErrorMessageToUI(string message)
        {
            if (String.IsNullOrEmpty(message))
                return message;

            return message.Replace("http://stamps.com/xml/namespace/2014/05/swsim/swsim", "...");
        }

        static public string ToXml(Exception ex)
        {
            //System.Xml.Serialization.XmlSerializer writer = 
            //    new System.Xml.Serialization.XmlSerializer(typeof(Exception));

            //writer.Serialize();

            //var stream = new MemoryStream();
            //System.IO.StreamWriter file = new System.IO.StreamWriter(stream);
            //writer.Serialize(file, ex);
            return String.Empty;
        }

        public static string GetMessage(Exception ex)
        {
            var msg = String.Empty;
            while (ex != null)
            {
                msg += ex.GetType().FullName + ": " + ex.Message + "\r\n"
                    + ex.StackTrace + "\r\n";
                ex = ex.InnerException;
            }
            return msg;
        }

        static public string GetAllMessages(Exception ex)
        {
            string message = String.Empty;
            while (ex != null)
            {
                message += ex.Message + "\r\n";
                ex = ex.InnerException;
            }
            return message;
        }

        static public string GetExceptionContextInfo(HttpContextBase context, RouteData routeData)
        {
            string message = GetHttpContextInfo(context);
            message += Environment.NewLine + "RouteData: ";
            foreach (var value in routeData.Values)
                message += Environment.NewLine + value.Key + ": " + value.Value;
            return message;
        }

        static public string GetHttpContextInfo(HttpContextBase context)
        {
            string message = String.Empty;
            try
            {
                message += Environment.NewLine + "User: " + context.User.Identity.Name;
            }
            catch { }
            message += Environment.NewLine + "Reqeust.Url: " + context.Request.Url;
            if (context.Request.UrlReferrer != null)
                message += Environment.NewLine + "Request.UrlReferrer: " + context.Request.UrlReferrer.AbsoluteUri;
            message += Environment.NewLine + "Params.AllKeys: ";
            foreach (var key in context.Request.Params.AllKeys)
                message += Environment.NewLine + key + ": " + context.Request.Params[key];

            return message;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Contracts.SystemActions;
using Newtonsoft.Json;

namespace Amazon.Core.Models
{
    public class NotificationHelper
    {
        public static string ToString(NotificationType type)
        {
            switch (type)
            {
                //case NotificationType.DuplicateOrder:
                //    return "Duplicate order";
                case NotificationType.LabelGotStuck:
                    return "Package got stuck";
                case NotificationType.LabelNeverShipped:
                    return "Label never shipped";
                case NotificationType.AmazonProductImageChanged:
                    return "Product image changed";
            }
            return "-";
        }


        static public T FromStr<T>(string data) where T : INotificationParams
        {
            if (data == null)
                data = ""; //or return default(T)

            return JsonConvert.DeserializeObject<T>(data);
        }

        public static string ToStr(INotificationParams data)
        {
            if (data == null)
                return null;

            return JsonConvert.SerializeObject(data);
        }
    }
}

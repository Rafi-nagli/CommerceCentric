using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemMessages;
using Amazon.DTO.Orders;
using Amazon.Model.Implementation.Charts;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Companies;

namespace Amazon.Web.ViewModels
{
    public class SystemMessageViewModel
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public int? Status { get; set; }
        public string Message { get; set; }

        public string FormattedStatus
        {
            get
            {
                if (Status == (int)MessageStatus.Error)
                    return "Error";
                if (Status == (int)MessageStatus.Warning)
                    return "Warning";
                if (Status == (int)MessageStatus.Info)
                    return "Info";
                if (Status == (int)MessageStatus.Success)
                    return "Success";
                return "n/a";
            }
        }

        public SystemMessageViewModel(SystemMessageDTO message)
        {
            Name = message.Name;
            Status = message.Status;

            if (message.Name == SystemMessageServiceHelper.ServiceStatusKey)
            {
                Name = message.Tag + " service status";
                Message = !String.IsNullOrEmpty(message.Data) ?
                    JsonHelper.Deserialize<ExceptionMessageData>(message.Data).Message : "";
            }
            if (message.Name == SystemMessageServiceHelper.MissingOrderKey)
            {
                var data = JsonHelper.Deserialize<MissingOrderMessageData>(message.Data);
                Name = "Missing order: " + (!String.IsNullOrEmpty(message.Data) ?
                    data.OrderId + "(" + MarketHelper.GetShortName((int)data.Market, data.MarketplaceId) + ")" : "-");
                Message = "Missing SKUs: " + String.Join(", ", data.MissingSKUList);
            }
        }

        public static IList<SystemMessageViewModel> GetAll(IDbFactory dbFactory)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var messages = db.SystemMessages.GetAllAsDto().Where(m => m.Status == (int)MessageStatus.Error).ToList();
                return messages.Select(m => new SystemMessageViewModel(m)).ToList();
            }
        }
    }
}
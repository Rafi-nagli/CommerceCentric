using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.InventoryGroup
{
    public class InventoryGroupPriceViewModel
    {
        public int? DiscountPercent { get; set; }
        public int? Market { get; set; }

        public IList<MessageString> Validate()
        {
            var messages = new List<MessageString>();
            if (!DiscountPercent.HasValue)
                messages.Add(MessageString.Error("Empty 'Discount Percent'"));
            if (DiscountPercent <= 0 || DiscountPercent >= 100)
                messages.Add(MessageString.Error("Invalid 'Discount Percent' value"));
            if (!Market.HasValue)
                messages.Add(MessageString.Error("Please choose marketplace"));

            return messages;
        }

        public static CallMessagesResultVoid Apply(IUnitOfWork db,
           ISystemActionService actionService,
           InventoryGroupPriceViewModel model,
           long? by)
        {
            return new CallMessagesResultVoid()
            {
                Status = CallStatus.Success
            };
        }
    }
}
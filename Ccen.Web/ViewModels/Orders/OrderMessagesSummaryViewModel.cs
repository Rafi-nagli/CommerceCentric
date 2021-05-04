using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;

namespace Amazon.Web.ViewModels.Orders
{
    public class OrderMessagesSummaryViewModel
    {
        public long? OrderEntityId { get; set; }
        public string OrderNumber { get; set; }

        public IList<CommentViewModel> Comments { get; set; }
        public int EmailCount { get; set; }


        public OrderMessagesSummaryViewModel()
        {
            
        }

        public static OrderMessagesSummaryViewModel GetByOrderId(IUnitOfWork db,
            ILogService log,
            string orderNumber)
        {
            var result = new OrderMessagesSummaryViewModel();
            if (!String.IsNullOrEmpty(orderNumber))
            {
                orderNumber = orderNumber.RemoveWhitespaces();
                orderNumber = OrderHelper.RemoveOrderNumberFormat(orderNumber);

                var order = db.Orders.GetByOrderIdAsDto(orderNumber);
                if (order != null)
                {
                    var comments = db.OrderComments.GetByOrderIdDto(order.Id)
                        .OrderByDescending(c => c.CreateDate)
                        .ToList();

                    result.Comments = comments.Select(c => new CommentViewModel()
                    {
                        Comment = c.Message,
                        CommentByName = c.CreatedByName,
                        CommentDate = c.CreateDate
                    }).ToList();
                    
                    var emails = db.Emails.GetAllByOrderId(order.CustomerOrderId);
                    result.EmailCount = emails
                        .Select(e => e.Id).Distinct().Count();

                    result.OrderEntityId = order.Id;
                }

                result.OrderNumber = orderNumber;
            }

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO.Orders;

namespace Amazon.Web.ViewModels.Orders
{
    public class OrderNotifyViewModel
    {
        public int Type { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }

        public OrderNotifyViewModel(OrderNotifyDto notify)
        {
            Type = notify.Type;
            Status = notify.Status;
            Message = notify.Message;
        }
    }
}
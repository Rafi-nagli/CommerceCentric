using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Orders
{

    public class CommentViewModel
    {
        public long Id { get; set; }
        public string Comment { get; set; }
        public int Type { get; set; }

        public string OrderNumber { get; set; }

        public string TypeName
        {
            get { return CommentHelper.TypeToString((CommentType)Type); }
        }

        public long? LinkedEmailId { get; set; }

        public bool HasEmailLink
        {
            get { return LinkedEmailId.HasValue; }
        }

        public string EmailUrl
        {
            get
            {
                if (LinkedEmailId.HasValue)
                    return UrlHelper.GetViewEmailUrl(LinkedEmailId.Value, OrderNumber);
                return null;
            }
        }

        public DateTime? CommentDate { get; set; }
        public string CommentByName { get; set; }
    }
}
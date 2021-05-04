using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Common.Helpers
{
    public class CommentHelper
    {
        public static string TypeToString(CommentType type)
        {
            switch (type)
            {
                case CommentType.None:
                    return "-";
                case CommentType.Address:
                    return "Address";
                case CommentType.Notification:
                    return "Notification";
                case CommentType.IncomingEmail:
                    return "Incoming Email";
                case CommentType.OutputEmail:
                    return "Sent Email";
                case CommentType.ReturnExchange:
                    return "Return/Exchange";
                case CommentType.MarketplaceClaim:
                    return "Marketplace Claim";
                case CommentType.System:
                    return "System";
                case CommentType.Other:
                    return "Other";
            }
            return "-";
        }
    }
}

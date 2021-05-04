using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;

namespace Amazon.Web.ViewModels.Orders
{
    public class ViewCommentsViewModel
    {
        public IList<CommentViewModel> Comments { get; set; }

        public ViewCommentsViewModel()
        {
            
        }

        public ViewCommentsViewModel(IUnitOfWork db, long orderId)
        {
            var comments = db.OrderComments.GetByOrderIdDto(orderId);
            Comments = comments.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Comment = c.Message,
                Type = c.Type,
                LinkedEmailId = c.LinkedEmailId,
                CommentDate = c.CreateDate,
                CommentByName = c.CreatedByName
            }).ToList();
        }
    }
}
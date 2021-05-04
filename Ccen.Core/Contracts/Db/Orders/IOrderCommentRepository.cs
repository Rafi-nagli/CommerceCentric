using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Views;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IOrderCommentRepository : IRepository<OrderComment>
    {
        IQueryable<CommentDTO> GetByOrderIdDto(long orderId);
        IList<OrderComment> GetByOrderId(long orderId);

        IQueryable<CommentDTO> GetAllAsDto();

        IQueryable<ViewActualOrderComment> GetLastComment();

        CommentDTO UpdateComments(IList<CommentDTO> comments,
            long orderId,
            DateTime? when,
            long? by);

        CommentDTO AddComments(IList<CommentDTO> comments,
            long orderId,
            DateTime? when,
            long? by);
    }
}

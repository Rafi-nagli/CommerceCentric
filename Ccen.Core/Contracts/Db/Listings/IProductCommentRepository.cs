using System;
using System.Collections.Generic;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IProductCommentRepository : IRepository<ProductComment>
    {
        IList<ProductComment> GetCommentsForProductId(long id);

        CommentDTO UpdateComments(IList<CommentDTO> comments,
            long parentItemId,
            DateTime? when,
            long? by);
    }
}

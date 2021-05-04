using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class ProductCommentRepository : Repository<ProductComment>, IProductCommentRepository
    {
        public ProductCommentRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<ProductComment> GetCommentsForProductId(long id)
        {
            return GetFiltered(c => !c.Deleted && c.ProductId == id).ToList();
        }

        public CommentDTO UpdateComments(IList<CommentDTO> comments, 
            long parentItemId,
            DateTime? when, 
            long? by)
        {
            //Keep only one default element
            var dbExistComments = GetCommentsForProductId(parentItemId);
            var newComments = comments.Where(l => l.Id == 0).ToList();
            CommentDTO lastComment = null;
            foreach (var dbComment in dbExistComments)
            {
                var existComment = comments.FirstOrDefault(l => l.Id == dbComment.Id);
                if (existComment != null && !string.IsNullOrEmpty(existComment.Message))
                {
                    lastComment = existComment;
                    if (existComment.Message != dbComment.Message)
                    {
                        dbComment.UpdatedBy = by;
                        dbComment.UpdateDate = when;
                        dbComment.Message = existComment.Message;
                    }
                }
                else
                {
                    Remove(dbComment);
                }
            }

            foreach (var newComment in newComments)
            {
                if (!string.IsNullOrEmpty(newComment.Message))
                {
                    lastComment = newComment;
                    Add(new ProductComment
                    {
                        ProductId = (int)parentItemId,
                        Message = newComment.Message,
                        CreatedBy = by,
                        CreateDate = when,
                    });
                }
            }

            unitOfWork.Commit();

            return lastComment;
        }
    }
}

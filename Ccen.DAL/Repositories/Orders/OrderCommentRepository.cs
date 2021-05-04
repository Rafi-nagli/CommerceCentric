using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class OrderCommentRepository : Repository<OrderComment>, IOrderCommentRepository
    {
        public OrderCommentRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<ViewActualOrderComment> GetLastComment()
        {
            return unitOfWork.GetSet<ViewActualOrderComment>();
        }

        public IQueryable<CommentDTO> GetByOrderIdDto(long orderId)
        {
            var query = from c in GetAll()
                join u in unitOfWork.Users.GetAll() on c.CreatedBy equals u.Id into withUser
                from u in withUser.DefaultIfEmpty()
                where !c.Deleted && c.OrderId == orderId
                        select new CommentDTO()
                {
                    Id = c.Id,
                    OrderId = c.OrderId,
                    Message = c.Message,
                    Type = c.Type,
                    LinkedEmailId = c.LinkedEmailId,
                    CreateDate = c.CreateDate,
                    CreatedBy = c.CreatedBy,
                    CreatedByName = u.Name
                };

            return query;
        }

        public IList<OrderComment> GetByOrderId(long orderId)
        {
            var query = from c in GetAll()
                        where !c.Deleted && c.OrderId == orderId
                        select c;

            return query.ToList();
        }

        public IQueryable<CommentDTO> GetAllAsDto()
        {
            var query = from c in GetAll()
                        join u in unitOfWork.Users.GetAll() on c.CreatedBy equals u.Id into withUser
                        from u in withUser.DefaultIfEmpty()
                        where !c.Deleted
                        select new CommentDTO()
                        {
                            Id = c.Id,
                            OrderId = c.OrderId,
                            Message = c.Message,
                            Type = c.Type,
                            LinkedEmailId = c.LinkedEmailId,
                            CreateDate = c.CreateDate,
                            CreatedBy = c.CreatedBy,
                            CreatedByName = u.Name
                        };

            return query;
        }

        public CommentDTO UpdateComments(IList<CommentDTO> comments,
            long orderId,
            DateTime? when,
            long? by)
        {
            //Keep only one default element
            var dbExistComments = GetByOrderId(orderId);
            var newComments = comments.Where(l => l.Id == 0).ToList();
            CommentDTO lastComment = null;
            foreach (var dbComment in dbExistComments)
            {
                var existComment = comments.FirstOrDefault(l => l.Id == dbComment.Id);
                if (existComment != null && !string.IsNullOrEmpty(existComment.Message))
                {
                    lastComment = existComment;
                    if (existComment.Message != dbComment.Message
                        || existComment.Type != dbComment.Type)
                    {
                        dbComment.UpdatedBy = by;
                        dbComment.UpdateDate = when;
                        dbComment.Message = existComment.Message;
                        dbComment.Type = existComment.Type;
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
                    Add(new OrderComment
                    {
                        OrderId = (int)orderId,
                        Message = newComment.Message,
                        Type = newComment.Type,
                        LinkedEmailId = newComment.LinkedEmailId,
                        CreatedBy = by,
                        CreateDate = when,
                        UpdatedBy = by,
                        UpdateDate = when
                    });
                }
            }

            unitOfWork.Commit();

            return lastComment;
        }

        public CommentDTO AddComments(IList<CommentDTO> comments,
            long orderId,
            DateTime? when,
            long? by)
        {
            //Keep only one default element
            var dbExistComments = GetByOrderIdDto(orderId);
            var newComments = comments.Where(l => l.Id == 0).ToList();
            CommentDTO lastComment = dbExistComments.OrderByDescending(c => c.CreateDate).FirstOrDefault();

            foreach (var newComment in newComments)
            {
                if (!string.IsNullOrEmpty(newComment.Message))
                {
                    lastComment = newComment;
                    Add(new OrderComment
                    {
                        OrderId = (int)orderId,
                        Message = newComment.Message,
                        Type = newComment.Type,
                        LinkedEmailId = newComment.LinkedEmailId,
                        CreatedBy = by,
                        CreateDate = when,
                        UpdatedBy = by,
                        UpdateDate = when
                    });
                }
            }

            unitOfWork.Commit();

            return lastComment;
        }
    }
}

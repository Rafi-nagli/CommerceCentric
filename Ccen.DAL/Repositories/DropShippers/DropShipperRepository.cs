using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Users;
using Amazon.DTO.DropShippers;

namespace Amazon.DAL.Repositories.DropShippers
{
    public class DropShipperRepository : Repository<DropShipper>, IDropShipperRepository
    {
        public DropShipperRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<DropShipperDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<DropShipperDTO> AsDto(IQueryable<DropShipper> query)
        {
            return query.Select(b => new DropShipperDTO()
            {
                Id = b.Id,

                Name = b.Name,
                IncomeFTPFolder = b.IncomeFTPFolder,
                IncomeArchiveFolder = b.IncomeArchiveFolder,
                OutputFTPFolder = b.OutputFTPFolder,

                OverrideEmail = b.OverrideEmail,

                ItemFeedNotifyEmails = b.ItemFeedNotifyEmails,
                OrderFeedNotifyEmails = b.OrderFeedNotifyEmails,
                NotifiesBccEmails = b.NotifiesBccEmails,
                CostMode = b.CostMode,
                CostMultiplier = b.CostMultiplier,

                QuantityThreshold = b.QuantityThreshold,
                ProcessingThresholdTime = b.ProcessingThresholdTime,

                PrePriority = b.PrePriority,
                PostPriority = b.PostPriority,

                SortOrder = b.SortOrder,
                IsActive = b.IsActive,
                CreateDate = b.CreateDate,
            });
        }
    }
}

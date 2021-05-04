using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Views;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class LabelRepository : Repository<ViewLabel>, ILabelRepository
    {
        public LabelRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }


        public IList<OrderShippingInfoDTO> GetByOrderIdAsDto(long orderId)
        {
            var shippingInfos = unitOfWork.OrderShippingInfos.GetByOrderIdAsDto(orderId).ToList();

            var mailLabels = unitOfWork.MailLabelInfos.GetByOrderIdsAsDto(new List<long>() { orderId })
                .Where(m => !m.CancelLabelRequested)
                .ToList();
            
            var labels = shippingInfos;
            labels.AddRange(mailLabels);
            return labels;
        }
    }
}

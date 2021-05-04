using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;

namespace Amazon.DAL.Repositories
{
    public class ScanFormRepository : Repository<ScanForm>, IScanFormRepository
    {
        public ScanFormRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<ScanFormDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }
        
        private IQueryable<ScanFormDTO> AsDto(IQueryable<ScanForm> query)
        {
            return query.Select(b => new ScanFormDTO()
            {
                Id = b.Id,
                
                FormId = b.FormId,
                FileName = b.FileName,
                
                CreateDate = b.CreateDate,
                CreatedBy = b.CreatedBy,
            });
        }
    }
}

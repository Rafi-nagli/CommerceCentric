using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.DTO.Inventory;
using Amazon.DTO.Shippings;

namespace Amazon.DAL.Repositories
{
    public class VendorInvoiceRepository : Repository<VendorInvoice>, IVendorInvoiceRepository
    {
        public VendorInvoiceRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<VendorInvoiceDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public void Update(VendorInvoiceDTO invoice)
        {
            var existEntry = Get(invoice.Id);
            if (existEntry != null)
            {
                
            }
            unitOfWork.Commit();
        }

        public long Store(VendorInvoiceDTO invoice)
        {
            var entity = new VendorInvoice()
            {
                FileName = invoice.FileName,

                InvoiceDate = invoice.InvoiceDate,
                InvoiceNumber = invoice.InvoiceNumber,
                
                CreateDate = invoice.CreateDate,
                CreatedBy = invoice.CreatedBy,
            };

            Add(entity);
            unitOfWork.Commit();

            return entity.Id;
        }

        private IQueryable<VendorInvoiceDTO> AsDto(IQueryable<VendorInvoice> query)
        {
            return query.Select(b => new VendorInvoiceDTO()
            {
                Id = b.Id,
                
                FileName = b.FileName,

                InvoiceDate = b.InvoiceDate,
                InvoiceNumber = b.InvoiceNumber,
                
                CreateDate = b.CreateDate,
                CreatedBy = b.CreatedBy,
            });
        }
    }
}

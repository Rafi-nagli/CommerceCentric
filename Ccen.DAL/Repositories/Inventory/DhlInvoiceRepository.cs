using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities.Orders;
using Amazon.DTO.Shippings;

namespace Amazon.DAL.Repositories
{
    public class DhlInvoiceRepository : Repository<DhlInvoice>, IDhlInvoiceRepository
    {
        public DhlInvoiceRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<DhlInvoiceDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public void Update(DhlInvoiceDTO invoice)
        {
            var existEntry = Get(invoice.Id);
            if (existEntry != null)
            {
                existEntry.Dimensions = invoice.Dimensions;
            }
            unitOfWork.Commit();
        }

        public long Store(DhlInvoiceDTO invoice)
        {
            var entity = new DhlInvoice()
            {
                OrderNumber = invoice.OrderNumber,
                Status = invoice.Status,

                InvoiceDate = invoice.InvoiceDate,
                InvoiceNumber = invoice.InvoiceNumber,

                BillNumber = invoice.BillNumber,
                Dimensions = invoice.Dimensions,
                ChargedBase = invoice.ChargedBase,
                ChargedSummary = invoice.ChargedSummary,
                ChargedCredit = invoice.ChargedCredit,

                Estimated = invoice.Estimated,

                SourceFile = invoice.SourceFile,

                CreateDate = invoice.CreateDate,
            };

            Add(entity);
            unitOfWork.Commit();

            return entity.Id;
        }

        private IQueryable<DhlInvoiceDTO> AsDto(IQueryable<DhlInvoice> query)
        {
            return query.Select(b => new DhlInvoiceDTO()
            {
                Id = b.Id,

                OrderNumber = b.OrderNumber,
                Status = b.Status,

                InvoiceDate = b.InvoiceDate,
                InvoiceNumber = b.InvoiceNumber,

                BillNumber = b.BillNumber,
                Dimensions = b.Dimensions,
                ChargedBase = b.ChargedBase,
                ChargedSummary = b.ChargedSummary,
                ChargedCredit = b.ChargedCredit,

                Estimated = b.Estimated,

                SourceFile = b.SourceFile,

                CreateDate = b.CreateDate,
            });
        }
    }
}

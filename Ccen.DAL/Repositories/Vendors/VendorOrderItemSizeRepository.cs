using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Entities.VendorOrders;
using Amazon.DTO.Vendors;

namespace Amazon.DAL.Repositories
{
    public class VendorOrderItemSizeRepository : Repository<VendorOrderItemSize>, IVendorOrderItemSizeRepository
    {
        public VendorOrderItemSizeRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public void UpdateSizesForVendorItem(long vendorItemId,
            IList<VendorOrderItemSizeDTO> sizes,
            DateTime when,
            long? by)
        {
            var dbExistItems = GetFiltered(l => l.VendorOrderItemId == vendorItemId).ToList();
            var newItems = sizes.Where(l => l.Id == 0).ToList();

            foreach (var dbItem in dbExistItems)
            {
                var existItem = sizes.FirstOrDefault(l => l.Id == dbItem.Id);
                if (existItem != null)
                {
                    dbItem.Size = existItem.Size;
                    dbItem.ASIN = existItem.ASIN;
                    dbItem.Breakdown = existItem.Breakdown;
                    dbItem.Order = existItem.Order;

                    dbItem.UpdateDate = when;
                    dbItem.UpdatedBy = by;
                }
                else
                {
                    Remove(dbItem);
                }
            }

            foreach (var newItem in newItems)
            {
                Add(new VendorOrderItemSize()
                {
                    VendorOrderItemId = vendorItemId,
                    Size = newItem.Size,
                    Breakdown = newItem.Breakdown,
                    ASIN = newItem.ASIN,
                    Order = newItem.Order,
                    
                    CreateDate = when,
                    CreatedBy = by
                });
            }

            unitOfWork.Commit();
        }

        public IQueryable<VendorOrderItemSizeDTO> GetAllAsDto()
        {
            return GetAll().Select(a => new VendorOrderItemSizeDTO()
            {
                Id = a.Id,
                VendorOrderItemId = a.VendorOrderItemId,
                Size = a.Size,
                Breakdown = a.Breakdown,
                ASIN = a.ASIN,
                Order = a.Order,
                CreateDate = a.CreateDate
            });
        }
    }
}

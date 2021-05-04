using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class ListingFBAInvRepository : Repository<ListingFBAInv>, IListingFBAInvRepository
    {
        public ListingFBAInvRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<ListingFBAInvDTO> GetAllActual()
        {
            return AsDto(GetFiltered(l => !l.IsRemoved)).ToList();
        }

        private IQueryable<ListingFBAInvDTO> AsDto(IQueryable<ListingFBAInv> items)
        {
            return items.Select(l => new ListingFBAInvDTO()
            {
                Id = l.Id,

                SellerSKU = l.SellerSKU,
                FulfillmentChannelSKU = l.FulfillmentChannelSKU,
                ASIN = l.ASIN,
                WarehouseConditionCode = l.WarehouseConditionCode,
                QuantityAvailable = l.QuantityAvailable,

                IsRemoved = l.IsRemoved,
                CreateDate = l.CreateDate,
                UpdateDate = l.UpdateDate
            });
        }

        public IEnumerable<string> ProcessRemoved(IEnumerable<string> skuList)
        {
            var results = new List<string>();
            var asRemoved = GetFiltered(i => !i.IsRemoved && !skuList.Contains(i.SellerSKU));
            foreach (var listing in asRemoved)
            {
                listing.IsRemoved = true;
                results.Add(listing.SellerSKU);
            }
            unitOfWork.Commit();

            return results;
        }

        public void CreateOrUpdate(ListingFBAInvDTO listing, DateTime? when)
        {
            var dbListing = GetFiltered(l => l.SellerSKU == listing.SellerSKU).FirstOrDefault();
            if (dbListing != null)
            {
                dbListing.SellerSKU = listing.SellerSKU;
                dbListing.FulfillmentChannelSKU = listing.FulfillmentChannelSKU;
                dbListing.ASIN = listing.ASIN;
                dbListing.WarehouseConditionCode = listing.WarehouseConditionCode;
                dbListing.QuantityAvailable = listing.QuantityAvailable;
                
                dbListing.UpdateDate = when;
            }
            else
            {
                dbListing = new ListingFBAInv();

                dbListing.SellerSKU = listing.SellerSKU;
                dbListing.FulfillmentChannelSKU = listing.FulfillmentChannelSKU;
                dbListing.ASIN = listing.ASIN;
                dbListing.WarehouseConditionCode = listing.WarehouseConditionCode;
                dbListing.QuantityAvailable = listing.QuantityAvailable;

                dbListing.CreateDate = when;

                unitOfWork.ListingFBAInvs.Add(dbListing);
            }

            unitOfWork.Commit();
        }
    }
}

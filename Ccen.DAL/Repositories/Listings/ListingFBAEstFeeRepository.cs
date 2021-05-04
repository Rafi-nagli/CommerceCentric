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
    public class ListingFBAEstFeeRepository : Repository<ListingFBAEstFee>, IListingFBAEstFeeRepository
    {
        public ListingFBAEstFeeRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<ListingFBAEstFeeDTO> GetAllActual()
        {
            return AsDto(GetFiltered(l => !l.IsRemoved)).ToList();
        }

        private IQueryable<ListingFBAEstFeeDTO> AsDto(IQueryable<ListingFBAEstFee> items)
        {
            return items.Select(l => new ListingFBAEstFeeDTO()
            {
                Id = l.Id,
                SKU = l.SKU,
                ASIN = l.ASIN,
                YourPrice = l.YourPrice,
                SalesPrice = l.SalesPrice,
                Currency = l.Currency,

                EstimatedFee = l.EstimatedFee,
                EstimatedReferralFeePerUnit = l.EstimatedReferralFeePerUnit,
                EstimatedVariableClosingFee = l.EstimatedVariableClosingFee,
                EstimatedOrderHandlingFeePerOrder = l.EstimatedOrderHandlingFeePerOrder,
                EstimatedPickPackFeePerUnit = l.EstimatedPickPackFeePerUnit,
                EstimatedWeightHandlingFeePerUnit = l.EstimatedWeightHandlingFeePerUnit,

                IsRemoved = l.IsRemoved,
                CreateDate = l.CreateDate,
                UpdateDate = l.UpdateDate
            });
        }

        public IEnumerable<string> ProcessRemoved(IEnumerable<string> skuList)
        {
            var results = new List<string>();
            var asRemoved = GetFiltered(i => !i.IsRemoved && !skuList.Contains(i.SKU));
            foreach (var listing in asRemoved)
            {
                listing.IsRemoved = true;
                results.Add(listing.SKU);
            }
            unitOfWork.Commit();

            return results;
        }

        public void CreateOrUpdate(ListingFBAEstFeeDTO listing, DateTime? when)
        {
            var dbListing = GetFiltered(l => l.SKU == listing.SKU).FirstOrDefault();
            if (dbListing != null)
            {
                dbListing.ASIN = listing.ASIN;
                dbListing.YourPrice = listing.YourPrice;
                dbListing.SalesPrice = listing.SalesPrice;
                dbListing.Currency = listing.Currency;
                
                dbListing.EstimatedFee = listing.EstimatedFee;
                dbListing.EstimatedReferralFeePerUnit = listing.EstimatedReferralFeePerUnit;
                dbListing.EstimatedVariableClosingFee = listing.EstimatedVariableClosingFee;
                dbListing.EstimatedOrderHandlingFeePerOrder = listing.EstimatedOrderHandlingFeePerOrder;
                dbListing.EstimatedPickPackFeePerUnit = listing.EstimatedPickPackFeePerUnit;
                dbListing.EstimatedWeightHandlingFeePerUnit = listing.EstimatedWeightHandlingFeePerUnit;

                dbListing.UpdateDate = when;
            }
            else
            {
                dbListing = new ListingFBAEstFee();

                dbListing.SKU = listing.SKU;
                dbListing.ASIN = listing.ASIN;
                dbListing.YourPrice = listing.YourPrice;
                dbListing.SalesPrice = listing.SalesPrice;
                dbListing.Currency = listing.Currency;

                dbListing.EstimatedFee = listing.EstimatedFee;
                dbListing.EstimatedReferralFeePerUnit = listing.EstimatedReferralFeePerUnit;
                dbListing.EstimatedVariableClosingFee = listing.EstimatedVariableClosingFee;
                dbListing.EstimatedOrderHandlingFeePerOrder = listing.EstimatedOrderHandlingFeePerOrder;
                dbListing.EstimatedPickPackFeePerUnit = listing.EstimatedPickPackFeePerUnit;
                dbListing.EstimatedWeightHandlingFeePerUnit = listing.EstimatedWeightHandlingFeePerUnit;

                dbListing.CreateDate = when;

                unitOfWork.ListingFBAEstFees.Add(dbListing);
            }

            unitOfWork.Commit();
        }
    }
}

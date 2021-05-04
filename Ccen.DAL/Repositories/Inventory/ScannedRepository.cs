using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.ScanOrders;

namespace Amazon.DAL.Repositories.Inventory
{
    public class ScannedRepository : Repository<ViewScannedSoldQuantity>, IScannedRepository
    {
        public ScannedRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<SoldSizeInfo> GetSentInStoreQuantities()
        {
            var items = unitOfWork.GetSet<ViewScannedSoldQuantity>().Select(v => new SoldSizeInfo
            {
                StyleItemId = v.Id,
                TotalQuantity = v.InventoryQuantity ?? 0,
                SoldQuantity = v.SoldQuantity ?? 0,
                TotalSoldQuantity = v.TotalSoldQuantity ?? 0,
                Size = v.Size,
                StyleId = v.StyleId,
                MinOrderDate = v.MinOrderDate,
                MaxOrderDate = v.MaxOrderDate,
                OrderCount = v.OrderCount,
            });
            return items;
        }

        public IQueryable<SoldSizeInfo> GetSentToFBAQuantities()
        {
            var items = unitOfWork.GetSet<ViewSentToFBAQuantity>().Select(v => new SoldSizeInfo
            {
                StyleItemId = v.Id,
                TotalQuantity = v.InventoryQuantity ?? 0,
                SoldQuantity = v.SoldQuantity ?? 0,
                TotalSoldQuantity = v.TotalSoldQuantity ?? 0,
                Size = v.Size,
                StyleId = v.StyleId,
                MinOrderDate = v.MinOrderDate,
                MaxOrderDate = v.MaxOrderDate,
                OrderCount = v.OrderCount,
            });
            return items;
        }


        public IQueryable<ScanOrderDTO> GetScanOrdersAsDto()
        {
            return unitOfWork.GetSet<ViewScanOrder>().Select(s => new ScanOrderDTO()
            {
                Id = s.Id,
                Description = s.Description,
                OrderDate = s.OrderDate,
                FileName = s.FileName,
                IsFBA = s.IsFBA
            });
        }

        public IQueryable<ScanItemDTO> GetScanItemAsDto()
        {
            var subLicenseQuery = from sfv in unitOfWork.GetSet<StyleFeatureValue>()
                join fv in unitOfWork.GetSet<FeatureValue>() on sfv.FeatureValueId equals fv.Id
                where sfv.FeatureId == StyleFeatureHelper.SUB_LICENSE1
                select new
                {
                    StyleId = sfv.StyleId,
                    FeatureValue = fv.Value
                };

            var query = from m in unitOfWork.GetSet<ViewScanItemOrderMapping>()
                join i in unitOfWork.GetSet<ViewScanItem>() on m.ItemId equals i.Id
                join b in unitOfWork.GetSet<StyleItemBarcode>() on i.Barcode equals b.Barcode into withBarcode
                from b in withBarcode.DefaultIfEmpty()
                join si in unitOfWork.GetSet<StyleItem>() on b.StyleItemId equals si.Id into withStyleItems
                from si in withStyleItems.DefaultIfEmpty()
                join s in unitOfWork.GetSet<Style>() on si.StyleId equals s.Id into withStyle
                from s in withStyle.DefaultIfEmpty()
                join subL in subLicenseQuery on s.Id equals subL.StyleId into withSubLicense
                from subL in withSubLicense.DefaultIfEmpty()
                select new ScanItemDTO()
                {
                    Id = i.Id,
                    OrderId = m.OrderId,
                    Barcode = i.Barcode,
                    Quantity = m.Quantity,
                    StyleId = s.Id,
                    StyleString = s.StyleID,
                    StyleItemId = si.Id,
                    Size = si.Size,

                    StyleName = s.Name,
                    SubLicense = subL.FeatureValue,

                    CreateDate = i.CreateDate,
                };
            return query;
        }
    }
}

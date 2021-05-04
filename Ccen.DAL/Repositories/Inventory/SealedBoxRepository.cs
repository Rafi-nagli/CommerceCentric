using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core;
using Amazon.Core.Entities;
using Amazon.DTO.Inventory;

namespace Amazon.DAL.Repositories.Inventory
{
    public class SealedBoxRepository : Repository<SealedBox>, ISealedBoxRepository
    {
        public SealedBoxRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<SealedBox> GetByStyleId(long styleId)
        {
            return unitOfWork.GetSet<SealedBox>().Where(s => s.StyleId == styleId && !s.Deleted).ToList();
        }

        public IQueryable<SealedBoxDto> GetAllAsDto()
        {
            var query = from b in unitOfWork.GetSet<SealedBox>()
                        join u in unitOfWork.GetSet<User>() on b.UpdatedBy equals u.Id into withUser
                        from u in withUser.DefaultIfEmpty()
                        where !b.Deleted
                        select new SealedBoxDto()
                        {
                            Id = b.Id,
                            StyleId = b.StyleId,

                            Type = b.Type,
                            Status = b.Status,
                            LinkedPreorder = b.LinkedPreorder,

                            BoxBarcode = b.BoxBarcode,
                            BoxQuantity = b.BoxQuantity,

                            Owned = b.Owned,
                            PajamaPrice = b.PajamaPrice,

                            Deleted = b.Deleted,
                            Printed = b.Printed,
                            PolyBags = b.PolyBags,
                            
                            Archived = b.Archived,

                            ExpectedReceiptDate = b.ExpectedReceiptDate,

                            CreateDate = b.CreateDate,
                            UpdateDate = b.UpdateDate,
                            OriginCreateDate = b.OriginCreateDate,

                            UpdatedByName = u.Name,
                        };

            return query;
        }

        public void MarkAsPrintedByStyleId(long styleId)
        {
            var boxes = unitOfWork.GetSet<SealedBox>().Where(s => s.StyleId == styleId && !s.Deleted).ToList();
            foreach (var sealedBox in boxes)
            {
                sealedBox.Printed = true;
            }
            unitOfWork.Commit();
        }

        public string GetDefaultBoxName(long styleId, DateTime when)
        {
            var style = unitOfWork.Styles.Get(styleId);

            var barcode = style.StyleID.ToString() + "-" + when.ToString("MMMMyyyy");
            var existBoxesWithBarcode = GetAll().Where(s => s.BoxBarcode.Contains(barcode)).ToList();
            var index = 0;
            while (existBoxesWithBarcode.Any(b => b.BoxBarcode == barcode + ((index > 0) ? "-" + index : "")))
            {
                index++;
            }
            return barcode + ((index > 0) ? "-" + index : "");
        }

        public string GetDefaultBoxNameForBothType(long styleId, DateTime when)
        {
            var style = unitOfWork.Styles.Get(styleId);

            var barcode = style.StyleID.ToString() + "-" + when.ToString("MMMMyyyy");
            var existBoxesWithBarcode = GetAll().Where(s => s.BoxBarcode.Contains(barcode)).Select(s => s.BoxBarcode).ToList();
            existBoxesWithBarcode.AddRange(unitOfWork.GetSet<OpenBox>().Where(s => s.BoxBarcode.Contains(barcode)).Select(s => s.BoxBarcode).ToList());
            var index = 0;
            while (existBoxesWithBarcode.Any(b => b == barcode + ((index > 0) ? "-" + index : "")))
            {
                index++;
            }
            return barcode + ((index > 0) ? "-" + index : "");
        }
    }
}

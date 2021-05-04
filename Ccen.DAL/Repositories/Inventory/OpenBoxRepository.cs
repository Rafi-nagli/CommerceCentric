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
    public class OpenBoxRepository : Repository<OpenBox>, IOpenBoxRepository
    {
        public OpenBoxRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<OpenBox> GetByStyleId(long styleId)
        {
            return unitOfWork.GetSet<OpenBox>().Where(s => s.StyleId == styleId && !s.Deleted).ToList();
        }

        public IQueryable<OpenBoxDto> GetAllAsDto()
        {
            var query = from b in unitOfWork.GetSet<OpenBox>()
                join u in unitOfWork.GetSet<User>() on b.UpdatedBy equals u.Id into withUser
                from u in withUser.DefaultIfEmpty()
                where !b.Deleted
                select new OpenBoxDto()
                {
                    Id = b.Id,
                    StyleId = b.StyleId,
                    Type = b.Type,
                    Status = b.Status,
                    LinkedPreorderBox = b.LinkedPreorder,
                    BoxBarcode = b.BoxBarcode,
                    BoxQuantity = b.BoxQuantity,
                    Price = b.Price,
                    Deleted = b.Deleted,
                    Printed = b.Printed,
                    PolyBags = b.PolyBags,
                    Owned = b.Owned,
                    Archived = b.Archived,

                    ExpectedReceiptDate = b.ExpectedReceiptDate,
                    OriginCreateDate = b.OriginCreateDate,
                    CreateDate = b.CreateDate,
                    UpdateDate = b.UpdateDate,

                    UpdatedByName = u.Name,
                };

            return query;
        }

        public void MarkAsPrintedByStyleId(string styleId)
        {
            var boxes = (from b in GetAll()
                join style in unitOfWork.Styles.GetAll() on b.StyleId equals style.Id
                where style.StyleID == styleId
                select b).ToList();
            foreach (var openBox in boxes)
            {
                openBox.Printed = true;
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Core.Models;

namespace Amazon.DAL.Repositories.Inventory
{
    public class StyleItemBarcodeRepository : Repository<StyleItemBarcode>, IStyleItemBarcodeRepository
    {
        public StyleItemBarcodeRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<BarcodeDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public List<BarcodeDTO> GetByStyleItemId(long styleItemId)
        {
            return AsDto(GetFiltered(b => b.StyleItemId == styleItemId)).ToList();
        }

        private IQueryable<BarcodeDTO> AsDto(IQueryable<StyleItemBarcode> query)
        {
            return query.Select(b => new BarcodeDTO()
            {
                Id = b.Id,
                StyleItemId = b.StyleItemId,
                Barcode = b.Barcode
            });
        }

        public List<BarcodeDTO> CheckOnDuplications(IList<string> barcodes, long? excludeStyleId)
        {
            var query = from s in unitOfWork.GetSet<Style>()
                        join si in unitOfWork.GetSet<StyleItem>() on s.Id equals si.StyleId
                        join b in unitOfWork.GetSet<StyleItemBarcode>() on si.Id equals b.StyleItemId
                        where !s.Deleted
                            && barcodes.Contains(b.Barcode)
                            && s.Id != excludeStyleId
                        select new BarcodeDTO()
                        {
                            Id = b.Id,
                            Barcode = b.Barcode,
                            StyleItemId = b.StyleItemId,
                            StyleId = s.StyleID
                        };

            return query.ToList();
        }

        public IList<EntityUpdateStatus<long>> UpdateStyleItemBarcodeForStyleItem(long styleItemId,
            IList<BarcodeDTO> barcodes,
            DateTime when,
            long? by)
        {
            var results = new List<EntityUpdateStatus<long>>();

            var dbExistItems = GetFiltered(l => l.StyleItemId == styleItemId).ToList();
            var newItems = barcodes.Where(l => !l.Id.HasValue || l.Id == 0).ToList();

            foreach (var dbItem in dbExistItems)
            {
                var existItem = barcodes.FirstOrDefault(l => l.Id == dbItem.Id);
                
                if (existItem != null)
                {
                    var hasChanges = dbItem.Barcode != existItem.Barcode;

                    if (hasChanges)
                    {
                        dbItem.Barcode = existItem.Barcode;

                        dbItem.UpdateDate = when;
                        dbItem.UpdatedBy = by;

                        results.Add(new EntityUpdateStatus<long>(dbItem.Id, UpdateType.Update));
                    }
                }
                else
                {
                    Remove(dbItem);
                    results.Add(new EntityUpdateStatus<long>(dbItem.Id, UpdateType.Removed));
                }
            }

            foreach (var newItem in newItems)
            {
                var dbItem = new StyleItemBarcode()
                {
                    StyleItemId = styleItemId,
                    Barcode = newItem.Barcode,

                    CreateDate = when,
                    CreatedBy = by
                };
                Add(dbItem);
                unitOfWork.Commit();
                                
                newItem.Id = dbItem.Id;

                results.Add(new EntityUpdateStatus<long>(dbItem.Id, UpdateType.Insert));
            }

            unitOfWork.Commit();

            return results;
        }
    }
}

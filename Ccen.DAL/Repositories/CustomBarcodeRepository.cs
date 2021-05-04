using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;
using Amazon.DTO.Sizes;

namespace Amazon.DAL.Repositories
{
    public class CustomBarcodeRepository : Repository<CustomBarcode>, ICustomBarcodeRepository
    {
        public CustomBarcodeRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<CustomBarcodeDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<CustomBarcodeDTO> AsDto(IQueryable<CustomBarcode> query)
        {
            return query.Select(s => new CustomBarcodeDTO()
            {
                Id = s.Id,

                Barcode = s.Barcode,
                SKU = s.SKU,
                AttachSKUBy = s.AttachSKUBy,
                AttachSKUDate = s.AttachSKUDate,

                CreateDate = s.CreateDate
            });
        }
    }
}

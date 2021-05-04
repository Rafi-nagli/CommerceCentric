using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class ShippingMethodRepository : Repository<ShippingMethod>, IShippingMethodRepository
    {
        public ShippingMethodRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }


        public IQueryable<ShippingMethodDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }


        public ShippingMethodDTO GetByIdAsDto(int id)
        {
            return AsDto(GetAll().Where(m => m.Id == id)).FirstOrDefault();
        }

        private IQueryable<ShippingMethodDTO> AsDto(IQueryable<ShippingMethod> query)
        {
            return query.Select(i => new ShippingMethodDTO()
            {
                Id = i.Id,
                
                ShipmentProviderType = i.ShipmentProviderType,
                CarrierName = i.CarrierName,
                Name = i.Name,
                ShortName = i.ShortName ?? i.Name,
                RequiredPackageSize = i.RequiredPackageSize,
                ServiceIdentifier = i.ServiceIdentifier,

                AllowOverweight = i.AllowOverweight,
                MaxWeight = i.MaxWeight,
                IsInternational = i.IsInternational,

                StampsPackageEnumCode = i.StampsPackageEnumCode,
                StampsServiceEnumCode = i.StampsServiceEnumCode,

                CroppedLayout = i.CroppedLayout,
                RotationAngle = i.RotationAngle,
                IsCroppedLabel = i.IsCroppedLabel,
                IsFullPagePrint = i.IsFullPagePrint,

                IsSupportReturnToPOBox = i.IsSupportReturnToPOBox,
                IsActive = i.IsActive,
            });
        }
    }
}

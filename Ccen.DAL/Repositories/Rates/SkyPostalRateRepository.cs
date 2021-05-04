using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Entities.Rates;
using Amazon.DTO.Orders;

namespace Amazon.DAL.Repositories.Orders
{
    public class SkyPostalRateRepository : Repository<SkyPostalRate>, ISkyPostalRateRepository
    {
        public SkyPostalRateRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<SkyPostalRateDTO> GetAllAsDto()
        {
            return null;
            return AsDto(GetAll());
        }

        public List<SkyPostalRate> GetRateByWeightAndZones(decimal weight, List<SkyPostalZone> zones)
        {
            var zoneIds = zones.Select(x => x.ZoneId).ToList();
            var ratesByWeight = GetFiltered(x => x.Weight >= weight && x.Weight < weight+1 && zoneIds.Contains(x.ServiceTypeZoneId));
            

            return ratesByWeight.ToList();

        }

        private IQueryable<SkyPostalRateDTO> AsDto(IQueryable<SkyPostalRate> query)
        {
            return query.Select(b => new SkyPostalRateDTO()
            {
                Id = b.Id,

                //ServiceType = b.ServiceType,


                Rate = b.Rate,
                Weight = b.Weight,
            });
        }        
    }

    public class SkyPostalZoneRepository : Repository<SkyPostalZone>, ISkyPostalZoneRepository
    {
        public SkyPostalZoneRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public List<SkyPostalZone> GetZonesByZip(int zip)
        {
            var res = GetFiltered(x => zip >= x.StartZip && zip <= x.EndZip);
            if (!res.Any())
            {
                throw new ApplicationException("Error Db or wrong input parameters");
            }
            return res.ToList();
        }
    }
}

    

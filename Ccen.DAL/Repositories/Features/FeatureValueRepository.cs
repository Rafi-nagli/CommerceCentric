using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Features;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.DAL.Repositories
{
    public class FeatureValueRepository : Repository<FeatureValue>, IFeatureValueRepository
    {
        public FeatureValueRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Get all posible feature values for item type
        /// </summary>
        /// <param name="itemTypeId"></param>
        /// <returns></returns>
        public IList<FeatureValueDTO> GetAllFeatureValueByItemType(int itemTypeId)
        {
            var query = from fv in unitOfWork.GetSet<FeatureValue>()
                join f in unitOfWork.GetSet<Feature>() on fv.FeatureId equals f.Id
                where f.ItemTypeId == itemTypeId
                    || !f.ItemTypeId.HasValue
                orderby f.Order ascending, f.Name ascending 
                select fv;
            
            return AsDto(query).ToList();
        }

        public FeatureValueDTO GetValueByStyleAndFeatureId(long styleId, int featureId)
        {
            var query = from fv in unitOfWork.GetSet<StyleFeatureValue>()
                join v in unitOfWork.GetSet<FeatureValue>() on fv.FeatureValueId equals v.Id
                where fv.StyleId == styleId && v.FeatureId == featureId
                select v;

            return AsDto(query).FirstOrDefault();
        }

        public IList<FeatureValueDTO> GetValuesByStyleIds(IList<long> styleIdList)
        {
            var query = from fv in unitOfWork.GetSet<StyleFeatureValue>()
                        join v in unitOfWork.GetSet<FeatureValue>() on fv.FeatureValueId equals v.Id
                        join f in unitOfWork.GetSet<Feature>() on fv.FeatureId equals f.Id
                        where styleIdList.Contains(fv.StyleId)
                        select new FeatureValueDTO()
                        {
                            Id = v.Id,
                            Value = v.Value,
                            ExtendedValue = v.ExtendedValue,
                            ExtendedData = v.ExtendedData,
                            Order = v.Order,
                            FeatureId = v.FeatureId,
                            FeatureName = f.Name,                            
                            StyleId = fv.StyleId,
                        };

            return query.ToList();
        }
        
         /// <summary>
        /// Get all feature values for specified feature
        /// </summary>
        /// <param name="featureId"></param>
        /// <returns></returns>
        public IList<FeatureValueDTO> GetValuesByFeatureId(int featureId)
        {
            return AsDto(unitOfWork.GetSet<FeatureValue>().Where(v => v.FeatureId == featureId)).ToList();
        }

        public IQueryable<FeatureValueDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        private IQueryable<FeatureValueDTO> AsDto(IQueryable<FeatureValue> query)
        {
            return query.Select(f => new FeatureValueDTO()
            {
                Id = f.Id,
                FeatureId = f.FeatureId,
                DisplayValue = f.DisplayValue,
                Value = f.Value,
                ExtendedValue = f.ExtendedValue,
                ExtendedData = f.ExtendedData,
                IsRequiredManufactureBarcode = f.IsRequiredManufactureBarcode,
                Order = f.Order
            });
        }
    }
}

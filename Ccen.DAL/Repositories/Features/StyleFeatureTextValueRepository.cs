using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Features;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class StyleFeatureTextValueRepository : Repository<StyleFeatureTextValue>, IStyleFeatureTextValueRepository
    {
        public StyleFeatureTextValueRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<StyleFeatureValueDTO> GetFeatureTextValueForAllStyleByFeatureId(int[] featureIdList)
        {
            var query = from sfv in unitOfWork.GetSet<StyleFeatureTextValue>()
                        join f in unitOfWork.GetSet<Feature>() on sfv.FeatureId equals f.Id
                        where featureIdList.Contains(sfv.FeatureId)
                        select new StyleFeatureValueDTO()
                        {
                            Id = sfv.Id,
                            FeatureId = sfv.FeatureId,
                            FeatureName = f.Name,
                            StyleId = sfv.StyleId,

                            Value = sfv.Value
                        };
            return query.ToList();
        }

        public IList<StyleFeatureValueDTO> GetAllFeatureTextValuesByStyleIdAsDto(IList<long> styleIdList)
        {
            return GetAllWithFeature()
                .Where(f => styleIdList.Contains(f.StyleId))
                .Select(f => f).ToList();
        }

        public IQueryable<StyleFeatureValueDTO> GetAllWithFeature()
        {
            return from sf in unitOfWork.GetSet<StyleFeatureTextValue>()
                   join f in unitOfWork.GetSet<Feature>() on sf.FeatureId equals f.Id
                   select new StyleFeatureValueDTO()
                   {
                       Id = f.Id,
                       FeatureId = sf.FeatureId,
                       FeatureName = f.Name,
                       StyleId = sf.StyleId,
                       Type = f.ValuesType,
                       Value = sf.Value
                   };
        }

        public StyleFeatureValueDTO GetFeatureValueByStyleIdByFeatureId(long styleId, int featureId)
        {
            return GetFeatureValueByStyleIdByFeatureId(new List<long>() { styleId }, new int[] { featureId }).FirstOrDefault();
        }

        public IList<StyleFeatureValueDTO> GetFeatureValueByStyleIdByFeatureId(IList<long> styleIdList,
            int[] featureIdList)
        {
            var query = from sfv in unitOfWork.GetSet<StyleFeatureTextValue>()
                        where styleIdList.Contains(sfv.StyleId)
                            && featureIdList.Contains(sfv.FeatureId)
                        select new StyleFeatureValueDTO
                        {
                            Id = sfv.Id,
                            StyleId = sfv.StyleId,
                            FeatureId = sfv.FeatureId,

                            //FeatureValueId = sfv.Id,
                            Value = sfv.Value,
                        };
            return query.ToList();
        }

    }
}

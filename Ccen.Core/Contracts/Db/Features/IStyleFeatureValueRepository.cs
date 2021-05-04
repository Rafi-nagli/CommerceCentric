using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Features;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IStyleFeatureValueRepository : IRepository<StyleFeatureValue>
    {
        IList<StyleFeatureValueDTO> GetFeatureValueForAllStyleByFeatureId(int[] featureIdList);

        IList<StyleFeatureValueDTO> GetFeatureValueByStyleIdByFeatureId(IList<long> styleIdList,
            int[] featureIdList);
        StyleFeatureValueDTO GetFeatureValueByStyleIdByFeatureId(long styleId,
            int featureId);

        IList<StyleFeatureValueDTO> GetAllFeatureValuesByStyleIdAsDto(IList<long> styleIdList);
        
        bool UpdateFeatureValues(long styleId,
            IList<StyleFeatureValueDTO> featureValues,
            DateTime? when,
            long? by);

        IQueryable<StyleFeatureValueDTO> GetAllWithFeature();
    }
}

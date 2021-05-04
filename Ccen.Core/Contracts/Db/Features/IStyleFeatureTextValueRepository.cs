using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Features;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IStyleFeatureTextValueRepository : IRepository<StyleFeatureTextValue>
    {
        StyleFeatureValueDTO GetFeatureValueByStyleIdByFeatureId(long styleId, int featureId);
        IList<StyleFeatureValueDTO> GetFeatureTextValueForAllStyleByFeatureId(int[] featureIdList);
        IList<StyleFeatureValueDTO> GetAllFeatureTextValuesByStyleIdAsDto(IList<long> styleIdList);

        IQueryable<StyleFeatureValueDTO> GetAllWithFeature();
    }
}

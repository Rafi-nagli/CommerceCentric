using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities.Features;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class StyleFeatureValueRepository : Repository<StyleFeatureValue>, IStyleFeatureValueRepository
    {
        public StyleFeatureValueRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public IList<StyleFeatureValueDTO> GetFeatureValueForAllStyleByFeatureId(int[] featureIdList)
        {
            var query = from sfv in unitOfWork.GetSet<StyleFeatureValue>()
                        join fv in unitOfWork.GetSet<FeatureValue>() on sfv.FeatureValueId equals fv.Id
                        join f in unitOfWork.GetSet<Feature>() on fv.FeatureId equals f.Id
                        where featureIdList.Contains(fv.FeatureId)
                        select new StyleFeatureValueDTO
                        {
                            Id = fv.Id,
                            StyleId = sfv.StyleId,
                            FeatureId = fv.FeatureId,
                            FeatureName = f.Name,

                            FeatureValueId = fv.Id,
                            Value = fv.Value,
                        };
            return query.ToList();
        }

        public StyleFeatureValueDTO GetFeatureValueByStyleIdByFeatureId(long styleId,
            int featureId)
        {
            return GetFeatureValueByStyleIdByFeatureId(new List<long>() {styleId}, new int[] { featureId }).FirstOrDefault();
        }

        public IList<StyleFeatureValueDTO> GetFeatureValueByStyleIdByFeatureId(IList<long> styleIdList,
            int[] featureIdList)
        {
            var query = from sfv in unitOfWork.GetSet<StyleFeatureValue>()
                        join fv in unitOfWork.GetSet<FeatureValue>() on sfv.FeatureValueId equals fv.Id
                        where styleIdList.Contains(sfv.StyleId)
                            && featureIdList.Contains(fv.FeatureId)
                        select new StyleFeatureValueDTO
                        {
                            Id = fv.Id,
                            StyleId = sfv.StyleId,
                            FeatureId = fv.FeatureId,

                            FeatureValueId = fv.Id,
                            Value = fv.Value,
                        };
            return query.ToList();
        }


        
        public IList<StyleFeatureValueDTO> GetAllFeatureValuesByStyleIdAsDto(IList<long> styleIdList)
        {
            var query = from f in GetAllWithFeature()
                        where styleIdList.Contains(f.StyleId)
                        select f;

            return query.ToList();
        }

        public IQueryable<StyleFeatureValueDTO> GetAllWithFeature()
        {
            var query = from sfv in unitOfWork.GetSet<StyleFeatureValue>()
                        join fv in unitOfWork.GetSet<FeatureValue>() on sfv.FeatureValueId equals fv.Id
                        join f in unitOfWork.GetSet<Feature>() on fv.FeatureId equals f.Id
                        select new StyleFeatureValueDTO()
                        {
                            Id = sfv.Id,
                            StyleId = sfv.StyleId,
                            FeatureId = fv.FeatureId,
                            FeatureName = f.Name,
                            FeatureValueId = fv.Id,
                            Type = f.ValuesType,
                            Value = fv.Value
                        };

            return query;
        }

        public bool UpdateFeatureValues(long styleId,
            IList<StyleFeatureValueDTO> featureValues,
            DateTime? when,
            long? by)
        {
            var values = unitOfWork.GetSet<StyleFeatureValue>().Where(f => f.StyleId == styleId).ToList();
            var textValues = unitOfWork.GetSet<StyleFeatureTextValue>().Where(f => f.StyleId == styleId).ToList();

            var hasChanges = false;

            foreach (var feature in featureValues)
            {
                if (feature.Type == (int)FeatureValuesType.DropDown || feature.Type == (int)FeatureValuesType.CacadeDropDown)
                {
                    var existValue = values.FirstOrDefault(f => f.FeatureId == feature.FeatureId);
                    if (existValue == null && feature.FeatureValueId.HasValue)
                    {
                        hasChanges = true;
                        Add(new StyleFeatureValue
                        {
                            FeatureId = feature.FeatureId,
                            FeatureValueId = feature.FeatureValueId.Value,
                            StyleId = styleId,
                            CreateDate = when,
                            CreatedBy = by
                        });
                    }
                    else
                    {
                        if (existValue != null)
                        {
                            if (feature.FeatureValueId.HasValue)
                            {
                                var hasFeatureChanges = existValue.FeatureValueId != feature.FeatureValueId.Value;

                                if (hasFeatureChanges)
                                {
                                    existValue.FeatureValueId = feature.FeatureValueId.Value;
                                    existValue.UpdateDate = when;
                                    existValue.UpdatedBy = by;
                                }

                                hasChanges = hasChanges || hasFeatureChanges;
                            }
                            else
                            {
                                Remove(existValue);
                                hasChanges = true;
                            }
                        }
                    }
                }

                if (feature.Type == (int)FeatureValuesType.TextBox || feature.Type == (int)FeatureValuesType.TextArea
                    || feature.Type == (int)FeatureValuesType.CheckBox)
                {
                    var existTextValue = textValues.FirstOrDefault(f => f.FeatureId == feature.FeatureId);
                    if (existTextValue == null)
                    {
                        unitOfWork.StyleFeatureTextValues.Add(new StyleFeatureTextValue
                        {
                            FeatureId = feature.FeatureId,
                            Value = feature.Value,
                            StyleId = styleId,
                            CreateDate = when,
                            CreatedBy = by
                        });
                        hasChanges = true;
                    }
                    else
                    {
                        if (existTextValue.Value != feature.Value)
                        {
                            hasChanges = true;

                            existTextValue.Value = feature.Value;
                            existTextValue.UpdateDate = when;
                            existTextValue.UpdatedBy = by;
                        }
                    }
                }
            }

            var valueToRemove = values.Where(fv => featureValues.All(f => f.FeatureId != fv.FeatureId)).ToList();
            var textValueToRemove = textValues.Where(fv => featureValues.All(f => f.FeatureId != fv.FeatureId)).ToList();

            foreach (var value in valueToRemove)
            {
                Remove(value);
                hasChanges = true;
            }

            foreach (var textValue in textValueToRemove)
            {
                unitOfWork.StyleFeatureTextValues.Remove(textValue);
                hasChanges = true;
            }

            unitOfWork.Commit();

            return hasChanges;
        }
    }
}

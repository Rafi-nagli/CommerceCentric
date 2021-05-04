using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Features;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Graphs;

namespace Amazon.Web.ViewModels.Graph
{
    public class ItemsByFeatureTypeGraphViewModel
    {
        public enum ValueType
        {
            Styles,
            Units,
        }

        public IList<IList<int>> StyleSeries { get; set; }
        public IList<IList<int>> UnitSeries { get; set; }
        public IList<LabelInfo> Labels { get; set; }

        public class LabelInfo
        {
            public long? Id { get; set; }
            public string Name { get; set; }
        }

        public static ItemsByFeatureTypeGraphViewModel Build(IUnitOfWork db, 
            ValueType valueType,
            int featureId,
            int? selectedFeatureId)
        {
            if (selectedFeatureId.HasValue)
                return GetBySubLicense(db, selectedFeatureId.Value);
            else
                return GetByFeatureId(db, featureId);
        }

        private static ItemsByFeatureTypeGraphViewModel GetBySubLicense(IUnitOfWork db, int selectedFeatureId)
        {
            var result = new ItemsByFeatureTypeGraphViewModel();

            var featureId = StyleFeatureHelper.SUB_LICENSE1;

            //Total main feature qty/price
            var mainFeatureId = StyleFeatureHelper.MAIN_LICENSE;
            var mainFeatures = from fv in db.StyleFeatureValues.GetAll()
                                   where fv.FeatureId == mainFeatureId
                                   select fv;

            var mainFeatureQuery = from s in db.Styles.GetAll()
                join sc in db.StyleCaches.GetAll() on s.Id equals sc.Id
                join si in db.StyleItems.GetAll() on s.Id equals si.StyleId
                join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id
                join fv in mainFeatures on s.Id equals fv.StyleId into withFeatures
                from fv in withFeatures.DefaultIfEmpty()
                join fvn in db.FeatureValues.GetAll() on fv.FeatureValueId equals fvn.Id into withFeatureValues
                from fvn in withFeatureValues.DefaultIfEmpty()
                where !s.Deleted
                    && fv.FeatureValueId == selectedFeatureId
                group new { fv, sic } by fv.FeatureId into byFeatureId
                select new
                {
                    Quantity = byFeatureId.Sum(j => j.sic.RemainingQuantity > 0 ? j.sic.RemainingQuantity : 0),
                    StyleCount = byFeatureId.Select(j => j.sic.StyleId).Distinct().Count()
                };

            var mainFeatureInfo = mainFeatureQuery.FirstOrDefault();


            var selectedFeatures = from fv in db.StyleFeatureValues.GetAll()
                                   where fv.FeatureId == featureId 
                                   select fv;

            var query = from s in db.Styles.GetAll()
                        join sc in db.StyleCaches.GetAll() on s.Id equals sc.Id
                        join si in db.StyleItems.GetAll() on s.Id equals si.StyleId
                        join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id
                        join fv in selectedFeatures on s.Id equals fv.StyleId into withFeatures
                        from fv in withFeatures.DefaultIfEmpty()
                        join fvn in db.FeatureValues.GetAll() on fv.FeatureValueId equals fvn.Id into withFeatureValues
                        from fvn in withFeatureValues.DefaultIfEmpty()
                        where !s.Deleted
                        select new
                        {
                            StyleId = s.Id,
                            FeatureName = fvn.Value,
                            FeatureId = (long?)fv.FeatureValueId,
                            ParentFeatureId = fvn.ExtendedValue,
                            Quantity = sic.RemainingQuantity,
                        };

            var items = query.ToList();

            var bySleeve = items.GroupBy(i => i.FeatureName).Select(i => new
            {
                FeatureName = i.Key,
                FeatureId = i.Max(j => (long?)j.FeatureId),
                ParentFeatureId = i.Max(j => j.ParentFeatureId),
                StyleCount = i.Select(j => j.StyleId).Distinct().Count(),
                Quantity = i.Sum(j => j.Quantity < 0 ? 0 : j.Quantity)
            })
            .Where(i => i.ParentFeatureId == selectedFeatureId.ToString())
            .ToList();

            var labels = new List<LabelInfo>();
            IList<int> unitSeries = new List<int>();
            IList<int> styleCountSeries = new List<int>();

            foreach (var item in bySleeve)
            {
                labels.Add(new LabelInfo()
                {
                    Name = String.IsNullOrEmpty(item.FeatureName) ? "n/a" : item.FeatureName,
                    Id = item.FeatureId
                });
                unitSeries.Add(item.Quantity);
                styleCountSeries.Add(item.StyleCount);
            }

            if (mainFeatureInfo != null)
            {
                var sumQuantity = bySleeve.Sum(j => j.Quantity < 0 ? 0 : j.Quantity);
                var sumStyleCount = bySleeve.Sum(i => i.StyleCount);

                labels.Add(new LabelInfo()
                {
                    Name = "n/a",
                    Id = null,
                });
                unitSeries.Add(mainFeatureInfo.Quantity - sumQuantity);
                styleCountSeries.Add(mainFeatureInfo.StyleCount - sumStyleCount);
            }

            result.StyleSeries = new[] { styleCountSeries };
            result.UnitSeries = new[] { unitSeries };
            result.Labels = labels;
            return result;
        }

        private static ItemsByFeatureTypeGraphViewModel GetByFeatureId(IUnitOfWork db, int featureId)
        {
            var result = new ItemsByFeatureTypeGraphViewModel();

            var selectedFeatures = from fv in db.StyleFeatureValues.GetAll()
                                   where fv.FeatureId == featureId //Feature.ITEMSTYLE
                                   select fv;

            var query = from s in db.Styles.GetAll()
                        join sc in db.StyleCaches.GetAll() on s.Id equals sc.Id
                        join si in db.StyleItems.GetAll() on s.Id equals si.StyleId
                        join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id
                        join fv in selectedFeatures on s.Id equals fv.StyleId into withFeatures
                        from fv in withFeatures.DefaultIfEmpty()
                        join fvn in db.FeatureValues.GetAll() on fv.FeatureValueId equals fvn.Id into withFeatureValues
                        from fvn in withFeatureValues.DefaultIfEmpty()
                        where !s.Deleted
                        select new
                        {
                            StyleId = s.Id,
                            FeatureName = fvn.Value,
                            FeatureId = (long?)fv.FeatureValueId,
                            Quantity = sic.RemainingQuantity,
                        };

            var items = query.ToList();

            var bySleeve = items.GroupBy(i => i.FeatureName).Select(i => new
            {
                FeatureName = i.Key,
                FeatureId = i.Max(j => (long?)j.FeatureId),
                StyleCount = i.Select(j => j.StyleId).Distinct().Count(),
                Quantity = i.Sum(j => j.Quantity < 0 ? 0 : j.Quantity)
            }).ToList();

            var labels = new List<LabelInfo>();
            IList<int> unitSeries = new List<int>();
            IList<int> styleCountSeries = new List<int>();

            foreach (var item in bySleeve)
            {
                labels.Add(new LabelInfo()
                {
                    Name = item.FeatureName ?? "n/a",
                    Id = item.FeatureId
                });
                unitSeries.Add(item.Quantity);
                styleCountSeries.Add(item.StyleCount);
            }
            result.StyleSeries = new[] { styleCountSeries };
            result.UnitSeries = new[] { unitSeries };
            result.Labels = labels;
            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Entities.Features;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Web.ViewModels.Html;

namespace Amazon.Web.ViewModels.Inventory
{
    public class FeatureViewModel
    {
        public int FeatureId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
        public string ExtendedValue { get; set; }
        public bool Disabled { get; set; }

        public int Type { get; set; }
        public IList<SelectListItemEx> Items { get; set; }
        public string Value { get; set; }

        public bool ValueAsBool
        {
            get { return Value == "1"; }
            set { Value = value ? "1" : "0"; }
        }

        public int? ValueAsInt
        {
            get
            {
                int result = 0;
                if (Value != null)
                    if (Int32.TryParse(Value, out result))
                        return result;
                return null;
            }
        }

        public FeatureViewModel()
        {

        }

        public FeatureViewModel(FeatureDTO feature, IList<FeatureValueDTO> values, string value)
        {
            FeatureId = feature.Id;
            Name = feature.Name;
            Title = feature.Title ?? feature.Name;
            Notes = feature.Notes;
            Value = value;
            Type = feature.ValuesType;
            ExtendedValue = feature.ExtendedValue;

            if (values != null && values.Count > 0)
            {
                long intValue = ValueAsInt ?? 0;
                Items = values.OrderBy(v => v.Order)
                    .Select(v => new SelectListItemEx
                    {
                        Selected = v.Id == intValue,
                        Text = v.Value,
                        ParentValue = v.ExtendedValue,
                        Value = v.Id.ToString()
                    })
                    .ToList();
            }
            else
            {
                Items = new List<SelectListItemEx>();
            }
        }

        public static List<FeatureViewModel> BuildFrom(IList<FeatureDTO> features,
            IList<FeatureValueDTO> allFeatureValues,
            IList<StyleFeatureValueDTO> styleFeatureValues,
            IList<StyleFeatureValueDTO> styleTextFeatureValues)
        {
            var results = new List<FeatureViewModel>();
            foreach (var feature in features)
            {
                var featureValues = allFeatureValues.Where(f => f.FeatureId == feature.Id).OrderBy(f => f.Value).ToList();
                string value = null;
                var styleValue = styleFeatureValues.FirstOrDefault(f => f.FeatureId == feature.Id);
                if (styleValue != null)
                {
                    value = styleValue.FeatureValueId.ToString();
                }
                var styleTextValue = styleTextFeatureValues.FirstOrDefault(f => f.FeatureId == feature.Id);
                if (styleTextValue != null)
                {
                    value = styleTextValue.Value;
                }
                results.Add(new FeatureViewModel(feature, featureValues, value));
            }
            return results;
        }

        public static IList<SelectListItemEx> GetFeatureValuesByName(IUnitOfWork db, string featureName)
        {
            var f = db.Features.GetAllAsDto().First(x => x.Name == featureName);
            if (f == null)
            {
                throw new ArgumentException("No feature found for by name", "featureName");
            }
            return db.FeatureValues.GetValuesByFeatureId(f.Id).Select(x => new SelectListItemEx() { ParentValue = f.Id.ToString(), Value = x.Id.ToString(), Text = x.Value }).ToList();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Entities.Features;
using Amazon.Core.Models.Inventory;
using Amazon.Common.Helpers;

namespace Amazon.Web.ViewModels
{
    public class LicenseViewModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string WMCharacter { get; set; }
        public bool WMCharacterPermanent { get; set; }
        public bool IsRequiredManufactureBarcode { get; set; }

        public List<LicenseViewModel> SubLicenses { get; set; }

        public override string ToString()
        {
            return "Id=" + Id + ", ParentId=" + ParentId + ", Name=" + Name + ", IsRequiredManufactureBarcode=" + IsRequiredManufactureBarcode;
        }

        public static IEnumerable<LicenseViewModel> GetLicenses(IUnitOfWork db)
        {
            var featureValues = db.FeatureValues.GetValuesByFeatureId(12).ToList();
            var results = new List<LicenseViewModel>();
            foreach (var fv in featureValues)
            {
                var exAttributes = JsonHelper.Deserialize<FeatureExAttributes>(fv.ExtendedData);
                results.Add(new LicenseViewModel()
                {
                    Id = fv.Id,
                    Name = fv.Value,
                    IsRequiredManufactureBarcode = (exAttributes?.IsRequiredManufactureBarcode ?? false) 
                        || fv.IsRequiredManufactureBarcode,
                });
            }
            return results;
        }

        public static IEnumerable<LicenseViewModel> GetSubLicenses(IUnitOfWork db, int licenseId)
        {
            var featureValues = db.FeatureValues.GetValuesByFeatureId(13)
                    .Where(l => l.ExtendedValue == licenseId.ToString())
                    .OrderBy(l => l.Value)
                    .ToList();

            var results = new List<LicenseViewModel>();
            foreach (var fv in featureValues)
            {
                var exAttributes = JsonHelper.Deserialize<FeatureExAttributes>(fv.ExtendedData);
                results.Add(new LicenseViewModel()
                {
                    Id = fv.Id,
                    Name = fv.Value,
                    ParentId = int.Parse(fv.ExtendedValue),
                    WMCharacter = exAttributes?.WMCharacter,
                    WMCharacterPermanent = exAttributes?.WMCharacterPermanent ?? false,
                });
            }
            return results;
        }

        public void AddAsParent(IUnitOfWork db)
        {
            var mainFeatureValue = new FeatureValue
            {
                FeatureId = 12,
                Value = Name,
                IsRequiredManufactureBarcode = IsRequiredManufactureBarcode,
                Order = 0,
                ExtendedData = JsonHelper.Serialize<FeatureExAttributes>(new FeatureExAttributes()
                {
                    IsRequiredManufactureBarcode = IsRequiredManufactureBarcode,
                }),
            };
            db.FeatureValues.Add(mainFeatureValue);
            db.Commit();

            Id = mainFeatureValue.Id;
        }

        public void UpdateAsParent(IUnitOfWork db)
        {
            var license = db.FeatureValues.Get(Id);
            if (license != null)
            {
                license.Value = Name;
                license.IsRequiredManufactureBarcode = IsRequiredManufactureBarcode;
                license.ExtendedData = JsonHelper.Serialize<FeatureExAttributes>(new FeatureExAttributes()
                {
                    IsRequiredManufactureBarcode = IsRequiredManufactureBarcode,
                });
                db.Commit();
            }
        }

        public void AddAsChild(IUnitOfWork db)
        {
            var child = new FeatureValue
            {
                FeatureId = 13,
                Value = Name,
                ExtendedValue = ParentId.ToString(),
                ExtendedData = JsonHelper.Serialize<FeatureExAttributes>(new FeatureExAttributes()
                {
                    WMCharacter = WMCharacter,
                    WMCharacterPermanent = WMCharacterPermanent,
                })
            };
            db.FeatureValues.Add(child);
            db.Commit();
            Id = child.Id;
        }

        public void UpdateAsChild(IUnitOfWork db)
        {
            var license = db.FeatureValues.Get(Id);
            if (license != null)
            {
                license.Value = Name;
                license.ExtendedData = JsonHelper.Serialize<FeatureExAttributes>(new FeatureExAttributes()
                {
                    WMCharacter = WMCharacter,
                    WMCharacterPermanent = WMCharacterPermanent,
                });
                db.Commit();
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Features;
using Amazon.Core.Models;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Inventory;

namespace Amazon.Web.ViewModels.Pages
{
    public class ItemPricePageViewModel
    {
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public ListingsModeType ListingsMode { get; set; }

        public string LastListingsSyncDate { get; set; }
        public bool ListingsSyncRequested { get; set; }
        public string ParentASIN { get; set; }

        public string StyleId { get; set; }

        public SelectList GenderList { get; set; }
        public IList<SelectListItemEx> MainLicenseList { get; set; }
        public IList<SelectListItemEx> SubLicenseList { get; set; }
        public List<string> SizeList { get; set; }


        public void Init(IUnitOfWork db, ISettingsService settings)
        {
            var syncRequested = settings.GetListingsManualSyncRequest(Market, MarketplaceId);
            var lastSyncData = settings.GetListingsSendDate(Market, MarketplaceId);

            LastListingsSyncDate = DateHelper.ToDateTimeString(DateHelper.ConvertUtcToApp(lastSyncData));
            ListingsSyncRequested = syncRequested ?? false;

            GenderList = new SelectList(db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.GENDER).ToList(), "Id", "Value");

            MainLicenseList = db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.MAIN_LICENSE).OrderBy(l => l.Value).Select(fv => new SelectListItemEx
            {
                Text = fv.Value,
                Value = fv.Id.ToString(),
                ParentValue = fv.Id.ToString()
            }).ToList();

            SubLicenseList = db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.SUB_LICENSE1).OrderBy(l => l.Value).Select(fv => new SelectListItemEx
            {
                Text = fv.Value,
                Value = fv.Id.ToString(),
                ParentValue = fv.ExtendedValue.ToString()
            }).ToList();

            var sizes = db.Sizes.GetAllAsDto();
            SizeList = sizes.Select(s => s.Name).ToList();
        }
    }
}
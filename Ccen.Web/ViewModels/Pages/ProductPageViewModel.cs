using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Features;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Inventory;

namespace Amazon.Web.ViewModels.Pages
{
    public class ProductPageViewModel
    {
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public ListingsModeType ListingsMode { get; set; }

        public string LastListingsSyncDate { get; set; }
        public bool ListingsSyncRequested { get; set; }

        public bool SyncPauseStatus { get; set; }

        public string ParentASIN { get; set; }

        public int? PublishedStatus { get; set; }

        public string StyleId { get; set; }

        public SelectList GenderList { get; set; }
        public IList<SelectListItemEx> MainLicenseList { get; set; }
        public IList<SelectListItemEx> SubLicenseList { get; set; }
        public List<string> SizeList { get; set; }


        public string MarketName
        {
            get { return MarketHelper.GetShortName((int)Market, MarketplaceId); }
        }

        public SelectList PublishedStatusList
        {
            get
            {
                return new SelectList(PublishedStatusAsArray, "Value", "Key");
            }
        }

        public List<KeyValuePair<string, int>> PublishedStatusAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.Published), (int)PublishedStatuses.Published),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.HasChanges), (int)PublishedStatuses.HasChanges),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.New), (int)PublishedStatuses.New),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.ChangesSubmited), (int)PublishedStatuses.ChangesSubmited),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.PublishedInProgress), (int)PublishedStatuses.PublishedInProgress),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.PublishingErrors), (int)PublishedStatuses.PublishingErrors),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.PublishedInactive), (int)PublishedStatuses.PublishedInactive),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.HasUnpublishRequest), (int)PublishedStatuses.HasUnpublishRequest),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.Unpublished), (int)PublishedStatuses.Unpublished),
        };


        public void Init(IUnitOfWork db, ISettingsService settings)
        {
            var syncRequested = settings.GetListingsManualSyncRequest(Market, MarketplaceId);
            var lastSyncData = settings.GetListingsSendDate(Market, MarketplaceId);

            LastListingsSyncDate = DateHelper.ToDateTimeString(DateHelper.ConvertUtcToApp(lastSyncData));
            ListingsSyncRequested = syncRequested ?? false;
            SyncPauseStatus = settings.GetListingsSyncPause(Market, MarketplaceId) ?? false;
            
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
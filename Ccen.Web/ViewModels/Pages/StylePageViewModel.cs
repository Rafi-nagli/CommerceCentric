using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Features;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Inventory;

namespace Amazon.Web.ViewModels.Pages
{
    public class StylePageViewModel
    {
        public string SelectedStyleId { get; set; }

        public SelectList GenderList { get; set; }
        public SelectList ItemStyleList { get; set; }
        public SelectList SleeveList { get; set; }
        public SelectList HolidayList { get; set; }

        public SelectList PictureStatusList { get; set; }

        public IList<SelectListItemEx> MainLicenseList { get; set; }
        public IList<SelectListItemEx> SubLicenseList { get; set; }

        public SelectList OnlineStatusList { get; set; }
        public SelectList MarketplaceList { get; set; }
        public SelectList DropShipperList { get; set; }

        public void Init(IUnitOfWork db)
        {
            GenderList = new SelectList(db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.GENDER).ToList(), "Id", "Value");

            ItemStyleList = new SelectList(db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.ITEMSTYLE).ToList(), "Id", "Value");

            SleeveList = new SelectList(db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.SLEEVE).ToList(), "Id", "Value");

            DropShipperList = OptionsHelper.DropShipperList;

            HolidayList = new SelectList(db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.HOLIDAY).ToList(), "Id", "Value");

            var marketplaceList = db.Marketplaces.GetAllAsDto().Select(m => new SelectListItem()
            {
                Text = MarketHelper.GetMarketName(m.Market, m.MarketplaceId),
                Value = m.Market + ";" + m.MarketplaceId
            });
            MarketplaceList = new SelectList(marketplaceList, "Value", "Text");

            OnlineStatusList = new SelectList(new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Text = "Online",
                    Value = "Online",
                },
                new SelectListItem()
                {
                    Text = "Offline",
                    Value = "Offline"
                }
            },
            "Value", 
            "Text");

            MainLicenseList = db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.MAIN_LICENSE).OrderBy(l => l.Value).Select(fv => new SelectListItemEx
            {
                Text = fv.Value,
                Value = fv.Id.ToString(),
                ParentValue = fv.Id.ToString()
            }).ToList();

            MainLicenseList.Insert(0, new SelectListItemEx()
            {
                Text = "No License",
                Value = "0",
                ParentValue = "0",
            });

            SubLicenseList = db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.SUB_LICENSE1).OrderBy(l => l.Value).Select(fv => new SelectListItemEx
            {
                Text = fv.Value,
                Value = fv.Id.ToString(),
                ParentValue = fv.ExtendedValue.ToString()
            }).ToList();
        }
    }
}
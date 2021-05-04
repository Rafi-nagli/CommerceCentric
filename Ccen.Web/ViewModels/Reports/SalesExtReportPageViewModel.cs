using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.ViewModels.Reports
{
    public class SalesExtReportPageViewModel
    {
        public SelectList GenderList { get; set; }
        public SelectList ItemStyleList { get; set; }
        
        public IList<SelectListItemEx> MainLicenseList { get; set; }
        public IList<SelectListItemEx> SubLicenseList { get; set; }

        public SelectList MarketplaceList { get; set; }
        
        public void Init(IUnitOfWork db)
        {
            GenderList = new SelectList(db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.GENDER).ToList(), "Id", "Value");

            ItemStyleList = new SelectList(db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.ITEMSTYLE).ToList(), "Id", "Value");

            var marketplaceList = db.Marketplaces.GetAllAsDto().Where(m => m.IsActive).Select(m => new SelectListItem()
            {
                Text = MarketHelper.GetMarketName(m.Market, m.MarketplaceId),
                Value = m.Market + "_" + m.MarketplaceId
            });
            MarketplaceList = new SelectList(marketplaceList, "Value", "Text");

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
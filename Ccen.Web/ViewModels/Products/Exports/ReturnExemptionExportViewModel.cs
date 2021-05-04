using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Api.Exports;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Items;
using Amazon.Core.Models.SystemActions;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Listings;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.ViewModels.ExcelToAmazon;
using Amazon.Web.ViewModels.Results;
using Kendo.Mvc.Extensions;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Core.Exports.Attributes;

namespace Amazon.Web.ViewModels.Products
{
    public class ReturnExemptionExportViewModel
    {
        public static string TemplatePath = "~/App_Data/Prepaid_returns_exemption_template.xlsx";

        [ExcelSerializable("SKU", Order = 0, Width = 25)]
        public string SKU { get; set; }

        [ExcelSerializable("PrePaidLabel", Order = 1, Width = 25)]
        public string PrePaidLabel { get; set; }

        [ExcelSerializable("ReasonCode", Order = 2, Width = 25)]
        public string ReasonCode { get; set; }

        [ExcelSerializable("Comment", Order = 3, Width = 25)]
        public string Comment { get; set; }


        public ReturnExemptionExportViewModel()
        {
            
        }
        
        public static MemoryStream Export(IUnitOfWork db,
            ITime time,
            ILogService log,
            DateTime when, 
            long? by,
            out string filename)
        {
            log.Info("Export Exemption");

            filename = "ExportReturnExemption_" + time.GetAppNowTime().ToString("yyyyMMddHHmmss") + ".xlsx";

            var underwearSKUs = (from l in db.Listings.GetAll()
                             join i in db.Items.GetAll() on l.ItemId equals i.Id
                             join st in db.Styles.GetAll() on i.StyleId equals st.Id
                             join t in db.StyleFeatureValues.GetAll() on st.Id equals t.StyleId
                             where !l.IsRemoved
                                && l.Market == (int)MarketType.Amazon
                                && l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                && t.FeatureValueId == 342 //NOTE: ItemStyle, Underwear
                                && l.RealQuantity > 0
                             select l.SKU).ToList();

            var models = underwearSKUs.Select(s => new ReturnExemptionExportViewModel()
            {
                SKU = s,
                PrePaidLabel = "NO",
                ReasonCode = "OTHER",
                Comment = "Underwear/Undergarment can’t be returned by Florida law",
            }).ToList();

            var stream = ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(ReturnExemptionExportViewModel.TemplatePath),
                "ReturnAttributes",
                models,
                headerRowOffset: 2);

            //var filePath = UrlHelper.GetProductTemplateFilePath(filename);
            //using (var file = File.Open(filePath, FileMode.Create))
            //{
            //    stream.WriteTo(file);
            //}
            //return UrlHelper.GetProductTemplateUrl(filename);

            return stream;
        }
        
    }
}
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.DropShippers;
using Amazon.DTO;
using Amazon.DTO.DropShippers;
using Amazon.DTO.Inventory;
using CsvHelper;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Model.Implementation
{
    public class CustomFeedService : ICustomFeedService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public CustomFeedService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
        }

        public byte[] GenerateFeed(long customFeedId)
        {
            using (var db = _dbFactory.GetRDb())
            {
                var feed = db.CustomFeeds.GetAll().FirstOrDefault(f => f.Id == customFeedId);
                var feedFields = db.CustomFeedFields.GetAllAsDto()
                    .Where(f => f.CustomFeedId == customFeedId)
                    .OrderBy(f => f.SortOrder)
                    .ToList();

                using (var memoryStream = new MemoryStream())
                {
                    using (TextWriter writer = new StreamWriter(memoryStream))
                    {
                        using (var csv = new CsvWriter(writer))
                        {
                            csv.Configuration.Encoding = Encoding.UTF8;

                            var allStyles = (from st in db.Styles.GetAll()
                                             join stc in db.StyleCaches.GetAll() on st.Id equals stc.Id
                                             join si in db.StyleItems.GetAll() on st.Id equals si.StyleId
                                             join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id
                                             join dsi in db.DSItems.GetAll() on st.LinkedDSItemId equals dsi.Id
                                             where st.DropShipperId.HasValue
                                             select new
                                             {
                                                 Id = st.Id,
                                                 StyleID = st.StyleID,
                                                 Name = st.Name,
                                                 RemainingQuantity = sic.RemainingQuantity,
                                                 DSCost = dsi.Cost,
                                             })
                                        .ToList();

                            var allMagentoListings = (from st in db.Styles.GetAll()
                                                      join si in db.StyleItems.GetAll() on st.Id equals si.StyleId
                                                      join i in db.Items.GetAll() on si.Id equals i.StyleItemId
                                                      join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                                      where st.DropShipperId.HasValue
                                                       && !st.Deleted
                                                       && i.StyleItemId.HasValue
                                                      select new ItemDTO()
                                                      {
                                                          ParentASIN = i.ParentASIN,
                                                          StyleItemId = si.Id,
                                                          StyleId = st.Id,
                                                          SKU = l.SKU,
                                                          Barcode = i.Barcode,
                                                          CurrentPrice = l.CurrentPrice,
                                                          SourceMarketUrl = i.SourceMarketUrl,
                                                      }).ToList();

                            var allFeatures = db.Features.GetAllAsDto().ToList();

                            var allStyleValueFeatures = (from st in db.Styles.GetAll()
                                                         join sf in db.StyleFeatureValues.GetAll() on st.Id equals sf.StyleId
                                                         join fv in db.FeatureValues.GetAll() on sf.FeatureValueId equals fv.Id
                                                         where st.DropShipperId.HasValue
                                                            && !st.Deleted
                                                         select new StyleFeatureValueDTO()
                                                         {
                                                             StyleId = st.Id,
                                                             FeatureId = sf.FeatureId,
                                                             Value = fv.Value,
                                                         }).ToList();

                            var allStyleTextFeatures = (from st in db.Styles.GetAll()
                                                        join sf in db.StyleFeatureTextValues.GetAll() on st.Id equals sf.StyleId
                                                        where st.DropShipperId.HasValue
                                                          && !st.Deleted
                                                        select new StyleFeatureValueDTO()
                                                        {
                                                            StyleId = st.Id,
                                                            FeatureId = sf.FeatureId,
                                                            Value = sf.Value,
                                                        }).ToList();

                            var allStyleImages = (from st in db.Styles.GetAll()
                                                  join im in db.StyleImages.GetAll() on st.Id equals im.StyleId
                                                  where !im.IsSystem
                                                   && st.DropShipperId.HasValue
                                                   && !st.Deleted
                                                  orderby im.IsDefault descending, im.Id ascending
                                                  select new StyleImageDTO()
                                                  {
                                                      StyleId = im.StyleId,
                                                      Image = im.Image,
                                                      Type = im.Type
                                                  }).ToList();
                            var allBarcodes = (from st in db.Styles.GetAll()
                                               join si in db.StyleItems.GetAll() on st.Id equals si.StyleId
                                               join b in db.StyleItemBarcodes.GetAll() on si.Id equals b.StyleItemId
                                               where !st.Deleted
                                                && st.DropShipperId.HasValue
                                               select new BarcodeDTO()
                                               {
                                                   StyleItemId = si.Id,
                                                   Barcode = b.Barcode
                                               }).ToList();

                            //Header
                            foreach (var feedField in feedFields)
                            {
                                csv.WriteField(StringHelper.GetFirstNotEmpty(feedField.CustomFieldName, feedField.SourceFieldName));
                            }
                            csv.NextRecord();

                            //Values
                            foreach (var magentoListing in allMagentoListings)
                            {
                                var style = allStyles.FirstOrDefault(st => st.Id == magentoListing.StyleId);
                                var styleValueFeatures = allStyleValueFeatures.Where(svf => svf.StyleId == style.Id).ToList();
                                var styleTextFeatures = allStyleTextFeatures.Where(svf => svf.StyleId == style.Id).ToList();
                                var styleImages = allStyleImages.Where(sm => sm.StyleId == style.Id).ToList();

                                foreach (var feedField in feedFields)
                                {
                                    var value = "";

                                    switch (feedField.SourceFieldName)
                                    {
                                        case "Custom Field":
                                            value = feedField.CustomFieldValue;
                                            break;
                                        case "SalePrice":
                                            value = PriceHelper.RoundToTwoPrecision(magentoListing?.CurrentPrice).ToString();
                                            break;
                                        case "Cost":
                                            value = PriceHelper.RoundToTwoPrecision(style.DSCost).ToString();
                                            break;
                                        case "ParentSKU":
                                            value = magentoListing?.ParentASIN;
                                            break;
                                        case "SKU":
                                            value = magentoListing?.SKU;
                                            break;
                                        case "Title":
                                            value = style.Name;
                                            break;
                                        case "UPC":
                                            value = magentoListing.Barcode;
                                            break;
                                        case "Site Product Link":
                                            value = magentoListing.SourceMarketUrl;
                                            break;
                                        case "MainImage":
                                            value = styleImages.FirstOrDefault()?.Image;
                                            break;
                                        case "Image1":
                                            value = styleImages.Skip(1).FirstOrDefault()?.Image;
                                            break;
                                        case "Image2":
                                            value = styleImages.Skip(2).FirstOrDefault()?.Image;
                                            break;
                                        case "Image3":
                                            value = styleImages.Skip(3).FirstOrDefault()?.Image;
                                            break;

                                    }

                                    if (String.IsNullOrEmpty(value))
                                    {
                                        var feature = allFeatures.FirstOrDefault(f => f.Name == feedField.SourceFieldName);
                                        if (feature != null)
                                        {
                                            var styleValueFeature = allStyleValueFeatures.FirstOrDefault(fv => fv.FeatureId == feature.Id)?.Value;
                                            var styleTextFeature = allStyleTextFeatures.FirstOrDefault(fv => fv.FeatureId == feature.Id)?.Value;
                                            value = StringHelper.GetFirstNotEmpty(styleValueFeature, styleTextFeature);
                                        }
                                    }

                                    csv.WriteField(value);
                                }

                                csv.NextRecord();
                            }

                            writer.Flush();
                            return memoryStream.ToArray();
                        }
                    }
                }
            }
        }

        public CallMessagesResultVoid UploadToFtp(byte[] bytes, string filename, CustomFeedDTO ftpInfo)
        {
            try
            {
                FtpClient client = new FtpClient(ftpInfo.FtpSite);
                if (!String.IsNullOrEmpty(ftpInfo.UserName))
                    client.Credentials = new NetworkCredential(ftpInfo.UserName, ftpInfo.Password);
                client.Connect();

                if (!String.IsNullOrEmpty(ftpInfo.FtpFolder))
                    client.SetWorkingDirectory(ftpInfo.FtpFolder);
                client.Upload(bytes, filename);

                return new CallMessagesResultVoid()
                {
                    Status = CallStatus.Success,
                };
            }
            catch (Exception ex)
            {
                return CallMessagesResultVoid.Fail(ex.Message, null);
            }
        }

        public IList<string> GetSourceFieldsListForIncomingFeed(DSFileTypes feedType, DSProductType productType)
        {
            var results = new List<string>();

            if (feedType == DSFileTypes.ItemsFull)
            {
                var featureList = new List<string>();
                var itemTypeId = DSHelper.GetItemTypeIdFromDSProductType((int)productType);
                using (var db = _dbFactory.GetRDb())
                {
                    featureList = db.Features.GetAllAsDto()
                        .Where(f => f.ItemTypeId == itemTypeId
                            || !f.ItemTypeId.HasValue)
                        .OrderBy(f => f.Order)
                        .Select(f => f.Name)
                        .ToList();
                }

                results.Add("SKU");
                //results.Add("Model");
                results.Add("Qty");
                results.Add("Cost");
                results.Add("Sale Price");
                results.Add("MSRP");
                results.Add("Barcode");
                results.Add("Name");
                results.Add("Description");
                results.Add("Main Image");
                results.Add("Image 1");
                results.Add("Image 2");
                results.Add("Image 3");

                //featureList = featureList.Where(f => f != "Product Type").ToList();

                results.AddRange(featureList);
            }
            if (feedType == DSFileTypes.ItemsLite)
            {
                results.Add("SKU");
                results.Add("Sale Price");
                results.Add("Qty");
                results.Add("Cost");
            }

            return results;
        }

        public IList<string> GetSourceFieldsListForCustomOutgoingFeed()
        {
            var results = new List<string>();
            var featureList = new List<string>();
            using (var db = _dbFactory.GetRDb())
            {
                featureList = db.Features.GetAllAsDto().Select(f => f.Name).ToList();
            }

            results.Add("Title");
            results.Add("SKU");
            results.Add("ParentSKU");
            results.Add("Sale Price");
            results.Add("Cost");
            results.Add("UPC");
            results.Add("Site Product Link");
            results.Add("MainImage");
            results.Add("Image1");
            results.Add("Image2");
            results.Add("Image3");

            results.AddRange(featureList);

            return results;
        }
    }
}

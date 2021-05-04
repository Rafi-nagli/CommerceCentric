using Amazon.Api.Exports;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.Core.Views;
using Amazon.DTO.Inventory;
using Amazon.DTO.ScanOrders;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports.Types;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.ExcelToAmazon;
using Amazon.Web.ViewModels.Html;
using Ccen.Core.Models.Enums;
using Ccen.DTO.Inventory;
using Ccen.Web.General.ViewModels.Exports.Types;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Inventory.FBAPickLists
{
    public class ShipmentPickListViewModel
    {
        public long Id { get; set; }

        public bool Archived { get; set; }
        public ShipmentStatuses Status { get; set; }

        public DateTime CreateDate { get; set; }

        public ShipmentsTypeEnum ShipmentType { get; set; }

        public string Name
        {
            get { return ShipmentType.ToString() + "-" + CreateDate.ToString("MM-dd-yyyy"); }
        }

        public string FormattedStatus
        {
            get { return Status.ToString(); }
        }

        public bool IsFinished
        {
            get { return Status == ShipmentStatuses.Finished; }
        }

        public IList<ShipmentPickListEntryViewModel> Entries { get; set; }

        public ShipmentPickListViewModel()
        {

        }

        public ShipmentPickListViewModel(FBAPickListDTO pickList, ShipmentsTypeEnum shipmentsType)
        {
            Id = pickList.Id;
            Archived = pickList.Archived;
            Status = pickList.Status;
            CreateDate = pickList.CreateDate;
            ShipmentType = shipmentsType;
        }

        public static ShipmentPickListViewModel Get(IUnitOfWork db, long? id, string shipmenttype)
        {
            var model = new ShipmentPickListViewModel();
            model.Id = id ?? 0;
            if (id.HasValue)
            {
                var pickList = db.FBAPickLists.Get(id.Value);
                var pickListEntries =
                    db.FBAPickListEntries.GetAllAsDto().Where(fe => fe.FBAPickListId == id.Value).ToList();
                model.CreateDate = pickList.CreateDate;
                model.Status = pickList.Status;
                model.Entries = pickListEntries.Select(e => new ShipmentPickListEntryViewModel(e)).ToList();
            }
            else
            {
                model.Entries = new List<ShipmentPickListEntryViewModel>();                
            }
            model.ShipmentType = GetShipmentsTypeEnum(shipmenttype);
            return model;
        }

        public static IList<ShipmentPickListViewModel> GetAll(IUnitOfWork db, ShipmentPickListFilterViewModel filters)
        {
            var shipmentsType = GetShipmentsTypeEnum(filters.type);
            var query = db.FBAPickLists.GetAllAsDto(shipmentsType);
            if (!filters.ShowArchived)
                query = query.Where(f => !f.Archived);

            return query
                .OrderByDescending(f => f.CreateDate)
                .ToList()
                .Select(f => new ShipmentPickListViewModel(f, shipmentsType))
                .ToList();
        }

        private static ShipmentsTypeEnum GetShipmentsTypeEnum(string type)
        {
            switch (type)
            {
                case "FBA": return ShipmentsTypeEnum.FBA;
                case "WFS": return ShipmentsTypeEnum.WFS;
                default:
                    return ShipmentsTypeEnum.FBA;
            }
        }

        public static void SetFinishStatus(IDbFactory dbFactory, 
            ISystemActionService actionService,
            long pickListId, 
            bool isFinished, 
            DateTime when, 
            long? by)
        {
            if (isFinished)
            {
                List<ScanItemDTO> pickListEntries;
                FBAPickList dbPickList;
                using (var db = dbFactory.GetRWDb())
                {
                    pickListEntries = (from pi in db.FBAPickListEntries.GetAll()
                                       join l in db.Listings.GetAll() on pi.ListingId equals l.Id
                                       join i in db.Items.GetAll() on l.ItemId equals i.Id
                                       where pi.FBAPickListId == pickListId
                                       select new ScanItemDTO()
                                       {
                                           Quantity = pi.Quantity,
                                           Barcode = i.Barcode,
                                           StyleId = i.StyleId,
                                       }).ToList();

                    dbPickList = db.FBAPickLists.GetAll().FirstOrDefault(pi => pi.Id == pickListId);
                    dbPickList.Status = ShipmentStatuses.Finished;
                    db.Commit();
                }
                
                using (var invDb = dbFactory.GetInventoryRWDb())
                {
                    var scanOrder = new ScanOrderDTO()
                    {
                        Description = dbPickList.FBAPickListType + "-" + DateHelper.ToDateTimeString(dbPickList.CreateDate),
                        FileName = dbPickList.Id.ToString(),
                        OrderDate = when,
                        IsFBA = true
                    };

                    invDb.ItemOrderMappings.AddNewOrder(scanOrder, pickListEntries);
                }

                var styleIds = pickListEntries.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToArray();
                using (var db = dbFactory.GetRWDb())
                {
                    SystemActionHelper.RequestQuantityDistribution(db, actionService, styleIds, by);
                }
            }
            else
            {
                FBAPickList dbPickList;
                using (var db = dbFactory.GetRWDb())
                {
                    dbPickList = db.FBAPickLists.GetAll().FirstOrDefault(pi => pi.Id == pickListId);
                    dbPickList.Status = ShipmentStatuses.Default;
                    db.Commit();
                }

                var name = dbPickList.FBAPickListType + "-" + DateHelper.ToDateTimeString(dbPickList.CreateDate);
                using (var invDb = dbFactory.GetInventoryRWDb())
                {
                    var dbInvOrder = invDb.Orders.GetAll().FirstOrDefault(o => o.Description == name);
                    if (dbInvOrder != null)
                    {
                        invDb.Orders.Remove(dbInvOrder);
                        invDb.Commit();
                    }
                }
            }
        }

        public void Apply(IUnitOfWork db,
            DateTime when,
            long? by)
        {
            var pickListId = this.Id;
            if (this.Id == 0) //New
            {
                var newPickList = new FBAPickList()
                {
                    CreateDate = when,
                    CreatedBy = by,
                    FBAPickListType = ShipmentType.ToString()
                };
                db.FBAPickLists.Add(newPickList);
                db.Commit();
                pickListId = newPickList.Id;
            }

            var allPickListEntries = db.FBAPickListEntries.GetAll().Where(p => p.FBAPickListId == pickListId).ToList();
            var updatedEntries = new List<long>();
            foreach (var entry in Entries)
            {
                var listing = db.Listings.GetViewListings().FirstOrDefault(l => l.Id == entry.ListingId);

                FBAPickListEntry dbEntry = null;
                if (entry.Id == 0)
                {
                    dbEntry = new FBAPickListEntry()
                    {
                        FBAPickListId = pickListId,

                        CreateDate = when,
                        CreatedBy = by
                    };
                    db.FBAPickListEntries.Add(dbEntry);
                }
                else
                {
                    dbEntry = allPickListEntries.FirstOrDefault(e => e.Id == entry.Id);
                }
                dbEntry.StyleString = entry.StyleString;
                dbEntry.StyleItemId = entry.StyleItemId;
                dbEntry.Quantity = entry.Quantity;
                if (listing != null)
                {
                    dbEntry.ListingId = listing.Id;
                    dbEntry.ListingParentASIN = listing.ParentASIN;
                    dbEntry.ListingASIN = listing.ASIN;
                    dbEntry.ListingSKU = listing.SKU;
                }

                updatedEntries.Add(dbEntry.Id);
            }

            db.Commit();

            var toDeleteList = allPickListEntries.Where(e => !updatedEntries.Contains(e.Id)).ToList();
            foreach (var toDelete in toDeleteList)
            {
                db.FBAPickListEntries.Remove(toDelete);
            }
            db.Commit();
        }

        public void GetForPickList(IUnitOfWork db)
        {

        }

        public static bool SetArchiveStatus(IUnitOfWork db, long id, bool newStatus)
        {
            var fbaPicklist = db.FBAPickLists.Get(id);
            fbaPicklist.Archived = newStatus;
            db.Commit();

            return fbaPicklist.Archived;
        }

        public static MemoryStream ExportToExcel(long pickListId,
            IUnitOfWork db,
            IDbFactory dbFactory,
            CompanyDTO company,
            IMarketCategoryService categoryService,
            IHtmlScraperService htmlScraper,
            ILogService log,
            ITime time,
            out string filename)
        {
            var resultItems = new List<ExcelProductUSViewModel>();

            var pickListEntries = db.FBAPickListEntries.GetAllAsDto()
                .Where(p => p.FBAPickListId == pickListId)
                .ToList();

            IList<FBAItemInfo> fbaItems = pickListEntries.Select(e => new FBAItemInfo()
            {
                ParentASIN = e.ListingParentASIN,
                ASIN = e.ListingASIN,
                SKU = e.ListingSKU,
                Quantity = e.Quantity,
            }).ToList();

            var marketplaceManager = new MarketplaceKeeper(dbFactory, false);
            marketplaceManager.Init();

            IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), time, log, dbFactory, null)
                .GetApi(AccessManager.Company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);

            var parentASINList = fbaItems.Select(i => i.ParentASIN).Distinct().ToList();

            foreach (var parentASIN in parentASINList)
            {
                var childFBAItems = fbaItems.Where(f => f.ParentASIN == parentASIN).ToList();
                var newItems = ExcelProductUSViewModel.GetItemsFor(log,
                    time,
                    categoryService,
                    htmlScraper,
                    api,
                    db,
                    company,
                    parentASIN,
                    ExportToExcelMode.FBA,
                    childFBAItems,
                    MarketType.Amazon,
                    MarketplaceKeeper.AmazonComMarketplaceId,
                    UseStyleImageModes.Auto,
                    out filename);

                var parentItem = newItems.FirstOrDefault(i => i.Parentage == ExcelHelper.ParentageParent);
                var wasAddedAny = false;
                var notExistNewItems = new List<ExcelProductUSViewModel>();
                foreach (var item in newItems.Where(i => i.Parentage != ExcelHelper.ParentageParent))
                {
                    var existListing = db.Listings.GetAll().FirstOrDefault(l => l.SKU == item.SKU && !l.IsRemoved);
                    if (existListing == null)
                    {
                        notExistNewItems.Add(item);
                        wasAddedAny = true;
                    }
                }
                if (wasAddedAny)
                {
                    if (parentItem != null)
                        resultItems.Add(parentItem);
                    resultItems.AddRange(notExistNewItems);
                }
            }

            var output = ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(ExcelProductUSViewModel.USTemplatePath),
                "Template",
                resultItems);

            filename = String.Format("FBAListings_{0}.xls", time.GetAppNowTime().ToString("MM_dd_yyyy_hh_mm_ss"));

            return output;
        }

        public static MemoryStream ExportToWFSExcel(long pickListId,
            IUnitOfWork db,
            IDbFactory dbFactory,
            CompanyDTO company,
            IMarketCategoryService categoryService,
            IHtmlScraperService htmlScraper,
            ILogService log,
            ITime time,
            out string filename)
        {
            var resultItems = new List<ExcelWFSProductUSViewModel>();

            var pickListEntries = db.FBAPickListEntries.GetAllAsDto()
                .Where(p => p.FBAPickListId == pickListId)
                .ToList();

            IList<FBAItemInfo> fbaItems = pickListEntries.Select(e => new FBAItemInfo()
            {
                ParentASIN = e.ListingParentASIN,
                ASIN = e.ListingASIN,
                SKU = e.ListingSKU,
                Quantity = e.Quantity,
            }).ToList();

            var marketplaceManager = new MarketplaceKeeper(dbFactory, false);
            marketplaceManager.Init();

            IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), time, log, dbFactory, null)
                .GetApi(AccessManager.Company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);

            var parentASINList = fbaItems.Select(i => i.ParentASIN).Distinct().ToList();

            foreach (var parentASIN in parentASINList)
            {
                var childFBAItems = fbaItems.Where(f => f.ParentASIN == parentASIN).ToList();
                var newItems = ExcelWFSProductUSViewModel.GetItemsFor(log,
                    time,
                    categoryService,
                    htmlScraper,
                    api,
                    db,
                    company,
                    parentASIN,
                    ExportToExcelMode.FBA,
                    childFBAItems,
                    MarketType.Amazon,
                    MarketplaceKeeper.AmazonComMarketplaceId,
                    UseStyleImageModes.Auto,
                    out filename);

                var parentItem = newItems.FirstOrDefault(i => i.Parentage == ExcelHelper.ParentageParent);
                var wasAddedAny = false;
                var notExistNewItems = new List<ExcelWFSProductUSViewModel>();
                foreach (var item in newItems.Where(i => i.Parentage != ExcelHelper.ParentageParent))
                {
                    var existListing = db.Listings.GetAll().FirstOrDefault(l => l.SKU == item.SKU && !l.IsRemoved);
                    if (existListing == null)
                    {
                        notExistNewItems.Add(item);
                        wasAddedAny = true;
                    }
                }
                if (wasAddedAny)
                {
                    if (parentItem != null)
                        resultItems.Add(parentItem);
                    resultItems.AddRange(notExistNewItems);
                }
            }

            var output = ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(ExcelWFSProductUSViewModel.USTemplatePath),
                "Template",
                resultItems);

            filename = String.Format("WFSListings_{0}.xls", time.GetAppNowTime().ToString("MM_dd_yyyy_hh_mm_ss"));

            return output;
        }

        public static MemoryStream ExportToPlanExcel(long pickListId,
            IUnitOfWork db,
            IDbFactory dbFactory,
            CompanyDTO company,
            ILogService log,
            ITime time,
            out string filename)
        {
            //var resultItems = new List<ExcelProductUSViewModel>();
            var pickList = db.FBAPickLists.GetAllAsDto(ShipmentsTypeEnum.FBA).FirstOrDefault(p => p.Id == pickListId);

            var pickListEntries = db.FBAPickListEntries.GetAllAsDto()
                .Where(p => p.FBAPickListId == pickListId)
                .ToList();

            var fbaPlanItems = pickListEntries.Select(e => new ExcelFBAPlanViewModel()
            {
                SKU = ItemExportHelper.PrepareSKU(e.ListingSKU, ExportToExcelMode.FBA),
                Quantity = e.Quantity,
                UnitsPerCase = e.Quantity,
                NumberOfCases = 1
            }).ToList();

            var output = ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(ExcelFBAPlanViewModel.TemplatePath),
                "Case Quantity Template",
                fbaPlanItems,
                customData: new List<ExcelHelper.CustomField>()
                {
                    new ExcelHelper.CustomField()
                    {
                        Row = 0,
                        Cell = 1,
                        Value = "FBA-" + pickList.CreateDate.ToString("MM-dd-yyyy")
                    }
                },
                headerRowOffset: 12);

            filename = String.Format("FBAPlan_{0}.xls", time.GetAppNowTime().ToString("MM_dd_yyyy_hh_mm_ss"));

            return output;
        }

        public static MemoryStream ExportToWFSPlanExcel(long pickListId,
            IUnitOfWork db,
            IDbFactory dbFactory,
            CompanyDTO company,
            ILogService log,
            ITime time,
            out string filename)
        {
            //var resultItems = new List<ExcelProductUSViewModel>();
            var pickList = db.FBAPickLists.GetAllAsDto(ShipmentsTypeEnum.WFS).FirstOrDefault(p => p.Id == pickListId);

            var pickListEntries = db.FBAPickListEntries.GetAllAsDto()
                .Where(p => p.FBAPickListId == pickListId)
                .ToList();

            var fbaPlanItems = pickListEntries.Select(e => new ExcelWFSPlanViewModel()
            {
                SKU = ItemExportHelper.PrepareSKU(e.ListingSKU, ExportToExcelMode.FBA),
                Quantity = e.Quantity,
                UnitsPerCase = e.Quantity,
                NumberOfCases = 1
            }).ToList();

            var output = ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(ExcelWFSPlanViewModel.TemplatePath),
                "sampleInboundShipmentTemplate 1",
                fbaPlanItems,
                customData: new List<ExcelHelper.CustomField>()
                {
                    new ExcelHelper.CustomField()
                    {
                        Row = 0,
                        Cell = 1,
                        Value = "WFS-" + pickList.CreateDate.ToString("MM-dd-yyyy")
                    }
                },
                headerRowOffset: 12);

            filename = String.Format("WFSPlan_{0}.xls", time.GetAppNowTime().ToString("MM_dd_yyyy_hh_mm_ss"));

            return output;
        }

        public static MemoryStream ExportPickList(long pickListId,
                IUnitOfWork db,
                ITime time,
                out string filename)
        {
            //var resultItems = new List<ExcelProductUSViewModel>();
            var pickList = db.FBAPickLists.GetAllAsDto(ShipmentsTypeEnum.None).FirstOrDefault(p => p.Id == pickListId);

            var pickListEntries = (from e in db.FBAPickListEntries.GetAll()
                                  join l in db.Listings.GetAll() on e.ListingId equals l.Id
                                  join i in db.Items.GetAll() on l.ItemId equals i.Id
                                  join st in db.Styles.GetAll() on i.StyleId equals st.Id
                                  where e.FBAPickListId == pickListId
                                  select new
                                  {
                                      Id = e.Id,
                                      StyleId = i.StyleId,
                                      SKU = l.SKU,
                                      Quantity = e.Quantity,
                                      Barcode = i.Barcode,
                                      Title = st.Name,
                                  }).ToList();

            var styleIds = pickListEntries.Select(e => e.StyleId).ToList();
            var locations = db.StyleLocations.GetAll().Where(l => styleIds.Contains(l.StyleId))
                .OrderByDescending(l => l.IsDefault)
                .ThenBy(l => l.Id)
                .ToList();

            IList<ExcelWFSLocationViewModel> locationItems = new List<ExcelWFSLocationViewModel>();
            foreach (var i in pickListEntries)
            {
                var location = locations.FirstOrDefault(l => l.StyleId == i.StyleId);
                locationItems.Add(new ExcelWFSLocationViewModel()
                {
                    PickListEntryId = i.Id,
                    SKU = ItemExportHelper.PrepareSKU(i.SKU, ExportToExcelMode.FBA),
                    Quantity = i.Quantity,
                    Barcode = i.Barcode,
                    Title = i.Title,
                    Location = location != null ? location.Isle + "/" + location.Section + "/" + location.Shelf : "",
                    LocationIndex = location != null ? location.SortIsle * 100000 + location.SortSection * 1000 + location.SortShelf : Int32.MaxValue,
                });
            }

            locationItems = locationItems
                .OrderBy(l => l.LocationIndex)
                .ThenBy(l => l.PickListEntryId)
                .ToList();

            var output = ExcelHelper.Export(locationItems,
                null,
                isXlsx: true);

            filename = String.Format(pickList.FBAPickListType + "_PickList_{0}.xlsx", pickList.CreateDate.ToString("MM-dd-yyyy"));

            return output;
        }

        public static IList<SelectListItemTag> GetListingByStyleSize(
            IUnitOfWork db,
            long styleItemId,
            long? selectedListingId,
            ShipmentsTypeEnum shipmenttype)
        {
            if (shipmenttype == ShipmentsTypeEnum.FBA)
            {

                var listings = db.Listings.GetViewListings().Where(l => l.StyleItemId == styleItemId
                        && !l.IsRemoved
                        && l.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                    .ToList()
                    .OrderByDescending(l => l.IsFBA)
                    .ThenBy(l => l.Rank ?? RankHelper.DefaultRank)
                    .ToList();

                var resultListings = new List<ViewListing>();
                foreach (var listing in listings)
                {
                    if (!resultListings.Any(l => l.ASIN == listing.ASIN && l.IsFBA) //NOTE: if list already contain FBA version of listing
                        || listing.Id == selectedListingId)
                        resultListings.Add(listing);
                }

                return resultListings.Select(l => new SelectListItemTag()
                {
                    Text = l.ASIN + " - " + (l.Rank.HasValue ? "#" + l.Rank.Value.ToString("###,###,###,###") : "n/a") + (l.IsFBA ? " (FBA)" : ""),
                    Value = l.Id.ToString(),
                    Selected = l.IsFBA,
                    Tag = l.IsFBA.ToString()
                }).ToList();
            }
            else
            {
                var listings = db.Listings.GetViewListings().Where(l => l.StyleItemId == styleItemId
                    && !l.IsRemoved
                    && l.Market == (int)MarketType.Walmart)
                .ToList()
                .OrderByDescending(l => l.Id)
                .ToList();

                return listings.Select(l => new SelectListItemTag()
                {
                    Text = l.SKU,
                    Value = l.Id.ToString()
                }).ToList();
            }
        }
    }
}
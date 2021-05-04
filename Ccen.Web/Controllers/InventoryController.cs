using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Model.Implementation;
using Amazon.Web.Filters;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Grid;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.Results;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Amazon.Web.ViewModels;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class InventoryController : BaseController
    {
        public override string TAG
        {
            get { return "InventoryController."; }
        }

        private const string PopupContentView = "StylePopupContent";

        public virtual ActionResult Styles(string styleId)
        {
            LogI("StylesFast, styleId=" + styleId);

            var model = new StylePageViewModel();
            model.SelectedStyleId = styleId;
            model.Init(Db);

            return View("Styles", model);
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult UpdateStyle(long id, int? itemType)
        {
            LogI("UpdateStyle, Id=" + id);
            SessionHelper.ClearUploadedImages();

            var item = Db.Styles.GetByStyleIdAsDto(id);

            StyleViewModel model;
            if (item == null)
            {
                model = new StyleViewModel(Db, itemType ?? StyleViewModel.DefaultItemType);
                ViewBag.IsAdd = true;
            }
            else
            {
                if (itemType.HasValue)
                    item.ItemTypeId = itemType;
                model = new StyleViewModel(Db, MarketplaceService, item);
                ViewBag.IsAdd = false;
            }

            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult SetOnHold(int id, bool onHold)
        {
            LogI("SetOnHold, id=" + id + ", onHold=" + onHold);
            var when = Time.GetAppNowTime();
            StyleViewModel.SetOnHold(Db, StyleHistoryService, ActionService, id, onHold, when, AccessManager.UserId);

            return new JsonResult
            {
                Data = new
                {
                    OnHold = onHold,
                    OnHoldUpdateDate = when
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public virtual ActionResult StyleHistory(string styleString)
        {
            LogI("StyleHistory, styleString=" + styleString);

            var model = new StyleHistoryControlViewModel()
            {
                StyleString = styleString,
            };

            return View("StyleHistory", model);
        }

        public virtual ActionResult GetStyleHistory(string styleString)
        {
            LogI("GetStyleHistory, styleString=" + styleString);

            var model = StyleHistoryViewModel.GetByStyleString(Db, LogService, styleString);

            return JsonGet(new ValueResult<StyleHistoryViewModel>()
            {
                IsSuccess = model != null,
                Data = model
            });
        }

        public virtual ActionResult GetStylePopoverInfo(string styleId, long? listingId)
        {
            LogI("GetStylePopoverInfo, styleId=" + styleId + ", listingId=" + listingId);

            var model = StylePopoverInfoViewModel.GetForStyle(Db, styleId, listingId);

            return JsonGet(ValueResult<StylePopoverInfoViewModel>.Success("", model));
        }

        [Authorize(Roles = AccessManager.AllWriteRole)]
        public virtual ActionResult DeleteStyle(long id)
        {
            LogI("DeleteStyle, Id=" + id);

            var record = Db.Styles.Get(id);
            record.Deleted = true;

            Db.Items.DeleteAnyLinksToStyleId(id);
            Db.Commit();
            return Json(ValueResult<long>.Success("", id), JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult CreateStyle(int itemType)
        {
            LogI("CreateStyle, itemType=" + itemType);
            SessionHelper.ClearUploadedImages();

            var model = new StyleViewModel(Db, itemType);

            model.IsHidden = AccessManager.UserName == UserHelper.SvetaTName;

            ViewBag.PartialViewName = PopupContentView;
            ViewBag.IsAdd = true;
            return View("EditEmpty", model);
        }
        
        public virtual ActionResult GetAllUpdates(DateTime? fromDate)
        {
            LogI("GetAllUpdates, fromDate=" + fromDate);

            IList<StyleViewModel> items = new List<StyleViewModel>();
            DateTime? when = null;
            if (fromDate.HasValue)
            {
                var searchFilter = new StyleSearchFilterViewModel()
                {
                    FromReSaveDate = fromDate
                };
                items = StyleViewModel.GetAll(Db, searchFilter).Items.ToList();
                when = Time.GetAppNowTime();
            }

            var data = new GridResponse<StyleViewModel>(items, items.Count, when);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetIdListByFilters(string barcode,
            int? gender,
            [Bind(Prefix = "itemStyles[]")]List<int> itemStyles,
            [Bind(Prefix = "sleeves[]")]List<int> sleeves,
            int? mainLicense,
            int? subLicense,
            int? holiday,
            bool hasInitialQty)
        {
            LogI("GetFilteredIdList, barcode=" + barcode
                + ", gender=" + gender
                + ", itemStyles=" + itemStyles
                + ", sleeves=" + sleeves
                + ", mainLicense=" + mainLicense
                + ", subLicense=" + subLicense
                + ", hasInitialQty=" + hasInitialQty);
            var searchFilter = new StyleSearchFilterViewModel()
            {
                Barcode = StringHelper.TrimWhitespace(barcode),
                Gender = gender,
                ItemStyles = itemStyles,
                Sleeves = sleeves,
                HolidayId = holiday,
                MainLicense = mainLicense,
                SubLicense = subLicense,
                HasInitialQty = hasInitialQty
            };
            var idList = StyleViewModel.GetIdListByFilters(Db, searchFilter);
            return Json(ValueResult<IEnumerable<long>>.Success("", idList), JsonRequestBehavior.AllowGet);
        }

        [Compress]
        public virtual ActionResult GetAll(GridRequest request,
            string styleString,
            string keywords,
            long? dropShipperId,
            bool onlyInStock,
            string barcode,
            string gender,
            [Bind(Prefix = "itemStyles[]")]List<int> itemStyles,
            [Bind(Prefix = "sleeves[]")]List<int> sleeves,
            int? holidayId,
            int? mainLicense,
            int? subLicense,
            bool hasInitialQty,
            int? pictureStatus,
            int? fillingStatus,
            int? noneSoldPeriod,
            string onlineStatus,
            bool includeKiosk,
            bool onlyOnHold,
            int? minQty,
            string excludeMarketplaceId,
            string includeMarketplaceId)
        {
            LogI("GetAll, barcode=" + barcode
                            + ", keywords=" + keywords
                            + ", styleString=" + styleString
                            + ", dropShipperId=" + dropShipperId
                            + ", gender=" + gender
                            + ", itemStyles=" + itemStyles
                            + ", sleeves=" + sleeves
                            + ", holidayId=" + holidayId
                            + ", pictureStatus=" + pictureStatus
                            + ", fillingStatus=" + fillingStatus
                            + ", noneSoldPeriod=" + noneSoldPeriod
                            + ", onlineStatus=" + onlineStatus
                            + ", mainLicense=" + mainLicense
                            + ", subLicense=" + subLicense
                            + ", onlyInStock=" + onlyInStock
                            + ", minQty=" + minQty
                            + ", includeKisok=" + includeKiosk
                            + ", onlyOnHold=" + onlyOnHold
                            + ", excludeMarketplaceId=" + excludeMarketplaceId
                            + ", includeMarketplaceId=" + includeMarketplaceId);

            var genderIds = (gender ?? "").Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => Int32.Parse(g))
                    .ToList();

            var pageSize = request.ItemsPerPage;

            var searchFilter = new StyleSearchFilterViewModel()
            {
                StyleString = StringHelper.TrimWhitespace(styleString),
                Barcode = StringHelper.TrimWhitespace(barcode),
                Keywords = StringHelper.TrimWhitespace(keywords),
                Genders = genderIds,
                DropShipperId = dropShipperId,
                MainLicense = mainLicense,
                SubLicense = subLicense,
                ItemStyles = itemStyles,
                Sleeves = sleeves,
                HolidayId = holidayId,
                OnlineStatus = onlineStatus,
                PictureStatus = pictureStatus,
                FillingStatus = fillingStatus,
                NoneSoldPeriod = noneSoldPeriod,
                MinQty = minQty,
                ExcludeMarketplaceId = excludeMarketplaceId,
                IncludeMarketplaceId = includeMarketplaceId,
                OnlyInStock = onlyInStock,
                IncludeKiosk = includeKiosk,
                OnlyOnHold = onlyOnHold,
                //BrandName = StringHelper.TrimWhitespace(brand),
                StartIndex = (request.Page - 1) * pageSize,
                LimitCount = pageSize,
                SortField = request.SortField,
                SortMode = request.SortMode == "asc" ? 0 : 1,
            };

            var gridResult = StyleViewModel.GetAll(Db, searchFilter);
            gridResult.RequestTimeStamp = request.TimeStamp;
            //var data = new GridResponse<StyleViewModel>(items, items.Count, Time.GetAppNowTime());

            return Json(gridResult, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        [HttpPost]
        public virtual ActionResult Submit(StyleViewModel model,
            int? generateForMarket)
        {
            var openUrl = String.Empty;

            LogI("Submit, model=" + model + ", generateForMarket=" + generateForMarket);

            //Save
            if (ModelState.IsValid)
            {
                var messages = new List<MessageString>();
                var errors = StyleViewModel.Validate(Db,
                    model.StyleId,
                    model.Id,
                    model.StyleItems.Items);

                if (!errors.Any())
                {
                    //Save changes
                    model.SetUploadedImages(SessionHelper.GetUploadedImages());

                    //Generate Excel
                    //NOTE: before apply, setting auto-generated barcodes
                    if (generateForMarket == (int)MarketType.Amazon)
                        openUrl = model.GenerateUS(Db, BarcodeService, AmazonCategoryService, Time.GetAppNowTime(), out messages);
                    if (generateForMarket == (int)MarketType.AmazonEU)
                        openUrl = model.GenerateUK(Db, BarcodeService, AmazonCategoryService, Time.GetAppNowTime(), out messages);
                    if (generateForMarket == (int) MarketType.AmazonAU)
                        openUrl = "";// model.GenerateUK(Db, BarcodeService, Time.GetAppNowTime(), out messages);
                    if (generateForMarket == (int)MarketType.Walmart)
                        openUrl = model.GenerateWalmart(Db, BarcodeService, Time.GetAppNowTime(), out messages);

                    var applyItemQty = generateForMarket != null && generateForMarket != (int) MarketType.None; //NOTE: only when generate

                    model.Apply(Db, 
                        LogService,
                        Cache,
                        QuantityManager,
                        PriceManager,
                        BarcodeService,
                        ActionService,
                        StyleHistoryService,
                        AutoCreateListingService,
                        applyItemQty,
                        DateHelper.GetAppNowTime(),
                        AccessManager.UserId);

                    SessionHelper.ClearUploadedImages();
                }

                if (messages.Any() || errors.Any())
                {
                    errors.AddRange(messages);

                    errors.ForEach(e => ModelState.AddModelError(e.Key ?? "model", e.Message));
                    model.ReInitFeatures(Db);

                    return PartialView(PopupContentView, model);
                }
                else
                {
                    //Get Lite Version of updates for Grid
                    model = StyleViewModel.GetAll(Db, new StyleSearchFilterViewModel()
                    {
                        StyleId = model.Id
                    }).Items?.FirstOrDefault();

                    //TODO: Add "Status", now only updates StatusCode
                    return Json(new UpdateRowViewModel(model,
                        "Styles",
                        new[]
                        {
                            "HasImage",
                            "Image",
                            "Thumbnail",
                            "StyleId",
                            "DropShipperName",
                            "Name",
                            "StyleItemCaches",
                            "StyleItems",
                            "Locations",
                            "MainLocation",
                            "HasLocation",
                            "IsOnline",
                            "OnHold",
                            "CreateDate",
                            "IsHidden",
                            "ToPhotographer"
                        },
                        false,
                        openUrl));
                }
            } 
            ViewBag.IsAdd = false;
            return PartialView(PopupContentView, model);
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult ChangeStyleItem(long styleItemId, int inputSizeId)
        {
            LogI("ChangeStyleItem, styleItemId=" + styleItemId + ", inputSizeId=" + inputSizeId);

            var result = StyleViewModel.ChangeSize(Db,
                LogService,
                Cache,
                styleItemId, 
                inputSizeId,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return JsonGet(new ValueMessageResult<StyleItemViewModel>()
            {
                IsSuccess = result.Status == CallStatus.Success,
                Messages = result.Messages,
                Data = result.Data
            });
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult RemoveStyleItems(List<long> styleItemIdList)
        {
            if (styleItemIdList == null)
                styleItemIdList = new List<long>();

            LogI("RemoveStyleItems, idList=" + String.Join(",", styleItemIdList));

            var result = StyleViewModel.RemoveStyleItems(Db,
                LogService,
                Cache,
                styleItemIdList,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return JsonGet(new MessagesResult()
            {
                IsSuccess = result.Status == CallStatus.Success,
                Messages = result.Messages
            });
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult MergeStyleItems(long toStyleItemId,
            long fromStyleItemId)
        {
            LogI("MergeStyleItems, toStyleItemId=" + toStyleItemId + ", fromStyleItemId=" + fromStyleItemId);

            var quantityManager = new QuantityManager(LogService, Time);

            var result = StyleViewModel.MergeStyleItems(Db,
                LogService,
                Cache,
                quantityManager,
                toStyleItemId,
                fromStyleItemId,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return JsonGet(new MessagesResult()
            {
                IsSuccess = result.Status == CallStatus.Success,
                Messages = result.Messages
            });
        }


        public virtual ActionResult CheckStyleId(long? id, string styleId)
        {
            LogI("CheckStyleId, id=" + id + ", styleId=" + styleId);

            var isNewStyle = Db.Styles.IsNewStyle(id, styleId);

            return Json(isNewStyle, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult CheckStyleItem(long styleItemId)
        {
            LogI("CheckStyleItemId, id=" + styleItemId);

            var hasLinkedItems = StyleItemViewModel.HasLinkedEntities(Db, styleItemId);

            return Json(new ValueResult<bool>(true, "", !hasLinkedItems), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetSizesInfo(long[] sizeIdList)
        {
            LogI("GetSizesInfo: " + ToStringHelper.ToString(sizeIdList));

            var results = new List<StyleItemViewModel>();
            if (sizeIdList != null)
            {
                var sizes = Db.Sizes.GetAllWithGroupAsDto()
                    .Where(s => sizeIdList.Contains(s.Id))
                    .ToList();

                results = sizes.Select(s => new StyleItemViewModel()
                {
                    SizeId = s.Id,
                    Size = s.Name,
                    SizeGroupName = s.SizeGroupName,
                    SizeGroupId = s.SizeGroupId,
                }).ToList();
            }

            return Json(new ValueResult<IList<StyleItemViewModel>>(true, "", results), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetGeneratedFile(string fileName)
        {
            LogI("GetGeneratedFile, fileName=" + fileName);

            var path = Models.UrlHelper.GetProductTemplateFilePath(fileName);
            return File(path, "application/vnd.ms-excel", fileName);
        }
    }
}

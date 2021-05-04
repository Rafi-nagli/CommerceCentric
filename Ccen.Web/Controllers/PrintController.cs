using Amazon.Common.Helpers;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.General;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.Print;
using Ccen.Core.Models.Enums;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using UrlHelper = Amazon.Web.Models.UrlHelper;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class PrintController : BaseController
    {
        public override string TAG
        {
            get { return "PrintController."; }
        }

        public virtual ActionResult PrintFiles()
        {
            LogI("PrintFiles");

            return View();
        }

        public virtual ActionResult GetPrintFiles([DataSourceRequest] DataSourceRequest request)
        {
            LogI("GetPrintFiles");

            var items = LabelPrintPackViewModel.GetAll(Db.LabelPrintPacks).OrderByDescending(l => l.CreateDate);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }



        public virtual ActionResult PrintFBAPickList(long fbaShipmentId)
        {
            LogI("PrintFBAPickList, fbaShipmentId=" + fbaShipmentId);

            string batchName = null;
            var batch = Db.FBAPickLists.GetAllAsDto(ShipmentsTypeEnum.None).FirstOrDefault(f => f.Id == fbaShipmentId);
            batchName = batch.FBAPickListType + "-" + batch.CreateDate.ToString("dd-MM-yyyy");

            return View("PrintFBAPickList", new PickListPageViewModel
            {
                BatchId = fbaShipmentId,
                BatchName = batchName,
            });
        }

        public virtual ActionResult PrintPhotoshootPickList(long photoshootId)
        {
            LogI("PrintPhotoshootPickList, photoshootId=" + photoshootId);

            string batchName = null;
            var batch = Db.PhotoshootPickLists.GetAllAsDto().FirstOrDefault(f => f.Id == photoshootId);
            batchName = "Photoshoot-" + batch.CreateDate.ToString("dd-MM-yyyy");

            return View("PrintPhotoshootPickList", new PickListPageViewModel
            {
                BatchId = photoshootId,
                BatchName = batchName,
            });
        }

        public virtual ActionResult PrintPickList(long? batchId)
        {
            LogI("PrintPickList, batchId=" + batchId);

            string batchName = null;
            if (batchId.HasValue)
            {
                var batch = Db.OrderBatches.GetAsDto(batchId.Value);
                batchName = batch.Name;
            }

            return View("PrintPickList", new PickListPageViewModel
            {
                BatchId = batchId,
                BatchName = batchName,
            });
        }

        public virtual ActionResult PrintPendingPickList()
        {
            LogI("PrintPendingPickList");

            return View("PrintPickList", new PickListPageViewModel { UseOnlyAllPending = true });
        }

        public virtual ActionResult GetPickListWithLocation([DataSourceRequest] DataSourceRequest request,
            long? batchId,
            bool? useOnlyAllPending)
        {
            LogI("GetPickListWithLocation, batchId=" + batchId + ", useOnlyAllPending=" + useOnlyAllPending);

            try
            {
                bool excludeOnHold = false;
                string[] orderStatus = null;
                if (batchId != null)
                {
                    excludeOnHold = false;
                    orderStatus = OrderStatusEnumEx.AllUnshippedWithShipped;
                }
                else
                {
                    excludeOnHold = false;
                    if (useOnlyAllPending == true)
                        orderStatus = new[] { OrderStatusEnumEx.Pending };
                    else
                        orderStatus = OrderStatusEnumEx.AllUnshipped;
                }

                var model = new OrderSearchFilterViewModel
                {
                    ShippingStatus = "Select...",
                    BatchId = batchId,
                    DropShipperId = DSHelper.DefaultPAId,
                    //Pick list should never have pajamas for which labels were already generated (more accurate restriction in VM)
                    OrderStatus = orderStatus,
                    ExcludeOnHold = excludeOnHold,
                    DateFrom = null,
                    DateTo = null,
                    BuyerName = null,
                    OrderNumber = null,
                };
                //Pick list should never have pajamas for which labels were already generated (if not in batch)
                if (!batchId.HasValue)
                    model.ExcludeWithLabels = true;

                var items = PickListItemViewModel.GetPickListItems(Db, LogService, WeightService, model);
                var dataSource = items.ToDataSourceResult(request);
                return JsonGet(dataSource);
            }
            catch (Exception ex)
            {
                LogE("GetPickListWithLocation", ex);
                return new JsonResult { Data = new List<PickListItemViewModel>(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public virtual ActionResult GetFBAPickListWithLocation([DataSourceRequest] DataSourceRequest request,
            long batchId)
        {
            LogI("GetFBAPickListWithLocation, batchId=" + batchId);

            try
            {
                var items = FBAPickListItemViewModel.GetPickListItems(Db, LogService, batchId);
                var dataSource = items.ToDataSourceResult(request);
                return JsonGet(dataSource);
            }
            catch (Exception ex)
            {
                LogE("GetFBAPickListWithLocation", ex);
                return new JsonResult { Data = new List<PickListItemViewModel>(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public virtual ActionResult GetPhotoshootPickListWithLocation([DataSourceRequest] DataSourceRequest request,
            long batchId)
        {
            LogI("GetPhotoshootPickListWithLocation, batchId=" + batchId);

            try
            {
                var items = PhotoshootPickListItemViewModel.GetPickListItems(Db, LogService, batchId);
                var dataSource = items.ToDataSourceResult(request);
                return JsonGet(dataSource);
            }
            catch (Exception ex)
            {
                LogE("GetFBAPickListWithLocation", ex);
                return new JsonResult { Data = new List<PickListItemViewModel>(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }


        public virtual ActionResult GetPackingSlip(long orderId)
        {
            LogI("GetPackingSlip, orderId=" + orderId);

            try
            {
                var companyAddress = new CompanyAddressService(AccessManager.Company);
                var orders = PackingSlipViewModel.GetList(Db, new[] { orderId }, SortMode.None, true).ToList();
                var model = new PackingSlipCollectionModel
                {
                    ReturnAddress = companyAddress.GetReturnAddress(MarketIdentifier.Empty()),
                    PackingSlips = orders,
                };

                var marketplaces = new MarketplaceKeeper(DbFactory, false);
                marketplaces.Init();
                model.Marketplaces = marketplaces.GetAll().Select(m => new PackingSlipMarketplaceInfo(m)).ToList();


                return View("PackingSlip", model);
            }
            catch (Exception ex)
            {
                LogE("GetPackingSlip", ex);
                throw;
            }
        }

        //public virtual ActionResult GetPackingSlipWithCoupon(long orderId)
        //{
        //    LogI("GetSamplePackingSlip, orderId=" + orderId);

        //    try
        //    {
        //        var orders = PackingSlipViewModel.GetList(Db, new[] { orderId }, SortMode.None, true).ToList();
        //        var model = new PackingSlipCollectionModel
        //        {
        //            ReturnAddress = AccessManager.Company.GetReturnAddressDto(),
        //            PackingSlips = orders,
        //        };

        //        var marketplaces = new MarketplaceKeeper(DbFactory);
        //        marketplaces.Init();
        //        model.Marketplaces = marketplaces.GetAll().Select(m => new PackingSlipMarketplaceInfo(m)).ToList();


        //        return View("PackingSlipWithCoupon", model);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogE("GetPackingSlip", ex);
        //        throw;
        //    }
        //}

        public virtual ActionResult GetPackingSlipsForBatch(long batchId)
        {
            LogI("GetPackingSlipsForBatch, batchId=" + batchId);

            var orderIds = Db.OrderBatches.GetOrderIdsForBatch(batchId, OrderStatusEnumEx.AllUnshippedWithShipped);
            var orders = PackingSlipViewModel.GetList(Db, orderIds, SortMode.ByShippingMethodThenLocation, false).ToList();
            var companyAddress = new CompanyAddressService(AccessManager.Company);
            var batch = Db.OrderBatches.Get(batchId);

            var model = new PackingSlipCollectionModel
            {
                BatchId = batch.Id,
                BatchName = batch.Name,
                Date = Time.GetAppNowTime(),

                ReturnAddress = companyAddress.GetReturnAddress(MarketIdentifier.Empty()),
                PackingSlips = orders,
            };

            var marketplaces = new MarketplaceKeeper(DbFactory, false);
            marketplaces.Init();
            model.Marketplaces = marketplaces.GetAll().Select(m => new PackingSlipMarketplaceInfo(m)).ToList();


            return View("PackingSlip", model);
        }



        public virtual ActionResult GetFile(string fileName)
        {
            LogI("GetFile, filename=" + fileName);

            var path = UrlHelper.GetLabelPath(fileName);
            return File(path, "application/pdf", "label");
        }

        public virtual ActionResult GetLabelPrintFile(long id)
        {
            LogI("GetLabelPrintFile, id=" + id);

            var labelFile = Db.LabelPrintPacks.Get(id);
            if (labelFile != null)
            {
                var model = new LabelPrintPackViewModel(labelFile);
                var path = UrlHelper.GetLabelPath(labelFile.FileName);
                var filename = model.IsReturn ? model.NumberOrPerson : "Label " + model.NumberOrPerson;
                return File(path, FileHelper.GetMimeTypeByExt(Path.GetExtension(path)), filename + Path.GetExtension(path));
            }

            return new EmptyResult();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DAL.Repositories;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.DTO.Sizes;
using Amazon.Model.Implementation;
using Amazon.Web.Filters;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Grid;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.Products;
using Ccen.Common.Helpers;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class QuantityOperationController : BaseController
    {
        public override string TAG
        {
            get { return "QuantityOperationController."; }
        }

        private const string PopupContentView = "QuantityOperationViewModel";

        public virtual ActionResult Index(string type, string styleId)
        {
            var model = new QuantityOperationPageViewModel()
            {
                Type = type,
                StyleId = styleId,
                Users = Db.Users.GetFiltered(x => !x.Deleted).ToList().Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() }).OrderBy(x => x.Text).ToList(),
                Types = Enum.GetValues(typeof(QuantityOperationType)).Cast<QuantityOperationType>()
                .Select(x => new SelectListItem() 
                { 
                    Value = ((int)x).ToString(), 
                    Text = EnumHelper<QuantityOperationType>.GetEnumDescription(x.ToString()) 
                }).ToList()
            };
            return View(model);
        }

        public virtual ActionResult AddOperation(string type, string styleId)
        {
            LogI("AddOperation, type=" + type + ", styleId=" + styleId);
            
            var model = new QuantityOperationViewModel()
            {
                Type = StringHelper.TryGetEnum(type, QuantityOperationType.Exchange),
                StyleId = styleId
            };
            ViewBag.PartialViewName = PopupContentView;
            return View("EditNew", model);
        }

        public virtual JsonResult GetOrderItems(string orderId)
        {
            try
            {
                var orderItems = Db.Listings.GetOrderItems(orderId).ToList()
                    .Select(i => new QuantityItemViewModel(i)).ToList();
                return new JsonResult()
                {
                    Data = ValueResult<IList<QuantityItemViewModel>>.Success("", orderItems),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception ex)
            {
                LogE("GetOrderItems, orderId=" + orderId, ex);
                return new JsonResult()
                {
                    Data = MessageResult.Error(ex.Message),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        
        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            string orderNumber,
            string styleString,
            long? styleItemId,
            long? userId,
            long? typeId,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            LogI("GetAll");
            var filter = new QuantityOperationFilterViewModel()
            {
                OrderNumber = orderNumber,
                StyleString = styleString,
                StyleItemId = styleItemId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                UserId = userId,
                Type = (QuantityOperationType?)typeId
            };
            var items = QuantityOperationViewModel.GetAll(Db, filter);

            var sortField = Request.Params["sort[0][field]"];
            var sortDir = Request.Params["sort[0][dir]"];
            if (!String.IsNullOrEmpty(sortField))
            {
                request.Sorts = new List<SortDescriptor>()
                {
                    new SortDescriptor()
                    {
                        Member = sortField,
                        SortDirection = sortDir == "desc" ? ListSortDirection.Descending : ListSortDirection.Ascending
                    }
                };
            };
                /* new List<SortDescriptor>()
            {
                new SortDescriptor() { Member = "CreateDate", SortDirection = System.ComponentModel.ListSortDirection.Descending }
            };*/
            var dataSource = items.ToDataSourceResult(request);
            return JsonGet(dataSource);
        }

        public virtual JsonResult Delete(long operationId)
        {
            LogI("Delete, operationId=" + operationId);

            QuantityOperationViewModel.Delete(Db,
                QuantityManager,
                Cache, 
                operationId, 
                Time.GetAppNowTime(), 
                AccessManager.UserId);

            return new JsonResult() {Data = MessageResult.Success(), JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }
        
        [HttpPost]
        public virtual ActionResult Submit(QuantityOperationViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var errors = model.Validate(Db);
                if (!errors.Any())
                {
                    var quantityManager = new QuantityManager(LogService, Time);

                    var id = model.Add(Db, 
                        quantityManager, 
                        Cache,
                        Time.GetAppNowTime(), 
                        AccessManager.UserId);
                }
                else
                {
                    foreach (var error in errors)
                        ModelState.AddModelError("", error.ErrorMessage);

                    return PartialView(PopupContentView, model);
                }

                return Json(new UpdateRowViewModel(model,
                    "quantityOperationGrid",
                    new[]
                    {
                        "Type"
                    },
                    false));
            }

            return View(PopupContentView, model);
        }
    }
}

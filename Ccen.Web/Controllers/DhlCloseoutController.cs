using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Web.UI;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Bargains;
using Amazon.Web.ViewModels.CustomBarcodes;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Products;
using Amazon.Web.ViewModels.Statuses;
using Ccen.Web;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class DhlCloseoutController : BaseController
    {
        public override string TAG
        {
            get { return "DhlCloseoutController."; }
        }

        //
        // GET: /Size/
        [OutputCache(CacheProfile = "MiddleTimeProfile")]
        public virtual ActionResult GetInfo()
        {
            LogI("GetInfo");
            var model = DhlCloseoutInfoViewModel.GetInfo(Db);
            return JsonGet(CallResult<DhlCloseoutInfoViewModel>.Success(model));
        }

        public virtual ActionResult GetCloseoutForm()
        {
            LogI("GetCloseoutForm");

            var shipmentProvider = ServiceFactory.GetShipmentProviderByType(
                    ShipmentProviderType.DhlECom, 
                    LogService,
                    Time,
                    DbFactory,
                    WeightService,
                    AccessManager.Company.ShipmentProviderInfoList,
                    AppSettings.DefaultCustomType,
                    AppSettings.LabelDirectory,
                    AppSettings.ReserveDirectory,
                    AppSettings.TemplateDirectory);

            var model = new DhlCloseoutFormViewModel(LogService, Time);
            var result = model.Closeout(Db, 
                shipmentProvider,
                PdfMaker,
                AppSettings.LabelDirectory,
                AppSettings.IsSampleLabels,
                AccessManager.UserId);

            var cacheToRemove = Url.Action("GetInfo", "DhlCloseoutController");
            HttpResponse.RemoveOutputCacheItem(path: cacheToRemove);

            if (result.IsSuccess)
            {
                return JsonGet(ValueResult<string>.Success("", result.Data));
            }
            else
            {
                return JsonGet(ValueResult<string>.Error(result.Message));
            }
        }
    }
}

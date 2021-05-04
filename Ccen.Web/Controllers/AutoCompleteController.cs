using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Helpers;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Messages;

namespace Amazon.Web.Controllers
{
    [Authorize]
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public partial class AutoCompleteController : BaseController
    {
        public override string TAG
        {
            get { return "AutoCompleteController."; }
        }
        
        public virtual ActionResult GetOrderIdList(//REMOVE: [DataSourceRequest] DataSourceRequest request,
                string filter)
        {
            //Прочитать: http://www.telerik.com/blogs/using-kendo-ui-with-mvc4-webapi-odata-and-ef
            //http://blog.falafel.com/connect-kendoui-autocomplete-mvc-action/
            filter = StringHelper.TrimWhitespace(filter);
            return Json(AutoCompleteViewModel.GetOrderIdList(Db, filter), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetStyleGroupNameList(string filter)
        {
            filter = StringHelper.TrimWhitespace(filter);
            return Json(AutoCompleteViewModel.GetStyleGroupNameList(Db, filter), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetStyleIdList(string filter)
        {
            filter = StringHelper.TrimWhitespace(filter);
            return Json(AutoCompleteViewModel.GetStyleStringList(Db, filter), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetSearchTextList(string filter)
        {
            filter = StringHelper.TrimWhitespace(filter);
            return Json(AutoCompleteViewModel.GetSearchTextList(Db, filter), JsonRequestBehavior.AllowGet);
        }

        //Only for VendorOrder
        public virtual ActionResult GetSizeList(string filter)
        {
            filter = StringHelper.TrimWhitespace(filter);
            return Json(AutoCompleteViewModel.GetSizeList(Db, filter), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetColorList(string filter)
        {
            filter = StringHelper.TrimWhitespace(filter);
            return Json(AutoCompleteViewModel.GetColorList(Db, filter), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetSizeListByGroup()
        {
            return Json(AutoCompleteViewModel.GetSizeListByGroup(Db), JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetStyleSizes(string styleString, bool? onlyWithQty)
        {
            try
            {
                var styleItems = Db.StyleItems
                    .GetByStyleIdWithRemainingAsDto(styleString)
                    .OrderBy(s => SizeHelper.GetSizeIndex(s.Size))
                    .ThenBy(s => s.Color)
                    .ToList();

                if (onlyWithQty == true)
                    styleItems = styleItems.Where(si => si.RemainingQuantity > 0).ToList();

                var list = styleItems.Select(i => new SelectListItemTag()
                {
                    Text = i.Size + (!String.IsNullOrEmpty(i.Color) ? " / " + i.Color : ""),
                    Value = i.StyleItemId.ToString(),
                    Tag = i.RemainingQuantity.ToString()
                }).ToList();
                return new JsonResult()
                {
                    Data = ValueResult<IList<SelectListItemTag>>.Success("", list),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception ex)
            {
                LogE("GetStyleSizes, styleId=" + styleString, ex);
                return new JsonResult()
                {
                    Data = MessageResult.Error(ex.Message),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
}

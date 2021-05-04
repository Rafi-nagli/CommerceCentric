using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Inventory;
using ImageResizer;
using Kendo.Mvc.Extensions;
using Amazon.Utils;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.Products;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using System.Web.SessionState;

namespace Amazon.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class ImageController : BaseController
    {
        public override string TAG
        {
            get { return "ImageController."; }
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult MarketImages()
        {
            return View(new MarketImagesPageViewModel());
        }

        public virtual ActionResult StyleImages()
        {
            return View();
        }

        public virtual ActionResult GetAllMarketImages([DataSourceRequest]DataSourceRequest request, 
            int? market, 
            string marketplaceId,
            string keywords,
            bool onlyIgnored)
        {
            LogI("GetAllMarketImages, market=" + market 
                + ", marketplaceId=" + marketplaceId 
                + ", keywords=" + keywords
                + ", onlyIgnored=" + onlyIgnored);

            request.Sorts = BuildFrom(this.Request.Params);

            var items = MarketImageViewModel.GetAll(Db,
                (MarketType)(market ?? (int) MarketType.Walmart),
                marketplaceId,
                keywords,
                onlyIgnored);

            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetAllStyleImages([DataSourceRequest]DataSourceRequest request,
            string keywords,
            bool styleWithLoRes,
            bool withMarketHiRes)
        {
            LogI("GetAllMarketImages, keywords=" + keywords
                + ", styleWithLoRes=" + styleWithLoRes
                + ", withMarketHiRes=" + withMarketHiRes);

            request.Sorts = BuildFrom(this.Request.Params);

            var items = StyleImageViewModel.GetAll(Db,
                keywords,
                styleWithLoRes,
                withMarketHiRes);

            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult ReplaceStyleImage(long id)
        {
            LogI("ReplaceStyleImage, itemImageId=" + id);

            var result = MarketImageViewModel.ReplaceStyleImage(Db, LogService, id, Time.GetAppNowTime(), AccessManager.UserId);

            if (result.IsSuccess)
                return JsonGet(ValueResult<string>.Success(result.Data));
            return JsonGet(ValueResult<string>.Error(result.Message));
        }

        public virtual ActionResult ResetItemLargeImage(long id)
        {
            MarketImageViewModel.ResetItemLargeImage(Db, LogService, id, false);

            return JsonGet(MessageResult.Success());
        }

        public virtual ActionResult ResetItemLargeIncludeParentImage(long id)
        {
            MarketImageViewModel.ResetItemLargeImage(Db, LogService, id, true);

            return JsonGet(MessageResult.Success());
        }

        public virtual ActionResult ResetItemImageDiff(long id)
        {
            MarketImageViewModel.ResetItemImageDiff(Db, LogService, id);

            return JsonGet(MessageResult.Success());
        }

        public virtual ActionResult ToggleIgnoreItemImage(long id, bool isIgnored)
        {
            MarketImageViewModel.ToggleIgnoreItemImage(Db, LogService, id, isIgnored);

            return JsonGet(MessageResult.Success());
        }

        private IList<SortDescriptor> BuildFrom(NameValueCollection values)
        {
            if (this.Request.Params.AllKeys.Contains("sort[0][field]"))
            {
                var dir = ListSortDirection.Ascending;
                if (this.Request.Params.AllKeys.Contains("sort[0][dir]"))
                    dir = this.Request.Params.GetValues("sort[0][dir]")[0] == "desc"
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;

                return new List<SortDescriptor>()
                {
                    new SortDescriptor(this.Request.Params.GetValues("sort[0][field]")[0],
                        dir)
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// http://stackoverflow.com/questions/7319842/mvc3-razor-thumbnail-resize-image-ideas
        /// .net method: http://stackoverflow.com/questions/2808887/create-thumbnail-image?rq=1
        /// <img src="@Url.Action("Thumbnail", "SomeController", new { path=Image, width = 100, height = 50 })" alt="thumb" />
        /// </summary>
        /// <param name="path"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public virtual ActionResult Thumbnail(string path, int width, int height, string rotate)
        {
            try
            {
                path = StringHelper.TrimWhitespace(HttpUtility.UrlDecode(path));

                //NOTE: encode/decode lose double slash
                if (path.StartsWith("http:/") && !path.StartsWith("http://"))
                    path = path.Replace("http:/", "http://");

                if (path.StartsWith("https:/") && !path.StartsWith("https://"))
                    path = path.Replace("https:/", "https://");

                if (path.StartsWith("http") || path.StartsWith("https"))
                {
                    var fileExt = Path.GetExtension(Models.UrlHelper.RemoveUrlParams(path));
                    var hash = MD5Utils.GetMD5HashAsString(path) + fileExt;
                    var thumbnailFile = Server.MapPath("~/Cache/" + "W" + width + "xH" + height + "xR" + rotate + "x" + hash);
                    if (System.IO.File.Exists(thumbnailFile))
                        return File(thumbnailFile, "image/png");

                    LogI("Generate thumbnail from downloading image: " + path);
                    
                    using (var srcImage = Image.FromStream(ImageHelper.DownloadRemoteImageFileAsStream(path)))
                    {
                        ThumbnailHelper.GenerateThumbnail(srcImage,
                            thumbnailFile,
                            width,
                            height,
                            rotate);

                        return File(thumbnailFile, 
                            FileHelper.GetMimeTypeByExt(Path.GetExtension(thumbnailFile)));
                    }
                }
                else
                {
                    var imageFile = Models.UrlHelper.GetPathFromRelativeImageUrl(path);
                    //var imageFile = Server.MapPath(path); // Path.Combine(Server.MapPath("~/App_Data"), path);

                    //NOTE: for eBay
                    var fileExt = Path.GetExtension(path);
                    var filename = FileHelper.PrepareFileName(Path.GetFileNameWithoutExtension(path));

                    var thumbnailFile = Server.MapPath("~/Cache/" + "W" + width + "xH" + height + "xR" + rotate + "x" + filename + fileExt);
                    if (System.IO.File.Exists(thumbnailFile))
                        return File(thumbnailFile, "image/png");

                    //WebImage img = new WebImage(path).Save();
                    if (System.IO.File.Exists(imageFile))
                    {
                        LogI("Generate thumbnail from local image: " + path);
                        using (var srcImage = Image.FromFile(imageFile))
                        {
                            ThumbnailHelper.GenerateThumbnail(srcImage,
                                thumbnailFile,
                                width,
                                height,
                                rotate);
                        }
                        return File(thumbnailFile, 
                            FileHelper.GetMimeTypeByExt(Path.GetExtension(thumbnailFile)));
                    }
                }
            }
            catch (Exception ex)
            {
                LogE("Unable to display thumbnails", ex);
            }

            LogI("Display NO_IMAGE_URL for path=" + path);
            return File(Server.MapPath(ImageHelper.NO_IMAGE_URL), 
                FileHelper.GetMimeTypeByExt(Path.GetExtension(ImageHelper.NO_IMAGE_URL)));
        }

        [AllowAnonymous]
        public virtual ActionResult GetUploadImage(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var path = Models.UrlHelper.GetUploadImagePath();
            var file = Path.Combine(path, filename);
            
            if (System.IO.File.Exists(file))
                return File(file, FileHelper.GetMimeTypeByExt(Path.GetExtension(filename)));
            return new HttpNotFoundResult();
        }

        [AllowAnonymous]
        public virtual ActionResult Swatch(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var path = Models.UrlHelper.GetSwatchImagePath();
            var file = Path.Combine(path, filename);

            if (System.IO.File.Exists(file))
                return File(file, FileHelper.GetMimeTypeByExt(Path.GetExtension(filename)));
            return new HttpNotFoundResult();
        }

        [AllowAnonymous]
        public virtual ActionResult Walmart(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var path = Models.UrlHelper.GetWalmartImagePath();
            var file = Path.Combine(path, filename);

            if (System.IO.File.Exists(file))
                return File(file, FileHelper.GetMimeTypeByExt(Path.GetExtension(filename)));
            return new HttpNotFoundResult();
        }

        [AllowAnonymous]
        public virtual ActionResult Jet(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var path = Models.UrlHelper.GetJetImagePath();
            var file = Path.Combine(path, filename);

            if (System.IO.File.Exists(file))
                return File(file, FileHelper.GetMimeTypeByExt(Path.GetExtension(filename)));
            return new HttpNotFoundResult();
        }

        [AllowAnonymous]
        public virtual ActionResult eBay(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var path = Models.UrlHelper.GetEBayImagePath();
            var file = Path.Combine(path, filename);

            if (System.IO.File.Exists(file))
                return File(file, FileHelper.GetMimeTypeByExt(Path.GetExtension(filename)));
            return new HttpNotFoundResult();
        }
        
        [AllowAnonymous]
        public virtual ActionResult Groupon(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var path = Models.UrlHelper.GetGrouponImagePath();
            var file = Path.Combine(path, filename);

            if (System.IO.File.Exists(file))
                return File(file, FileHelper.GetMimeTypeByExt(Path.GetExtension(filename)));
            return new HttpNotFoundResult();
        }
        
        [AllowAnonymous]
        public virtual ActionResult Raw(string filename)
        {
            filename = HttpUtility.UrlDecode(filename);
            var path = Models.UrlHelper.GetRawImagePath();
            var file = Path.Combine(path, filename);

            if (System.IO.File.Exists(file))
                return File(file, FileHelper.GetMimeTypeByExt(Path.GetExtension(filename)));
            return new HttpNotFoundResult();
        }


        //Temporary using both versions with GetUploadImage
        [AllowAnonymous]
        public virtual ActionResult UploadImage(string filename)
        {
            return GetUploadImage(filename);
        }

        //Alias to GetUploadImage
        [AllowAnonymous]
        public virtual ActionResult Get(string filename)
        {
            return GetUploadImage(filename);
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult Save(IEnumerable<HttpPostedFileBase> image1, 
            IEnumerable<HttpPostedFileBase> image2,
            IEnumerable<HttpPostedFileBase> image3,
            IEnumerable<HttpPostedFileBase> image4,
            IEnumerable<HttpPostedFileBase> image5,
            IEnumerable<HttpPostedFileBase> image6,
            IEnumerable<HttpPostedFileBase> image7,
            IEnumerable<HttpPostedFileBase> image8,
            IEnumerable<HttpPostedFileBase> image9,
            IEnumerable<HttpPostedFileBase> image10,
            IEnumerable<HttpPostedFileBase> image11,
            IEnumerable<HttpPostedFileBase> image12)
        {
            var list = new List<IEnumerable<HttpPostedFileBase>>()
            {
                image1, image2, image3, image4, image5, image6,
                image7, image8, image9, image10, image11, image12
            };
            string warning = "";
            list.ForEach(x => CheckUploadedFiles(x, list.IndexOf(x) + 1, ref warning));
            // Return an empty string to signify success
            return Content("");
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult SaveWithCheck(IEnumerable<HttpPostedFileBase> image1,
            IEnumerable<HttpPostedFileBase> image2,
            IEnumerable<HttpPostedFileBase> image3,
            IEnumerable<HttpPostedFileBase> image4,
            IEnumerable<HttpPostedFileBase> image5,
            IEnumerable<HttpPostedFileBase> image6,
            IEnumerable<HttpPostedFileBase> image7,
            IEnumerable<HttpPostedFileBase> image8,
            IEnumerable<HttpPostedFileBase> image9,
            IEnumerable<HttpPostedFileBase> image10,
            IEnumerable<HttpPostedFileBase> image11,
            IEnumerable<HttpPostedFileBase> image12)
        {
            var list = new List<IEnumerable<HttpPostedFileBase>>()
            {
                image1, image2, image3, image4, image5, image6,
                image7, image8, image9, image10, image11, image12
            };
            string warning = "";
            list.ForEach(x => CheckUploadedFiles(x, list.IndexOf(x) + 1, ref warning));

            // Return an empty string to signify success
            return Json(warning);
        }

        public virtual ActionResult SaveSingle([DataSourceRequest] DataSourceRequest request,
            IEnumerable<HttpPostedFileBase> files)
        {
            var savedFilePaths = new List<string>();
            
            if (files != null)
            {
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    if (fileName != null)
                    {
                        var dir = Models.UrlHelper.GetUploadImagePath();
                        fileName = FileHelper.GetNotExistFileName(dir, fileName);
                        var destinationPath = Path.Combine(dir, fileName);

                        file.SaveAs(destinationPath);

                        savedFilePaths.Add(Models.UrlHelper.GetAbsolutePath(Models.UrlHelper.GetUploadImageUrl(fileName)).ToLower());
                    }
                }
            }

            return Json(new[] { savedFilePaths }.ToDataSourceResult(request));
        }

        private void CheckUploadedFiles(IEnumerable<HttpPostedFileBase> files, int order, ref string warning)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.ContentType == "image/jpg" || file.ContentType == "image/jpeg" || file.ContentType == "image/pjpeg")
                    {
                        var im = ImageHelper.GetImageFromStream(file.InputStream);
                        if (!ImageHelper.IsImageSrgbJpeg(im))
                        {
                            warning += "The image has wrong profile please convert it to “SRGB” profile";
                        }
                    }
                    // Some browsers send file names with full path. We only care about the file name.
                    var fileName = FileHelper.PrepareFileName(Path.GetFileNameWithoutExtension(file.FileName)) + Path.GetExtension(file.FileName);
                    var dir = Models.UrlHelper.GetUploadImagePath();
                    fileName = FileHelper.GetNotExistFileName(dir, fileName);
                    var destinationPath = Path.Combine(dir, fileName);

                    file.SaveAs(destinationPath);
                    SessionHelper.AddUploadedImage(fileName, order);
                }
            }
        }

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult Remove(string[] fileNames)
        {
            foreach (var fullName in fileNames)
            {
                var fileName = Path.GetFileName(fullName);
                SessionHelper.RemoveUploadedImage(fileName);
            }

            // Return an empty string to signify success
            return Content("");
        }
    }
}

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

namespace Amazon.Web.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public partial class ImageGetController : BaseController
    {
        public override string TAG
        {
            get { return "ImageGetController."; }
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
    }
}

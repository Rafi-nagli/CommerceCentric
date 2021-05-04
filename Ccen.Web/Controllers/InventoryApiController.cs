using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Web.Models;
using log4net;

namespace Amazon.Web.Controllers
{
    public class InventoryApiController : ApiController
    {
        public readonly ILog Log = LogManager.GetLogger("ApiLogger");

        private IUnitOfWork db;
        public IUnitOfWork Db
        {
            get
            {
                if (db == null)
                    db = DbFactory.GetRWDb();
                return db;
            }
        }

        public IDbFactory DbFactory
        {
            get { return new DbFactory(); }
        }

        #region Mobile Device API
        [HttpPost]
        public HttpResponseMessage RegisterPush(PushModel push)
        {
            Log.Info("RegisterPush, push=" + push.ToString());
            try
            {
                var existPush = Db.Pushes.GetByRegistrationId(push.RegistrationId);
                if (existPush == null)
                {
                    Db.Pushes.Add(new Push
                    {
                        DeviceId = push.Email,
                        RegistrationId = push.RegistrationId
                    });
                    Db.Commit();
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Log.Error("Error RegisterPush: ", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Unexpected error has occured. " + ex.Message);
            }
        }

        public HttpResponseMessage GetStyles()
        {
            Log.Info("GetStyles");
            try
            {
                var stylesWithoutImage = Db.Styles.GetAllWithoutImage();
                return Request.CreateResponse(HttpStatusCode.OK, stylesWithoutImage);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting styles: ", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Unexpected error has occured. " + ex.Message);
            }
        }

        public HttpResponseMessage GetUnshippedInfo()
        {
            Log.Info("GetUnshippedInfo");
            try
            {
                var info = Db.Orders.GetUnshippedInfo();
                var settings = new SettingsService(DbFactory);
                var date = DateHelper.ConvertUtcToApp(settings.GetOrdersSyncDate(MarketType.Amazon, MarketplaceKeeper.DefaultMarketplaceId));
                info.LastProcessedDate = date != null ? date.Value.ToString("yyyy-MM-dd HH:mm") : string.Empty;
                return Request.CreateResponse(HttpStatusCode.OK, info);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting unshipped info: ", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Unexpected error has occured. " + ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage SaveImage(long id)
        {
            Log.Info("SaveImage, id=" + id);
            var result = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                var imageString = Request.Content.ReadAsStringAsync().Result;
                if (imageString.Length == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Content is empty");
                }
                var bytes = Convert.FromBase64String(imageString);
#if DEBUG
                var fullPath = string.Format("C://Amazon//InventoryPics//image_{0}.jpg", id);
#else
                var fullPath = string.Format("D://Amazon//InventoryPics//image_{0}.jpg", id);
#endif
                
                using (var imageFile = new FileStream(fullPath, FileMode.Create))
                {
                    imageFile.Write(bytes, 0, bytes.Length);
                    imageFile.Flush();
                }
                var st = Db.Styles.Get(id);
                st.Image = fullPath;
                Log.Info(string.Format("Style id: {0}; image path: {1}", id, fullPath));
                Db.Commit();
            }
            catch (Exception ex)
            {
                Log.Error("Error when saving image for style: " + id, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Unexpected error has occured. " + ex.Message);
            }
            return result;
        }
        #endregion
    }
}

using Amazon.DAL;
using Amazon.DTO.DropShippers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.ViewModels.UploadOrders
{
    public class UploadOrderFeedFilterViewModel
    {
        public int? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }


        public static UploadOrderFeedFilterViewModel Default
        {
            get { return new UploadOrderFeedFilterViewModel(); }
        }


        public SelectList FieldMappingsList
        {
            get
            {
                using (var db = new UnitOfWork())
                {
                    var items = db.Context.CustomFeeds.Select(s => new CustomFeedDTO() { Id = s.Id, FeedName = s.FeedName }).ToList();
                    return new SelectList(items.Select(s => new { Text = s.FeedName, Value = s.Id }).ToList(), "Value", "Text");
                }

            }
        }
    }
}
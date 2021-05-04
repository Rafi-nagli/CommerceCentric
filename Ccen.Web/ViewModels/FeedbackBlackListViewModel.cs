using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http.ModelBinding;
using System.Web.UI.WebControls;
using Amazon.Core;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Kendo.Mvc.Extensions;

namespace Amazon.Web.ViewModels
{
    public class FeedbackBlackListViewModel
    {
        public long? Id { get; set; }
        public string OrderId { get; set; }
        public string Reason { get; set; }

        public DateTime? CreateDate { get; set; }

        public int? Market { get; set; }
        public string MarketplaceId { get; set; }
        public string MarketOrderId { get; set; }

        public DateTime? OrderDate { get; set; }
        
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string AmazonEmail { get; set; }

        public string PersonName { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingZipAddon { get; set; }
        public string ShippingPhone { get; set; }

        public string MarketName
        {
            get
            {
                return MarketHelper.GetMarketName(Market ?? (int)MarketHelper.Default, MarketplaceId);
            }
        }

        public string SellerUrl
        {
            get { return UrlHelper.GetSellerCentralOrderUrl((MarketType)Market, MarketplaceId, OrderId); }
        }

        public override string ToString()
        {
            return "Id=" + Id + 
                ", OrderId=" + OrderId +
                ", Reason=" + Reason + 
                ", CreateDate=" + CreateDate;
        }

        public FeedbackBlackListViewModel()
        {
            
        }

        public FeedbackBlackListViewModel(FeedbackBlackListDto blDto)
        {
            if (blDto == null)
                return;

            Id = blDto.Id;
            OrderId = blDto.OrderId;
            Reason = blDto.Reason;
            CreateDate = blDto.CreateDate;

            MarketOrderId = blDto.MarketOrderId;
            Market = blDto.Market;
            MarketplaceId = blDto.MarketplaceId;
            OrderDate = blDto.OrderDate;
            BuyerName = blDto.BuyerName;
            BuyerEmail = blDto.BuyerEmail;
            AmazonEmail = blDto.AmazonEmail;

            PersonName = blDto.PersonName;
            ShippingCountry = blDto.ShippingCountry;
            ShippingAddress1 = blDto.ShippingAddress1;
            ShippingAddress2 = blDto.ShippingAddress2;
            ShippingCity = blDto.ShippingCity;
            ShippingState = blDto.ShippingState;
            ShippingZip = blDto.ShippingZip;
            ShippingZipAddon = blDto.ShippingZipAddon;
            ShippingPhone = blDto.ShippingPhone;
        }

        public static IEnumerable<FeedbackBlackListViewModel> GetAll(IUnitOfWork db)
        {
            return db.FeedbackBlackLists.GetAllExtendedAsDto()
                .Select(s => new FeedbackBlackListViewModel(s));
        }

        public static bool Validate(IUnitOfWork db, FeedbackBlackListViewModel model, out IList<KeyValuePair<string, string>> errors)
        {
            errors = new List<KeyValuePair<string, string>>();
            var order = db.Orders.UniversalGetByOrderId(model.OrderId);
            if (order == null)
            {
                errors.Add(new KeyValuePair<string, string>("FromOrderId", "Not found"));
            }
            return !errors.Any();
        }

        public static FeedbackBlackListViewModel Add(IUnitOfWork db, FeedbackBlackListViewModel item, DateTime? when, long? by)
        {
            var itemDb = new FeedbackBlackList
            {
                OrderId = item.OrderId,
                Reason = item.Reason,
                CreateDate = when,
                CreatedBy = by
            };
            db.FeedbackBlackLists.Add(itemDb);
            db.Commit();

            item = new FeedbackBlackListViewModel(db.FeedbackBlackLists.GetExtendedByIdAsDto(itemDb.Id));
            //item.Id = itemDb.Id;
            return item;
        }

        public static FeedbackBlackListViewModel Update(IUnitOfWork db, FeedbackBlackListViewModel item, DateTime? when, long? by)
        {
            if (!item.Id.HasValue)
                return null;

            var itemDb = db.FeedbackBlackLists.Get(item.Id.Value);
            if (itemDb != null)
            {
                itemDb.OrderId = item.OrderId;
                itemDb.Reason = item.Reason;
                itemDb.UpdateDate = when;
                itemDb.UpdatedBy = by;

                db.Commit();

                return new FeedbackBlackListViewModel(db.FeedbackBlackLists.GetExtendedByIdAsDto(itemDb.Id));
            }
            return null;
        }

        public static void Delete(IUnitOfWork db, long id)
        {
            var itemDb = db.FeedbackBlackLists.Get(id);
            if (itemDb != null)
            {
                db.FeedbackBlackLists.Remove(itemDb);
                db.Commit();
            }
        }
    }
}
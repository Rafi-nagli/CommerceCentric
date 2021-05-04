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
using Kendo.Mvc.Extensions;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels
{
    public class BuyerBlackListViewModel
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
            get { return UrlHelper.GetSellerCentralOrderUrl((MarketType)(Market ?? (int)MarketHelper.Default), MarketplaceId, OrderId); }
        }

        public override string ToString()
        {
            return "Id=" + Id + 
                ", OrderId=" + OrderId +
                ", Reason=" + Reason + 
                ", CreateDate=" + CreateDate;
        }

        public BuyerBlackListViewModel()
        {
            
        }

        public BuyerBlackListViewModel(BuyerBlackListDto blDto)
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

        public static IEnumerable<BuyerBlackListViewModel> GetAll(IUnitOfWork db)
        {
            return db.BuyerBlackLists.GetAllExtendedAsDto()
                .Select(s => new BuyerBlackListViewModel(s));
        }

        public static bool Validate(IUnitOfWork db, BuyerBlackListViewModel model, out IList<KeyValuePair<string, string>> errors)
        {
            errors = new List<KeyValuePair<string, string>>();
            var order = db.Orders.UniversalGetByOrderId(model.OrderId);
            if (order == null)
            {
                errors.Add(new KeyValuePair<string, string>("FromOrderId", "Not found"));
            }
            return !errors.Any();
        }

        public static BuyerBlackListViewModel Add(IUnitOfWork db, BuyerBlackListViewModel item, DateTime? when, long? by)
        {
            var itemDb = new BuyerBlackList
            {
                OrderId = item.OrderId,
                Reason = item.Reason,
                CreateDate = when,
                CreatedBy = by
            };
            db.BuyerBlackLists.Add(itemDb);
            db.Commit();

            item = new BuyerBlackListViewModel(db.BuyerBlackLists.GetExtendedByIdAsDto(itemDb.Id));
            //item.Id = itemDb.Id;
            return item;
        }

        public static BuyerBlackListViewModel Update(IUnitOfWork db, BuyerBlackListViewModel item, DateTime? when, long? by)
        {
            if (!item.Id.HasValue)
                return null;

            var itemDb = db.BuyerBlackLists.Get(item.Id.Value);
            if (itemDb != null)
            {
                itemDb.OrderId = item.OrderId;
                itemDb.Reason = item.Reason;
                itemDb.UpdateDate = when;
                itemDb.UpdatedBy = by;

                db.Commit();

                return new BuyerBlackListViewModel(db.BuyerBlackLists.GetExtendedByIdAsDto(itemDb.Id));
            }
            return null;
        }

        public static void Delete(IUnitOfWork db, long id)
        {
            var itemDb = db.BuyerBlackLists.Get(id);
            if (itemDb != null)
            {
                db.BuyerBlackLists.Remove(itemDb);
                db.Commit();
            }
        }
    }
}
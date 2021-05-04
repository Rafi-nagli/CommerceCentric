using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.ViewModels.Html;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Ccen.Web.ViewModels.CustomReports
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CustomReportAdditionalFieldAttribute : CustomReportFieldAttribute
    {
    }

    public class CustomReportFieldAttribute : Attribute
    {
        public string EntityName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string Title { get; set; }
        public int Width { get; set; }
        public List<SelectListItemEx> Items { get; set; }
        public bool Multi { get; set; } 

        public Func<IUnitOfWork, List<SelectListItemEx>> GetItemsFromDbFunc { get; set; }

        private CustomReportFieldAttribute(CustomReportFieldAttribute atr)
        {
            EntityName = atr.EntityName;
            ColumnName = atr.ColumnName;
            DataType = atr.DataType;
            Title = atr.Title;
            Width = atr.Width;
            Items = atr.Items;
            GetItemsFromDbFunc = atr.GetItemsFromDbFunc;
            Multi = atr.Multi;
        }

        public CustomReportFieldAttribute(CustomReportFields field) : this(Predefined[field])
        {            
        }

        public CustomReportFieldAttribute()
        {
        }

        private static IDictionary<CustomReportFields, CustomReportFieldAttribute> Predefined => new Dictionary<CustomReportFields, CustomReportFieldAttribute>() 
        {
            [CustomReportFields.Style_Id] = Style_Id,
            [CustomReportFields.StyleID] = StyleID,
            [CustomReportFields.OrderDate] = OrderDate,
            [CustomReportFields.Market] = Market,
            [CustomReportFields.Gender] = Gender,
            [CustomReportFields.ItemStyle] = ItemStyle,
            [CustomReportFields.MainLicense] = MainLicense,
        };

        private static CustomReportFieldAttribute StyleID => new CustomReportFieldAttribute()
        {
            DataType = "string",
            EntityName = "Styles",
            ColumnName = "StyleID",
            Title = "Style Id",
            Width = 200
        };

        private static CustomReportFieldAttribute Style_Id => new CustomReportFieldAttribute()
        {
            DataType = "long",
            EntityName = "Styles",
            ColumnName = "id",
            Title = "Id",
            Width = 80
        };

        private static CustomReportFieldAttribute OrderDate => new CustomReportFieldAttribute()
        {
            DataType = "datetime",
            Title = "",
            EntityName = "Orders",
            ColumnName = "OrderDate",
            Width = 150
        };        

        private static CustomReportFieldAttribute Market => new CustomReportFieldAttribute()
        {
            DataType = "int32",            
            EntityName = "Orders",
            ColumnName = "Market", 
            GetItemsFromDbFunc = (db)=>
            {
                var marketplaceList = db.Marketplaces.GetAllAsDto().Where(m => m.IsActive).Select(m => new SelectListItemEx()
                {
                    Text = MarketHelper.GetMarketName(m.Market, m.MarketplaceId),
                    Value = m.Market.ToString()
                });
                return marketplaceList.ToList();
            }
        };
        private static CustomReportFieldAttribute ItemStyle => new CustomReportFieldAttribute()
        {
            DataType = "int32",
            EntityName = "FeatureValues",
            ColumnName = "ItemStyle",
            Multi = true,
            GetItemsFromDbFunc = (db) =>
            {
                var list = db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.ITEMSTYLE).Select(m => new SelectListItemEx()
                {
                    Text = m.Value,
                    Value = m.Id.ToString()
                });
                return list.ToList();
            }
        };

        private static CustomReportFieldAttribute Gender => new CustomReportFieldAttribute()
        {
            DataType = "int32",
            EntityName = "StyleCaches",
            ColumnName = "Gender",
            Multi = true,
            GetItemsFromDbFunc = (db) =>
            {
                var list = db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.GENDER).Select(m => new SelectListItemEx()
                {
                    Text = m.Value,
                    Value = m.Id.ToString()
                });
                return list.ToList();
            }
        };

        private static CustomReportFieldAttribute MainLicense => new CustomReportFieldAttribute()
        {
            DataType = "int32",
            EntityName = "StyleCaches",
            ColumnName = "MainLicense",
            GetItemsFromDbFunc = (db) =>
            {
                var list = db.FeatureValues.GetValuesByFeatureId(StyleFeatureHelper.MAIN_LICENSE).Select(m => new SelectListItemEx()
                {
                    Text = m.Value,
                    Value = m.Id.ToString()
                });
                return list.ToList();
            }
        };
    }

    public enum CustomReportFields
    {
        [Description("long Style.Id")]
        Style_Id,
        StyleID,
        OrderDate,
        Market,
        ItemStyle,
        Gender,
        MainLicense
    }
}

    
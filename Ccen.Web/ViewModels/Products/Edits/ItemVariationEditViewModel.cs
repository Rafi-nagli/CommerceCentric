﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Web.Models.Exports;

namespace Amazon.Web.ViewModels.Products
{
    public class ItemVariationEditViewModel
    {
        public int? Id { get; set; }
        public long? ListingEntityId { get; set; }

        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public long? StyleItemId { get; set; }
        public string StyleSize { get; set; }
        public string StyleColor { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
        public bool IsPrime { get; set; }
        public string SKU { get; set; }

        public string Barcode { get; set; }
        public bool AutoGeneratedBarcode { get; set; }

        public int AutoGeneratedBarcodeInt
        {
            get { return AutoGeneratedBarcode ? 1 : 0; }
            set { AutoGeneratedBarcode = value == 0 ? false : true; }
        }

        public bool IsExistOnAmazon { get; set; }

        public bool IsDefault { get; set; }

        public int PublishedStatus { get; set; }

        public int RealQuantity { get; set; }
        
        public bool IsSelected { get; set; }


        public int? OverridePublishedStatus { get; set; }

        public ItemVariationEditViewModel()
        {
            
        }

        public ItemVariationEditViewModel(ItemDTO item, bool isEditMode)
        {
            if (isEditMode)
            {
                Id = item.Id;
                Barcode = item.Barcode;
                ListingEntityId = item.ListingEntityId;
            }
            else
            {
                if (item.Market == (int)MarketType.Walmart || item.Market == (int)MarketType.WalmartCA)
                {
                    AutoGeneratedBarcode = true;
                }
                else
                {
                    Barcode = item.Barcode;
                }
            }


            StyleString = item.StyleString;
            StyleId = item.StyleId;
            StyleItemId = item.StyleItemId;
            StyleSize = item.StyleSize;
            StyleColor = item.StyleColor;

            Size = item.Size;
            Color = item.Color;

            Price = item.CurrentPrice;
            IsPrime = item.IsPrime;
            SKU = item.SKU;
            IsDefault = item.IsDefault;
            IsExistOnAmazon = item.IsExistOnAmazon ?? false;

            PublishedStatus = item.PublishedStatus;

            RealQuantity = item.RealQuantity;

            IsSelected = true;
        }
    }
}
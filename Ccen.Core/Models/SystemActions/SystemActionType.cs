using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum SystemActionType
    {
        None = 0,
        UpdateOnMarketCancelOrder = 1,

        //UpdateOnMarketProductData = 4, //TODO:
        UpdateOnMarketReturnOrder = 5,
        UpdateOnMarketProductRelationship = 6,
        UpdateOnMarketProductImage = 7,
        UpdateOnMarketProductPriceRule = 8,
        UpdateOnMarketProductWeight = 9,

        ProcessBatch = 10,

        PrintBatch = 12,

        UpdateOnMarketProductQuantity = 15,

        AddComment = 30,

        UpdateRates = 32,

        QuantityDistribute = 35,

        UpdateListings = 51,
        
        ProcessUploadedSales = 55,
        ProcessPublishFeed = 60,
        ProcessInventoryFeed = 65,
        ProcessUploadOrderFeed = 66,

        DeleteOnMarketProduct = 70,

        SetOnHold = 75,

        BulkEdit = 80,

        UpdateOnMarketFeature = 200,
        UpdateOnMarketFeatureOption = 201,
        
        SendEmail = 101,
        UpdateCache = 1001,

        ListingPriceRecalculation = 2001,

        UpdateOnMarketProductTags = 9001,
        UpdateOnMarketProductTitle = 9002,
        UpdateOnMarketProductBarcode = 9003,
        UpdateOnMarketProductData = 9004,
        UpdateOnMarketProductVendorName = 9005,

        RestartService = 100001,
    }
}

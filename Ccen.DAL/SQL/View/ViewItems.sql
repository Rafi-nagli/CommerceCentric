SELECT     Id, ASIN, ParentASIN, Barcode, Size, ConvertedSize, StyleSize, Title, 

					  Market, MarketplaceId, 
					  SKU, IsFBA, ListingId, ListingEntityId, CreateDate, UpdateDate, UserId, StyleString, StyleId, 
                      DisplayQuantity, RealQuantity, CurrentPrice, 
					  
					  AutoQuantity, AutoQuantityUpdateDate,

					  TargetPrice, QuantityTillChangePrice, 
					  SalePrice, SaleStartDate, SaleEndDate, MaxPiecesOnSale, PiecesSoldOnSale,	  
					  OpenDate,
					  
					  LowestPrice, Weight, ShippingSize, ShippingSizeName, Color, ItemPicture, 
                      IsAmazonParentASIN, IsRemoved, ItemNumber, AmazonRealQuantity, AmazonCurrentPrice, 
					  OnHold, RestockDate, EstimatedOrderHandlingFeePerOrder, 
                      EstimatedPickPackFeePerUnit, EstimatedWeightHandlingFeePerUnit
FROM         dbo.ViewItemsWithRemoved
WHERE     (IsRemoved = 0)
SELECT     i.Id, i.ASIN, 
												i.ParentASIN, 
												i.Barcode, 
												i.Size,
												i.Title,  

												ISNULL(isi.Size, i.Size) AS ConvertedSize, 
												isi.Size AS StyleSize, 
												isi.StyleItemId as StyleItemId,

											  ls.Market, ls.MarketplaceId,
											  ls.SKU, ls.IsFBA, ls.ListingId, 
                                              ls.Id AS ListingEntityId, i.CreateDate, i.UpdateDate, i.UserId, 
											  st.StyleID AS StyleString, st.Id AS StyleId, 
											  
											  ls.DisplayQuantity, CASE WHEN ls.IsFBA = 1 THEN fbaLs.QuantityAvailable ELSE ls.RealQuantity END AS RealQuantity, 
											  ls.OnHold, ls.RestockDate, ls.CurrentPrice, 
											  ls.AutoQuantity, ls.AutoQuantityUpdateDate,
											  ls.TargetPrice, ls.QuantityTillChangePrice, 
											  ls.SalePrice, ls.SaleStartDate, ls.SaleEndDate, ls.MaxPiecesOnSale, ls.PiecesSoldOnSale,
											  ls.OpenDate,

											  ls.LowestPrice, ls.AmazonRealQuantity, 
                                              ls.AmazonCurrentPrice, fbaFee.EstimatedOrderHandlingFeePerOrder, fbaFee.EstimatedPickPackFeePerUnit, fbaFee.EstimatedWeightHandlingFeePerUnit, 
                                              CASE WHEN isi.Weight IS NULL THEN 0 ELSE isi.Weight END AS Weight, 
											  i.Color, 

                                              CASE WHEN i.PrimaryImage IS NULL OR i.PrimaryImage = '' THEN st.Image ELSE i.PrimaryImage END AS ItemPicture,
											   
											  i.IsAmazonParentASIN, ls.IsRemoved
FROM         dbo.Items AS i LEFT OUTER JOIN
                      dbo.ViewActualListings AS ls ON ls.ItemId = i.Id LEFT OUTER JOIN
                      dbo.ListingFBAInvs AS fbaLs ON fbaLs.SellerSKU = ls.SKU LEFT OUTER JOIN
                      dbo.ListingFBAEstFees AS fbaFee ON fbaFee.SKU = ls.SKU LEFT OUTER JOIN
					  dbo.ViewItemToStyleItems AS isi ON isi.ItemId = i.Id LEFT OUTER JOIN
                      dbo.Styles AS st ON i.StyleId = st.Id



SELECT     l.Id, l.Market, l.MarketplaceId, l.ListingId, l.SKU, l.IsFBA, l.IsRemoved, l.ItemId,  l.CurrentPrice, l.RestockDate,
			siToItem.Weight, siToItem.Id AS StyleItemId, siToItem.Size AS ConvertedSize, siToItem.Quantity,
			s.StyleID as StyleString, s.Id as StyleId,
			i.ASIN, i.Size, i.Color, i.ParentASIN, i.Title, i.AdditionalImages AS ItemPicture, 
			sCache.ShippingSizeValue AS ShippingSize,
			sCache.InternationalPackageValue as InternationalPackage,
            p.Rank, CASE WHEN p.ImageSource IS NULL OR p.ImageSource = '' THEN s.Image ELSE p.ImageSource END AS Picture
FROM         dbo.Listings AS l INNER JOIN
                      dbo.Items AS i ON l.ItemId = i.Id LEFT OUTER JOIN
                      dbo.ParentItems AS p ON i.ParentASIN = p.ASIN AND i.Market = p.Market AND i.MarketplaceId = i.MarketplaceId LEFT OUTER JOIN
                      dbo.Styles AS s ON i.StyleId = s.Id LEFT OUTER JOIN
                      dbo.StyleCaches AS sCache ON s.Id = sCache.Id LEFT OUTER JOIN
                          (SELECT     *
                            FROM          (SELECT     si.Weight, si.Id, si.Size, si.StyleId, i.Id AS ItemId, si.Quantity, ROW_NUMBER() OVER (PARTITION BY si.StyleId, i.Id
                                                    ORDER BY si.Weight DESC, si.Quantity DESC, si.Id DESC) AS RowNumber
                            FROM          dbo.StyleItems AS si LEFT OUTER JOIN
                                                   dbo.SizeMappings AS sm ON sm.StyleSize = si.Size INNER JOIN
                                                   dbo.Items AS i ON i.StyleId = si.StyleId AND ISNULL(CASE sm.Id WHEN NULL THEN si.Size ELSE sm.ItemSize END, N'') = ISNULL(i.Size, N'')) 
							AS siRowNum
							WHERE     RowNumber = 1) AS siToItem ON siToItem.ItemId = i.Id
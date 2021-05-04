SELECT     Id AS ListingId, 
			MAX(StyleItemId) AS StyleItemId, 
			MAX(StyleId) AS StyleId, 
			MAX(ConvertedSize) AS Size, 
			MAX(InventoryQuantity) AS InventoryQuantity, 
            MAX(InventorySetDate) AS InventorySetDate, Market, MarketplaceId, 
			SUM(SoldQuantity) AS SoldQuantity, 
			SUM(TotalSoldQuantity) AS TotalSoldQuantity,
			MAX(OrderDate) AS MaxOrderDate, 
            MAX(OrderDate) AS MinOrderDate, 
			COUNT(OrderDate) AS OrderCount
FROM         
(
	SELECT 
		soldQ.Id, 
		invQ.Id as StyleItemId, 
		soldQ.StyleId as StyleId, 
		soldQ.ConvertedSize, 
		soldQ.Market, 
		soldQ.MarketplaceId,
		soldQ.OrderDate,
		invQ.QuantitySetDate AS InventorySetDate,
		invQ.Quantity AS InventoryQuantity, 
		soldQ.Quantity as TotalSoldQuantity,  
		(case when (invQ.QuantitySetDate IS NULL OR invQ.QuantitySetDate <= OrderDate) 
			then soldQ.Quantity 
			else 0 end) as SoldQuantity 
	FROM  
		dbo.ViewInventoryQuantities AS invQ 
		
		INNER JOIN

		(SELECT     l.Id, 
				i.Styleid as StyleId,
				i.Size, 
				l.ItemId, 
				l.ListingId, 
				ISNULL(sm.StyleSize, i.Size) AS ConvertedSize, 
				m.Quantity, 
				o.OrderDate, 
				o.Market, 
				o.MarketplaceId
			FROM		dbo.Orders AS o INNER JOIN
									dbo.OrderShippingInfoes AS sh ON o.Id = sh.OrderId INNER JOIN
									dbo.ItemOrderMappings AS m ON m.ShippingInfo_Id = sh.Id INNER JOIN
									dbo.Listings AS l ON m.Listing_Id = l.Id INNER JOIN
									 (SELECT     Id, Size, StyleId
                                        FROM          dbo.Items AS it
                                        UNION
                                        SELECT     it.Id, it.Size, sr.LinkedStyleId AS StyleId
                                        FROM         dbo.Items AS it INNER JOIN
                                                            dbo.StyleReferences AS sr ON it.StyleId = sr.StyleId) AS i 
										ON i.Id = l.ItemId LEFT OUTER JOIN
									dbo.SizeMappings AS sm ON ISNULL(i.Size, N'') = ISNULL(sm.ItemSize, N'') LEFT OUTER JOIN
									dbo.StyleReferences AS stRef ON stRef.StyleId = i.StyleId

			WHERE      (sh.IsActive = 1) 
						AND (i.StyleId IS NOT NULL) 
						AND (o.OrderStatus = 'Shipped' OR
							o.OrderStatus = 'Pending' OR
							o.OrderStatus = 'PartiallyShipped' OR
							o.OrderStatus = 'Unshipped')
		) AS soldQ 

		ON invQ.Size = soldQ.ConvertedSize AND (InvQ.StyleId = soldQ.StyleId)

) as allOrders

GROUP BY Id, StyleId, Market, MarketplaceId
SELECT     s.Id, s.Image, s.Type, s.Name, s.DisplayName, 
				siStatus.HasManuallyQuantity, 
				siStatus.Quantity as ManuallyQuantity,
				(CASE WHEN oq.Quantity IS NULL THEN 0 ELSE oq.Quantity END) + (CASE WHEN sq.Quantity IS NULL THEN 0 ELSE sq.Quantity END) AS BoxQuantity, 
				(CASE WHEN oq.Price IS NULL THEN 0 ELSE oq.Price END) + (CASE WHEN sq.Price IS NULL THEN 0 ELSE sq.Price END) AS BoxTotalPrice, 				
				s.StyleID, s.StatusId, st.Name AS StatusName, s.ItemTypeId AS ItemTypeId, t .Name AS ItemTypeName, s.UpdateDate, s.CreateDate, 
                pItem.ParentASIN AS AssociatedASIN, pItem.Market AS AssociatedMarket, pItem.MarketplaceId AS AssociatedMarketplaceId
FROM         dbo.Styles AS s LEFT OUTER JOIN
                      dbo.ItemTypes AS t ON ISNULL(s.ItemTypeId, 1) = t .Id 
					  LEFT OUTER JOIN
                      dbo.Statuses AS st ON s.StatusId = st.Id 
					  LEFT OUTER JOIN
                          (SELECT     StyleID, SUM(Quantity) AS Quantity, SUM(Price) AS Price
                            FROM          dbo.ViewSealedBoxQuantities
                            GROUP BY StyleID) AS sq ON sq.StyleID = s.Id 
					  LEFT OUTER JOIN
                          (SELECT     StyleID, SUM(Quantity) AS Quantity, SUM(Price) AS Price
                            FROM          dbo.ViewOpenBoxQuantities
                            GROUP BY StyleID) AS oq ON oq.StyleID = s.Id 
					  LEFT OUTER JOIN
                          (SELECT     *
                            FROM          (SELECT     ParentASIN, Market, MarketplaceId, StyleId, ROW_NUMBER() OVER (PARTITION BY StyleId
                                                    ORDER BY Market ASC, MarketplaceId DESC, CreateDate DESC) AS Number
                            FROM          dbo.Items) AS withNumber
							WHERE     Number = 1) AS pItem ON pItem.StyleId = s.Id
					  LEFT OUTER JOIN
							(SELECT StyleId, SUM(Quantity) as Quantity, MAX(QtyStatus) AS HasManuallyQuantity FROM	
								(SELECT StyleId, Quantity, (CASE WHEN Quantity IS NOT NULL THEN 1 ELSE 0 END) as QtyStatus FROM StyleItems) as innerSi
							GROUP BY StyleId) as siStatus ON siStatus.StyleId = s.Id
WHERE     (s.Deleted = 0)